using Framework;
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

        public float speed = 5f;

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
            hitTank.Attacked(50);
            //GameObject explosionPrefab = ResManager.LoadPrefab("explosion");
            //Instantiate(explosionPrefab);
            Debug.Log("Explosion!");
            Destroy(gameObject);
        }
    }
}