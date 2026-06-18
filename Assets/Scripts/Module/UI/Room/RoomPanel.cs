using Framework;
using Framework.UI;
using Framework.Web;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Room
{
    public class RoomPanel : BasePanel
    {
        private Button startBtn;
        private Button closeBtn;
        private Transform contentTF;
        private GameObject playerPF;

        public override void OnInit()
        {
            base.OnInit();

            skinPath = "UI/Room/RoomPanel";
            layer = PanelManager.Layer.Panel;
        }

        public override void OnShow(params object[] para)
        {
            base.OnShow(para);
            playerPF = ResManager.LoadPrefab("UI/Room/PlayerCell");
            contentTF = skin.transform.Find("Scroll View/Viewport/Content");
            startBtn = skin.transform.Find("startBtn").GetComponent<Button>();
            closeBtn = skin.transform.Find("closeBtn").GetComponent<Button>();

            startBtn.onClick.AddListener(OnStartButtonClick);
            closeBtn.onClick.AddListener(OnCloseButtonClick);

            NetManager.AddMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
            NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
            NetManager.AddMsgListener("MsgStartBattle", OnMsgStartBattle);

            MsgGetRoomInfo msg = new MsgGetRoomInfo(null);
            NetManager.Send(msg);
        }
        public override void OnClose()
        {
            base.OnClose();

            startBtn.onClick.RemoveListener(OnStartButtonClick);
            closeBtn.onClick.RemoveListener(OnCloseButtonClick);

            NetManager.RemoveMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
            NetManager.RemoveMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
            NetManager.RemoveMsgListener("MsgStartBattle", OnMsgStartBattle);
        }

        private void OnCloseButtonClick()
        {
            MsgLeaveRoom msgLeaveRoom = new MsgLeaveRoom(0);
            NetManager.Send(msgLeaveRoom);
        }
        private void OnMsgLeaveRoom(MsgBase msgBase)
        {
            MsgLeaveRoom msg = (MsgLeaveRoom)msgBase;
            if(msg.result == 0)
            {
                PanelManager.Open<TipPanel>("[LeaveRoom] success");
                PanelManager.Open<RoomListPanel>();
                Close();
            }
            else
            {
                PanelManager.Open<TipPanel>("[LeaveRoom] fail");
            }
        }

        private void OnStartButtonClick()
        {
            Debug.Log("[Send] MsgStartBattle");
            MsgStartBattle msgStartBattle = new MsgStartBattle(0);
            NetManager.Send(msgStartBattle);
        }
        private void OnMsgStartBattle(MsgBase msgBase)
        {
            MsgStartBattle msg = (MsgStartBattle)msgBase;
            if(msg.result == 0)
            {
                Close();
            }
            else
            {
                PanelManager.Open<TipPanel>("[Start] fail");
            }
        }

        private void OnMsgGetRoomInfo(MsgBase msgBase)
        {
            MsgGetRoomInfo msg = (MsgGetRoomInfo)msgBase;
            for(int i=contentTF.childCount-1; i>=0; --i)
            {
                GameObject go = contentTF.GetChild(i).gameObject;
                Destroy(go);
            }
            if (msg.players == null) return;
            foreach(PlayerInfo player in msg.players)
            {
                GameObject go = Instantiate(playerPF);
                go.transform.SetParent(contentTF);

                go.transform.Find("accountText").GetComponent<TMP_Text>().text = $"Account: {player.id}";
                go.transform.Find("scoreText").GetComponent<TMP_Text>().text = $"Score: WIN[{player.win}] | LOSE[{player.lose}]";

                if(player.isOwner == 1)
                {
                    go.transform.Find("teamText").GetComponent<TMP_Text>().text = $"Team: {player.camp}!";
                }
                else
                {
                    go.transform.Find("teamText").GetComponent<TMP_Text>().text = $"Team: {player.camp}";
                }
            }
        }
    }
}