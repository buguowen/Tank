using Framework;
using Framework.UI;
using Framework.Web;
using Game;
using Proto;
using Tank;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Login
{
    public class LoginPanel : BasePanel
    {
        private TMP_InputField idInput;
        private TMP_InputField pwInput;
        private Button loginBtn;
        private Button registerBtn;
        public override void OnInit()
        {
            skinPath = "UI/LoginPanel";
            layer = PanelManager.Layer.Panel;
        }
        public override void OnShow(params object[] para)
        {

            idInput = skin.transform.Find("Input_ID").GetComponent<TMP_InputField>();
            pwInput = skin.transform.Find("Input_PW").GetComponent<TMP_InputField>();
            loginBtn = skin.transform.Find("Btn_Login").GetComponent<Button>();
            registerBtn = skin.transform.Find("Btn_Register").GetComponent<Button>();

            loginBtn.onClick.AddListener(OnLoginClick);
            registerBtn.onClick.AddListener(OnRegisterClick);

            NetManager.AddNetListener(NetEvent.ConnectSuccess, OnConnectSuccess);
            NetManager.AddNetListener(NetEvent.ConnectFail, OnConnectFail);

            NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        }
        public override void OnClose()
        {
            NetManager.RemoveNetListener(NetEvent.ConnectSuccess, OnConnectSuccess);
            NetManager.RemoveNetListener(NetEvent.ConnectFail, OnConnectFail);
            NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
        }

        private void OnLoginClick()
        {
            if(idInput.text == "" || pwInput.text == "")
            {
                PanelManager.Open<TipPanel>("ID and PW is empty.");
                return;
            }
            MsgLogin msgLogin = new MsgLogin(idInput.text, pwInput.text);
            NetManager.Send(msgLogin);
        }
        private void OnRegisterClick()
        {
            PanelManager.Open<RegisterPanel>();
        }

        private void OnMsgLogin(MsgBase msgBase)
        {
            MsgLogin msg = (MsgLogin)msgBase;

            if(msg.result != 0)
            {
                PanelManager.Open<TipPanel>("[Login] fail");
                return;
            }

            Debug.Log("[Login] success");
            GameMain.id = msg.id;
            GameObject tank = new GameObject("tank");
            CtrlTank ctrlTank = tank.AddComponent<CtrlTank>();
            ctrlTank.Init("Tanks/Pz-VI Tiger");
            tank.AddComponent<CameraFollow>();
            Close();
        }

        private void OnConnectSuccess(string msg)
        {
            Debug.Log("[Connect] success");
        }
        private void OnConnectFail(string msg)
        {
            PanelManager.Open<TipPanel>(msg);
        }


    }
}