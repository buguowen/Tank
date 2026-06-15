
using Framework.UI;
using Module.Room;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{

    public class ResultPanel : BasePanel
    {
        private TMP_Text resultText;
        private Button okBtn;

        public override void OnInit()
        {
            base.OnInit();

            skinPath = "UI/ResultPanel";
            layer = PanelManager.Layer.Tip;
        }

        public override void OnShow(params object[] para)
        {
            base.OnShow(para);

            resultText = skin.transform.Find("Result_Text").GetComponent<TMP_Text>();
            okBtn = skin.transform.Find("Button_OK").GetComponent<Button>();

            bool isWin = (bool)para[0];
            resultText.text = isWin ? "WIN" : "LOSE";

            okBtn.onClick.AddListener(OnOkClick);
        }

        private void OnOkClick()
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
    }
}