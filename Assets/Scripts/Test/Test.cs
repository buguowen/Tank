using Framework.UI;
using Framework.Web;
using Module;
using Module.Login;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class Test : MonoBehaviour
    {
        public Button connectBtn;
        private void Start()
        {
            connectBtn.onClick.AddListener(OnConnectClick);

            NetManager.AddNetListener(NetEvent.ConnectSuccess, OnConnectSuccess);
            NetManager.AddNetListener(NetEvent.ConnectFail, OnConnectFail);
            NetManager.AddNetListener(NetEvent.Close, OnCLose);

        }

        private void Update()
        {
            NetManager.Update();
        }

        //Event
        private void OnCLose(string msg)
        {
            Debug.Log("[Close] 断开连接 " + msg);
        }
        private void OnConnectFail(string msg)
        {
            Debug.Log("[Connect] 连接失败: " + msg);
        }
        private void OnConnectSuccess(string msg)
        {
            Debug.Log("[Connect] 连接成功.");
        }
        private void OnConnectClick()
        {
            NetManager.Connect("127.0.0.1", 8888);
        }
        //Msg
    }
}