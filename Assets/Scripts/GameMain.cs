using Battle;
using Framework.UI;
using Framework.Web;
using Module;
using Module.Login;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        public static string id;

        private void Awake()
        {
            id = "";
        }
        private void Start()
        {
            NetManager.Connect("127.0.0.1", 8888);

            PanelManager.Init();
            BattleManager.Init();

            PanelManager.Open<LoginPanel>();
        }
        private void OnEnable()
        {
            NetManager.AddNetListener(NetEvent.ConnectSuccess, OnConnectSuccess);
            NetManager.AddNetListener(NetEvent.Close, OnClose);

            NetManager.AddMsgListener("MsgKick", OnMsgKick);
        }
        private void Update()
        {
            NetManager.Update();
        }
        private void OnDisable()
        {
            NetManager.RemoveNetListener(NetEvent.ConnectSuccess, OnConnectSuccess);
            NetManager.RemoveNetListener(NetEvent.Close, OnClose);

            NetManager.RemoveMsgListener("MsgKick", OnMsgKick);
        }

        //MsgEvent
        private void OnMsgKick(MsgBase msgBase)
        {
            MsgKick msg = (MsgKick)msgBase;
            if (msg.reason == 1)
            {
                PanelManager.Open<TipPanel>("[Kick] Shared Account.");
            }
            else
            {
                PanelManager.Open<TipPanel>("[Kick] No Reason.");
            }
        }


        // NetEvent
        private void OnConnectSuccess(string msg)
        {
            Debug.Log("[Connect]连接成功");
        }
        private void OnClose(string msg)
        {
            PanelManager.Open<TipPanel>("[Close]" + msg);
        }
    }
}