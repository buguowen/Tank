using Proto;
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
                _socket.BeginSend(byteArray.bytes, byteArray.readIdx, byteArray.Remain, 0, SendCallBack, _socket);
            }

        }
        public static void Update()
        {
            if (_socket == null || !_socket.Connected) return;
            MsgUpdate();
            //PingUpdate();
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
            if (!netListeners.ContainsKey(netEvent)) return;
            if (netListeners[netEvent] == null) return;

            netListeners[netEvent](msg);
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

                //?
                if(readBuff.Remain < 8)
                {
                    readBuff.MoveBytes();
                    readBuff.CheckAndExpand(readBuff.Length * 2);
                }
                
                socket.BeginReceive(readBuff.bytes, readBuff.readIdx, readBuff.Remain, 0, ReceiveCallBack, socket);
            }
            catch(SocketException ex)
            {
                Debug.Log("[Receive] fail: " + ex.ToString());
            }
        }
        private static void OnReceiveData()
        {
            if (readBuff.Length <= 2) return;

            Int16 bodylen = (Int16)(readBuff.bytes[readBuff.readIdx+1] << 8 | readBuff.bytes[readBuff.readIdx]);
            if (bodylen > readBuff.bytes.Length) return;
            readBuff.readIdx += 2;

            string protoName;
            MsgBase msg = MsgBase.Decode(readBuff.bytes, readBuff.readIdx, bodylen, out protoName);
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