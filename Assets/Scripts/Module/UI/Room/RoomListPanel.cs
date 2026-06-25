using Framework;
using Framework.UI;
using Framework.Web;
using Game;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Room
{
    public class RoomListPanel : BasePanel
    {
        private Transform contentTF;
        private GameObject roomGO;

        private TMP_Text idText;
        private TMP_Text scoreText;
        private Button createBtn;
        private Button reflashBtn;
        public override void OnInit()
        {
            base.OnInit();

            skinPath = "UI/Room/RoomListPanel";
            layer = PanelManager.Layer.Panel;
        }

        public override void OnShow(params object[] para)
        {
            base.OnShow(para);
            contentTF = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
            roomGO = ResManager.LoadPrefab("UI/Room/RoomCell");

            idText = skin.transform.Find("InfoPanel/Content/idText").GetComponent<TMP_Text>();
            scoreText = skin.transform.Find("InfoPanel/Content/scoreText").GetComponent<TMP_Text>();
            createBtn = skin.transform.Find("CtrlPanel/Content/createBtn").GetComponent<Button>();
            reflashBtn = skin.transform.Find("CtrlPanel/Content/updateBtn").GetComponent<Button>();

            idText.text = GameMain.id;

            createBtn.onClick.AddListener(OnCreateBtnClick);
            reflashBtn.onClick.AddListener(OnReflashBtnClick);

            NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
            NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
            NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);
            NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);

            MsgGetAchieve msgGetAchieve = new MsgGetAchieve(0, 0);
            MsgGetRoomList msgGetRoomList = new MsgGetRoomList(null);
            NetManager.Send(msgGetAchieve);
            NetManager.Send(msgGetRoomList);
        }

        public override void OnClose()
        {
            base.OnClose();

            createBtn.onClick.RemoveListener(OnCreateBtnClick);
            reflashBtn.onClick.RemoveListener(OnReflashBtnClick);

            NetManager.RemoveMsgListener("MsgGetAchieve", OnMsgGetAchieve);
            NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
            NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);
            NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        }

        private void OnReflashBtnClick()
        {
            MsgGetRoomList msg = new MsgGetRoomList(null);
            NetManager.Send(msg);
        }
        private void OnMsgGetRoomList(MsgBase msgBase)
        {
            MsgGetRoomList msg = (MsgGetRoomList)msgBase;
            for(int i=contentTF.childCount-1; i>=0; --i)
            {
                GameObject go = contentTF.GetChild(i).gameObject;
                Destroy(go);
            }

            if (msg.rooms == null) return;

            foreach(RoomInfo room in msg.rooms)
            {
                GenerateRoom(room);
            }
        }

        private void OnCreateBtnClick()
        {
            MsgCreateRoom msg = new MsgCreateRoom(0);
            NetManager.Send(msg);
        }
        private void OnMsgCreateRoom(MsgBase msgBase)
        {
            MsgCreateRoom msg = (MsgCreateRoom)msgBase;
            if(msg.result == 0)
            {
                PanelManager.Open<TipPanel>("[CreateRoom] success");
                PanelManager.Open<RoomPanel>();
                Close();
            }
            else
            {
                PanelManager.Open<TipPanel>("[Create Room] fail");
            }
        }

        private void OnEnterRoomClick(string name)
        {
            MsgEnterRoom msg = new MsgEnterRoom(int.Parse(name), 0);
            NetManager.Send(msg);
        }
        private void OnMsgEnterRoom(MsgBase msgBase)
        {
            MsgEnterRoom msg = (MsgEnterRoom)msgBase;
            if(msg.result == 0)
            {
                PanelManager.Open<RoomPanel>();
                Close();
            }
            else
            {
                PanelManager.Open<TipPanel>("[Enter Room] fail");
            }
        }

        private void OnMsgGetAchieve(MsgBase msgBase)
        {
            MsgGetAchieve msg = (MsgGetAchieve)msgBase;
            scoreText.text = $"WIN[{msg.win}] - LOSE[{msg.lose}]";
        }

        private void GenerateRoom(RoomInfo room)
        {
            GameObject newRoom = Instantiate(roomGO);
            newRoom.transform.SetParent(contentTF, false);

            newRoom.transform.Find("numText").GetComponent<TMP_Text>().text = $"Num: {room.id}";
            newRoom.transform.Find("peopleText").GetComponent<TMP_Text>().text = $"People: {room.count}";
            newRoom.transform.Find("stateText").GetComponent<TMP_Text>().text = $"State: {room.status}";
            Button joinBtn = newRoom.transform.Find("JoinBtn").GetComponent<Button>();

            joinBtn.name = room.id.ToString();
            joinBtn.onClick.AddListener(() => { OnEnterRoomClick(joinBtn.name); });
        }


        
    }
}