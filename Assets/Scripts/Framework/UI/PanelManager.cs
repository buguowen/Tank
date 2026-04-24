using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Framework.UI
{
    public static class PanelManager
    {
        public enum Layer
        {
            Panel,
            Tip
        }

        private static Dictionary<Layer, Transform> layers;
        private static Dictionary<string, BasePanel> panels;
        private static Transform root;
        private static Transform canvas;
        static PanelManager()
        {
            layers = new Dictionary<Layer, Transform>();
            panels = new Dictionary<string, BasePanel>();
        }
        
        public static void Init()
        {
            root = GameObject.Find("Root").transform;
            canvas = root.Find("Canvas");
            Transform panel = canvas.Find("Panel");
            Transform tip = canvas.Find("Tip");

            layers.Add(Layer.Panel, panel);
            layers.Add(Layer.Tip, tip);

        }
        public static void Open<T>(params object[] para) where T:BasePanel
        {
            string name = typeof(T).ToString();
            if(panels.ContainsKey(name))
            {
                return;
            }

            BasePanel panel = root.AddComponent<T>();
            panel.OnInit();
            panel.Init();

            Transform layer = layers[panel.Layer];
            panel.skin.transform.SetParent(layer, false);

            panels.Add(name, panel);
            panel.OnShow(para);
        }
        public static void Close(string name)
        {
            if(!panels.ContainsKey(name))
            {
                return;
            }
            BasePanel panel = panels[name];
            panel.OnClose();
            panels.Remove(name);

            GameObject.Destroy(panel.skin);
            GameObject.Destroy(panel);
        }
   
    }
}