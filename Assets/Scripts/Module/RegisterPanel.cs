using Framework.UI;
using Framework.Web;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module
{
    public class RegisterPanel : BasePanel
    {
        private TMP_InputField idInput;
        private TMP_InputField pwInput;
        private TMP_InputField pwResetInput;
        private Button registerBtn;
        private Button backBtn;
        public override void OnInit()
        {
            base.OnInit();
            skinPath = "UI/RegisterPanel";
            layer = PanelManager.Layer.Panel;
        }
        public override void OnShow(params object[] para)
        {
            base.OnShow(para);
            idInput = skin.transform.Find("Input_ID").GetComponent<TMP_InputField>();
            pwInput = skin.transform.Find("Input_PW").GetComponent<TMP_InputField>();
            pwResetInput = skin.transform.Find("Input_pwReset").GetComponent<TMP_InputField>();
            registerBtn = skin.transform.Find("Btn_Register").GetComponent<Button>();
            backBtn = skin.transform.Find("Btn_Back").GetComponent<Button>();

            registerBtn.onClick.AddListener(OnRegisterClick);
            backBtn.onClick.AddListener(OnBackClick);

            NetManager.AddMsgListener("MsgLogin", OnMsgRegister);
        }
        public override void OnClose()
        {
            base.OnClose();
            NetManager.RemoveMsgListener("MsgLogin", OnMsgRegister);
        }

        private void OnMsgRegister(MsgBase msgBase)
        {
            MsgRegister msg = (MsgRegister)msgBase;
            if(msg.result == 0)
            {
                Debug.Log("Register Success");
                PanelManager.Open<TipPanel>("Register Success");
                Close();
            }
            else
            {
                PanelManager.Open<TipPanel>("Register fail");
            }
        }

        private void OnRegisterClick()
        {
            if(idInput.text=="" || pwInput.text=="" || pwResetInput.text=="")
            {
                PanelManager.Open<TipPanel>("text is empty");
                return;
            }
            if(pwInput.text != pwResetInput.text)
            {
                PanelManager.Open<TipPanel>("password is not same");
                return;
            }

            MsgRegister msg = new MsgRegister(idInput.text, pwInput.text);
            NetManager.Send(msg);
        }
        private void OnBackClick()
        {
            Close();
        }
    }
}