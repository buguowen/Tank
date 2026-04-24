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
        static NetManager()
        {
            netListeners = new Dictionary<NetEvent, NetEventListener>();
            msgListeners = new Dictionary<string, MsgEventListener>();
            MAX_MSG_HANDLER_COUNT = 10;
        }

        // 公开方法
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

            InitState();
            _socket.BeginConnect(ip, port, ConnectCallBack, _socket);
            isConnecting = true;
        }
        public static void Close()
        {
            if(isClosing)    
            {
                return;
            }
            if(_socket == null || !_socket.Connected)
            {
                return;
            }

            if(sendQueue.Count == 0)
            { 
                _socket.Close();
                FireNetEvent(NetEvent.Close, "");
            }
            else
            {
                isClosing = true;
            }
        }
        public static void Send(MsgBase msgBase)
        {
            if (_socket == null || !_socket.Connected) return;
            if (isConnecting) return;
            if (isClosing) return;

            byte[] bytes = MsgBase.Encode(msgBase);
            ByteArray byteArray = new ByteArray(bytes);
            sendQueue.Enqueue(byteArray);

            if(sendQueue.Count == 1)
            {
                _socket.BeginSend(byteArray.bytes, byteArray.readIdx, byteArray.Size, 0, SendCallBack, _socket);
            }

        }
        public static void Update()
        {
            MsgUpdate();
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

            netListeners[netEvent].Invoke(msg);
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

            msgListeners[name].Invoke(msgBase);
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
                FireNetEvent(NetEvent.ConnectFail, ex.ToString());
                isConnecting = false;
            }
        }
        private static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar);
                ByteArray byteArray = sendQueue.Peek();
                byteArray.readIdx += count;

                if(byteArray.Size <= 0)
                { 
                    sendQueue.Dequeue();
                    if (sendQueue.Count == 0)
                    {
                        //if(isClosing) 
                        //{
                        //    socket.Close();
                        //    FireNetEvent(NetEvent.Close, "");
                        //}
                        //return;
                        byteArray = null;
                    }
                    else 
                    {
                        byteArray = sendQueue.Peek(); 
                    }
                }
                if(byteArray != null)
                {
                    socket.BeginSend(byteArray.bytes, byteArray.readIdx, byteArray.Size, 0, SendCallBack, _socket);
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

                if(readBuff.Remain < count)
                {
                    readBuff.Expand(readBuff.Size + count);
                }
                else if(readBuff.capacity-readBuff.writeIdx < count)
                {
                    readBuff.MoveBytes();
                }
                socket.BeginReceive(readBuff.bytes, readBuff.readIdx, readBuff.Remain, 0, ReceiveCallBack, socket);
            }
            catch(SocketException ex)
            {
                Debug.Log("[Receive] fail: " + ex.ToString());
            }
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

        }
        private static void OnReceiveData()
        {
            if(readBuff.Size <= 2)
            {
                return;
            }

            byte[] bytes = readBuff.bytes;
            Int16 len = (Int16)(bytes[readBuff.readIdx+1] << 8 | bytes[readBuff.readIdx]);
            if(len > bytes.Length)
            {
                return;
            }

            string protoName;
            MsgBase msgBase = MsgBase.Decode(readBuff.bytes, readBuff.readIdx + 2, len, out protoName);

            readBuff.readIdx += (len + 2);

            msgQueue.Enqueue(msgBase);

            OnReceiveData();
        }
        private static void MsgUpdate()
        {
            for(int i=0; i<MAX_MSG_HANDLER_COUNT; ++i)
            {
                if (msgQueue.Count == 0) return;

                MsgBase msgBase = msgQueue.Dequeue();
                FireMsgEvent(msgBase.ProtoName, msgBase);
            }
        }
        
    }
}