using Framework.UI;
using Framework.Web;
using Module;
using Module.Login;
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
            PanelManager.Init();
            PanelManager.Open<LoginPanel>();
        }
        private void OnEnable()
        {
            NetManager.AddNetListener(NetEvent.Close, OnClose);
            NetManager.AddMsgListener("MsgKick", OnMsgKick);
        }
        private void Update()
        {
            NetManager.Update();
        }
        private void OnDisable()
        {
            NetManager.RemoveNetListener(NetEvent.Close, OnClose);
            NetManager.RemoveMsgListener("MsgKick", OnMsgKick);
        }

        private void OnMsgKick(MsgBase msgBase)
        {
            PanelManager.Open<TipPanel>("Kicked.");
        }
        private void OnClose(string msg)
        {
            
        }
    }
}