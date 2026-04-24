using Framework.UI;
using Module;
using Module.Login;
using UnityEngine;

namespace Test
{
    public class Test : MonoBehaviour
    {
        
        private void Start()
        {
            PanelManager.Open<LoginPanel>();
            PanelManager.Open<RegisterPanel>();

        }
    }
}