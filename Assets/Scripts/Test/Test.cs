using Framework;
using ns;
using System.Collections;
using System.Collections.Generic;
using Tank;
using UnityEngine;

namespace Test
{
    public class Test : MonoBehaviour
    {
        
        private void Start()
        {
            //GameObject tank = ResManager.LoadPrefab("Low Poly German Tanks/Pz-VI-Tiger/Pz-VI Tiger");
            //GameObject GO = Instantiate(tank);

            //GameObject tankGO = new GameObject("tank");
            //BaseTank baseTank = tankGO.AddComponent<BaseTank>();
            //baseTank.Init("Low Poly German Tanks/Pz-VI-Tiger/Pz-VI Tiger");

            GameObject tankGO = new GameObject("tank");
            CtrlTank ctrlTank = tankGO.AddComponent<CtrlTank>();
            ctrlTank.Init("Tanks/Pz-VI Tiger");

            tankGO.AddComponent<Rigidbody>();
            BoxCollider boxCollider = tankGO.AddComponent<BoxCollider>();
            boxCollider.center = new Vector3(0, 1.3f, -0.15f);
            boxCollider.size = new Vector3(3.5f, 2.55f, 5.6f);

            tankGO.AddComponent<CameraFollow>();
        }
    }
}