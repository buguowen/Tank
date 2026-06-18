using Proto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Framework.Web
{
    public delegate void NetEventListener(string msg);
    public delegate void MsgEventListener(MsgBase msgBase);  // 处理协议, 需要协议信息->传递MsgBase
    public enum NetEvent
    {
        ConnectSuccess,
        ConnectFail,
        Close
    }
    public static class NetManager 
    {
        private static Socket _socket;

        // 状态
        private static bool isConnecting;
        private static bool isClosing;
        // 发送
        private static Queue<ByteArray> sendQueue;
        // 接收
        private static ByteArray readBuff;
        private static Queue<MsgBase> msgQueue;
        private static int MAX_MSG_HANDLER_COUNT;
        // 事件
        private static Dictionary<NetEvent, NetEventListener> netListeners;
        private static Dictionary<string, MsgEventListener> msgListeners;

        private struct NetEventState
        {
            public NetEvent netEvent;
            public string msg;
        }
        private static Queue<NetEventState> netEventQueue = new Queue<NetEventState>();

        private static float lastPingTime;
        private static float lastPongTime;
        private static int pingInterval;

        // 初始化
        static NetManager()
        {
            netListeners = new Dictionary<NetEvent, NetEventListener>();
            msgListeners = new Dictionary<string, MsgEventListener>();
            MAX_MSG_HANDLER_COUNT = 10;
            pingInterval = 5;
            AddMsgListener("MsgPong", OnMsgPong);

            // 强制全局 Newtonsoft.Json 使用 InvariantCulture 以支持跨区域浮点数（包括科学计数法）正确解析
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture
            };
        }

        private static void InitState()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            isConnecting = false;
            isClosing = false;

            sendQueue = new Queue<ByteArray>();
            readBuff = new ByteArray(1024);
            msgQueue = new Queue<MsgBase>();
            netEventQueue = new Queue<NetEventState>();

            lastPingTime = Time.time;
            lastPongTime = Time.time;
        }

        // 对外接口
        public static void Connect(string ip, int port)
        {
            if(isConnecting)
            {
                Debug.Log("[Connect] 正在连接");
                return;
            }
            if(_socket!=null && _socket.Connected)
            {
                Debug.Log("[Connect] 已经连接");
                return;
            }

            Debug.Log("[Connect] 开始连接...");
            InitState();
            isConnecting = true;
            _socket.BeginConnect(ip, port, ConnectCallBack, _socket);
        }
        public static void Close()
        {
            if(isClosing) return;
            if (_socket == null || !_socket.Connected) return;

            if(sendQueue.Count > 0)
            {
                isClosing = true;
            }
            else
            {
                _socket.Close();
                FireNetEvent(NetEvent.Close, "");
            }
        }
        public static void CloseSocketImmediate()
        {
            if (_socket == null) return;
            try
            {
                _socket.Close();
            }
            catch (Exception ex)
            {
                Debug.Log("[CloseSocketImmediate] Error: " + ex.ToString());
            }
            isClosing = false;
            isConnecting = false;
            FireNetEvent(NetEvent.Close, "Protocol Error");
        }
        public static void Send(MsgBase msgBase)
        {
            if (_socket == null || !_socket.Connected) return;
            if (isConnecting) return;
            if (isClosing) return;

            byte[] bytes = MsgBase.Encode(msgBase);
            ByteArray byteArray = new ByteArray(bytes);

            int count = 0;
            lock(sendQueue)
            {
                sendQueue.Enqueue(byteArray);
                count = sendQueue.Count;
            }

            if(count == 1)
            {
                _socket.BeginSend(byteArray.bytes, byteArray.readIdx, byteArray.Length, 0, SendCallBack, _socket);
            }

        }
        public static void Update()
        {
            NetEventUpdate();
            if (_socket == null || !_socket.Connected) return;
            MsgUpdate();
            //PingUpdate();
        }
        private static void NetEventUpdate()
        {
            while (true)
            {
                bool hasEvent = false;
                NetEventState state = default;
                lock (netEventQueue)
                {
                    if (netEventQueue.Count > 0)
                    {
                        state = netEventQueue.Dequeue();
                        hasEvent = true;
                    }
                }
                if (hasEvent)
                {
                    if (netListeners.ContainsKey(state.netEvent) && netListeners[state.netEvent] != null)
                    {
                        netListeners[state.netEvent](state.msg);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public static void AddNetListener(NetEvent netEvent, NetEventListener listener)
        {
            if(netListeners.ContainsKey(netEvent))
            {
                netListeners[netEvent] += listener;
            }
            else
            {
                netListeners.Add(netEvent, listener);
            }
        }
        public static void RemoveNetListener(NetEvent netEvent, NetEventListener listener)
        {
            if(!netListeners.ContainsKey(netEvent))
            {
                return;
            }
            netListeners[netEvent] -= listener;
            if(netListeners[netEvent] == null)
            {
                netListeners.Remove(netEvent);
            }
        }
        public static void FireNetEvent(NetEvent netEvent, string msg)
        {
            lock(netEventQueue)
            {
                netEventQueue.Enqueue(new NetEventState { netEvent = netEvent, msg = msg });
            }
        }
       
        public static void AddMsgListener(string name, MsgEventListener listener)
        {
            if (msgListeners.ContainsKey(name))
            {
                msgListeners[name] += listener;
            }
            else
            {
                msgListeners.Add(name, listener);
            }
        }
        public static void RemoveMsgListener(string name, MsgEventListener listener)
        {
            if (!msgListeners.ContainsKey(name))
            {
                return;
            }
            msgListeners[name] -= listener;
            if (msgListeners[name] == null)
            {
                msgListeners.Remove(name);
            }
        }
        public static void FireMsgEvent(string name, MsgBase msgBase)
        {
            if (!msgListeners.ContainsKey(name)) return;
            if (msgListeners[name] == null) return;

            Console.WriteLine("[FireMsgEvent] " + name);
            msgListeners[name](msgBase);
        }
        
        // 私有
        private static void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);

                isConnecting = false;
                FireNetEvent(NetEvent.ConnectSuccess, "");
                
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0, ReceiveCallBack, socket);
            }
            catch(SocketException ex)
            {
                isConnecting = false;
                FireNetEvent(NetEvent.ConnectFail, ex.ToString());
            }
        }
        private static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar);

                ByteArray ba;
                lock(sendQueue)
                {
                    ba = sendQueue.Peek();
                }
                ba.readIdx += count;

                if(ba.Length <= 0)
                {
                    lock(sendQueue)
                    {
                        sendQueue.Dequeue();
                        ba = sendQueue.Count > 0 ? sendQueue.Peek() : null;
                    }
                }

                if(ba != null)
                {
                    socket.BeginSend(ba.bytes, ba.readIdx, ba.Length, 0, SendCallBack, socket);
                }
                else if(isClosing)
                {
                    socket.Close();
                    FireNetEvent(NetEvent.Close, "");
                }
            }
            catch(SocketException ex)
            {
                Debug.Log("[Send] fail: " + ex.ToString());
            }
        }
        private static void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndReceive(ar);
                
                if(count == 0)
                {
                    Close();
                    return;
                }

                readBuff.writeIdx += count;
                OnReceiveData();

                if (_socket == null || !_socket.Connected || socket != _socket || isClosing)
                {
                    return;
                }

                //?
                if(readBuff.Remain < 8)
                {
                    readBuff.MoveBytes();
                    readBuff.CheckAndExpand(readBuff.Length * 2);
                }
                
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0, ReceiveCallBack, socket);
            }
            catch(Exception ex)
            {
                Debug.Log("[Receive] fail: " + ex.ToString());
            }
        }
        private static void OnReceiveData()
        {
            if (_socket == null || !_socket.Connected || isClosing) return;
            if (readBuff.Length <= 2) return;

            Int16 bodylen = (Int16)(readBuff.bytes[readBuff.readIdx+1] << 8 | readBuff.bytes[readBuff.readIdx]);
            if (bodylen <= 0 || bodylen > 8192)
            {
                string hex = "";
                int printLen = Math.Min(64, readBuff.Length);
                for (int i = 0; i < printLen; i++)
                {
                    hex += readBuff.bytes[readBuff.readIdx + i].ToString("X2") + " ";
                }
                Debug.LogError("[OnReceiveData Protocol Error] bodylen = " + bodylen + ", Length = " + readBuff.Length + ", Hex: " + hex + ". Force closing socket.");
                CloseSocketImmediate();
                return;
            }
            if (readBuff.Length < bodylen + 2) return;
            readBuff.readIdx += 2;

            string protoName;
            MsgBase msg = MsgBase.Decode(readBuff.bytes, readBuff.readIdx, bodylen, out protoName);
            if (msg == null)
            {
                string hex = "";
                int printLen = Math.Min(bodylen + 2, readBuff.Length);
                for (int i = 0; i < printLen; i++)
                {
                    hex += readBuff.bytes[readBuff.readIdx - 2 + i].ToString("X2") + " ";
                }
                Debug.LogError("[OnReceiveData Decode Error] protoName = " + protoName + ", Hex: " + hex + ". Force closing socket.");
                CloseSocketImmediate();
                return;
            }
            readBuff.readIdx += bodylen;

            if(msg != null)
            {
                lock(msgQueue)
                {
                    Console.WriteLine($"msgQueue.Enqueue: [ProtoName] {msg.ProtoName}");
                    msgQueue.Enqueue(msg);
                }
            }
            
            OnReceiveData();
        }

        // 每帧更新
        private static void MsgUpdate()
        {
            for(int i=0; i<MAX_MSG_HANDLER_COUNT; ++i)
            {
                MsgBase msg = null;
                lock(msgQueue)
                {
                    if(msgQueue.Count > 0)
                    {
                        msg = msgQueue.Dequeue();
                    }
                }
                if(msg != null)
                {
                    Console.WriteLine($"msgQueue.Dequeue: [ProtoName] {msg.ProtoName}");
                    FireMsgEvent(msg.ProtoName, msg);
                }
            }
        }
        private static void PingUpdate()
        {
            if (_socket == null || !_socket.Connected) return;

            if(Time.time > lastPongTime + pingInterval*4)
            {
                Close();
                return;
            }

            if(Time.time > lastPingTime + pingInterval)
            {
                MsgPing pingMsg = new MsgPing();
                Send(pingMsg);

                lastPingTime = Time.time;
            }
        }

        //MsgEvent
        private static void OnMsgPong(MsgBase msgBase)
        {
            lastPongTime = Time.time;
        }
    }
}