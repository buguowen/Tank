using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Framework.UI.PanelManager;

namespace Framework.UI
{
    public class BasePanel : MonoBehaviour
    {
        protected string skinPath;
        public  GameObject skin;
        protected Layer layer;
        public Layer Layer => layer;
        public GameObject Skin => skin;

        public void Init()
        {
            GameObject prefab = ResManager.LoadPrefab(skinPath);
            skin = Instantiate(prefab);
        }
        public void Close()
        {
            string name = this.GetType().ToString();
            PanelManager.Close(name);
        }
        public virtual void OnInit()
        {

        }
        public virtual void OnShow(params object[] para)
        {

        }
        public virtual void OnClose()
        {

        }
    }
}