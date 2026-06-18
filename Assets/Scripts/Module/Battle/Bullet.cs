using Framework;
using Framework.Web;
using Game;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Tank
{
    public class Bullet : MonoBehaviour
    {
        public BaseTank tank;
        private GameObject skinGO;
        private Rigidbody bulletRB;

        public float speed = 50f;

        public void Init(string skinPath)
        {
            GameObject skin = ResManager.LoadPrefab(skinPath);
            skinGO = Instantiate(skin);
            skinGO.transform.SetParent(this.transform);
            skinGO.transform.localPosition = Vector3.zero;
            skinGO.transform.localRotation = Quaternion.identity;
            bulletRB = this.AddComponent<Rigidbody>(); 
            bulletRB.useGravity = false;
        }
        private void Update()
        {
            transform.position = transform.position + transform.forward * speed * Time.deltaTime;
        }
        private void OnCollisionEnter(Collision hit)
        {
            BaseTank hitTank = hit.gameObject.GetComponent<BaseTank>();
            if (hitTank == null || hitTank == tank) return;

            SendMsgHit(tank, hitTank);
            //hitTank.Attacked(50);

            GameObject explosionPrefab = ResManager.LoadPrefab("explosion");
            Instantiate(explosionPrefab, hit.transform.position + Vector3.up*5, Quaternion.identity);
            Debug.Log("Explosion!");
            Destroy(gameObject);
        }

        private void SendMsgHit(BaseTank tank, BaseTank hitTank)
        {
            if (tank == null || hitTank == null) return;
            if (tank.id != GameMain.id) return;

            MsgHit msg = new MsgHit();
            msg.targetId = hitTank.id;
            msg.id = tank.id;
            msg.x = transform.position.x;
            msg.y = transform.position.y;
            msg.z = transform.position.z;
            NetManager.Send(msg);
        }
    }
}