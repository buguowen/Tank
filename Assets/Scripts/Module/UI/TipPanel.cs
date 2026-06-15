using Framework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Module
{
    public class TipPanel : BasePanel
    {
        private Button backBtn;
        private TMP_Text contentText;
        public override void OnInit()
        {
            base.OnInit();
            skinPath = "UI/TipPanel";
            layer = PanelManager.Layer.Tip;
        }
        public override void OnShow(params object[] para)
        {
            base.OnShow(para);
            backBtn = skin.transform.Find("Btn_Back").GetComponent<Button>();
            contentText = skin.transform.Find("Text_Content").GetComponent<TMP_Text>();

            if(para.Length == 1)
                contentText.text = para[0].ToString();

            backBtn.onClick.AddListener(OnBackClick);
        }

        private void OnBackClick()
        {
            Close();
        }
    }
}