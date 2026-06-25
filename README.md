# Tank 3D 联机对战坦克客户端 (Unity)

本项目是一个基于 Unity 引擎开发的 3D 联机对战坦克游戏客户端。该项目实现了完整的客户端-服务端双端联机对战闭环，包含登录注册、房间大厅管理、实时状态同步战斗以及平滑预测算法等核心要素。

---

## 演示

### 1. 注册登录

![Login_Register](E:\Project\GameDevelopment\Tank\gif\Login_Register.gif)

### 2. 创建房间

![CreateRoom](E:\Project\GameDevelopment\Tank\gif\CreateRoom.gif)

### 3. 加入房间

![JoinRoom](E:\Project\GameDevelopment\Tank\gif\JoinRoom.gif)

### 4. 战斗和胜利结算

![Battle](E:\Project\GameDevelopment\Tank\gif\Battle.gif)



## 目录

1. [网络模块设计](#1-网络模块设计)
2. [UI 框架设计](#2-ui-框架设计)
3. [核心玩法与业务流](#3-核心玩法与业务流)
4. [网络同步与平滑算法](#4-网络同步与平滑算法)

## 1. 网络模块设计

网络架构位于命名空间 `Framework.Web` 内，基于 C# Socket (TCP) 构建了高性能分帧异步收发模块。

### 1.1 NetManager (网络核心管理器)
- **非阻塞 Socket**：使用异步 Socket (`BeginConnect`, `BeginReceive`, `BeginSend`) 实现高性能低延迟收发。
- **消息发送队列**：维护 `sendQueue` 队列，发送大包时对未发送完的字节累加 `sendIdx` 进行分片发送，规避高频并发时阻塞主线程。
- **浮点数跨地区解析**：强制全局设置 `JsonConvert.DefaultSettings` 使用 `CultureInfo.InvariantCulture`，解决不同语言操作系统环境下（如德法等逗号分隔符地区）浮点数反序列化失效的问题。

### 1.2 ByteArray (高性能滑动字节缓冲区)
- **动态扩容**：支持检测可用缓冲区容量 `Remain`，当空间不足时自动扩展至 `Length * 2`。
- **滑动压缩 (MoveBytes)**：通过 `Array.Copy` 对已读取的历史缓存进行前向覆盖滑动，重置读写指针（`readIdx` 与 `writeIdx`）。

### 1.3 MsgBase (网络协议格式与编解码)
- **协议帧结构**：
  ```text
  ┌──────────────────┬──────────────────┬──────────────┬──────────────┐
  │ 协议总长度 (2B)  │ 协议名长度 (2B)  │ 协议名 (UTF8) │ JSON Payload │
  └──────────────────┴──────────────────┴──────────────┴──────────────┘
  ```
- **序列化方案**：底层选用高执行效率的 `Newtonsoft.Json` 反序列化为反射得到的具体协议类。

---

## 2. UI 框架设计

基于松耦合思想构建的 UI 管理系统。

### 2.1 PanelManager (面板生命周期管理器)
- **动态加载**：利用 `Resources` 动态加载各面板 Prefab 并缓存。
- **层级管理**：划分底层、中层、顶层，保证 UI 渲染遮罩层次符合人类工程学。
- **全局通知**：提供统一的 `Open<T>` 与 `Close` 接口维护活跃 UI 面板列表。

### 2.2 BasePanel (基础面板生命周期)
- **Init**：面板创建时执行一次，用于获取 UI 节点引用并注册 UI 交互事件。
- **OnOpen / OnClose**：打开与关闭面板时的过渡动画、网络事件监听绑定与解除。

---

## 3. 核心玩法与业务流

整个客户端包含三个主要场景/业务环节：
- **登录注册**：与后台 MySQL 数据库打通，实现账号认证与冲突校验。
- **房间大厅**：支持创建房间、刷新房间列表、进入房间、阵营切换。房主拥有发起战斗（`MsgStartBattle`）的特权。
- **实时战斗**：坦克操纵移动（车身旋转与前后进）、炮台水平偏转（Q/E 键键入）以及发射炮弹。

---

## 4. 网络同步与平滑算法

为应对网络波动与延迟，确保在 100ms 同步周期（`syncInterval = 0.1f`）下体验丝滑：

### 4.1 线性预测平滑插值 (SyncTank)
- 远端坦克（`SyncTank`）不直接瞬移，而是通过接收到的 `MsgSyncTank` 状态包，计算差分得出预测速度。
- 利用 `Vector3.Lerp` 和 `Quaternion.Lerp` 将当前帧位置平滑过渡到预测的终点坐标（`ForecastUpdate`）。

### 4.2 炮台旋转同步 (localEulerAngles)
- 发送端发送炮台相对于车身的局部偏转角（`localEulerAngles.y`），接收端同步更新其局部角度，使炮台在坦克身体翻越坡面或旋转时，保持正确的相对指向。
