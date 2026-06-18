using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tank
{
    public class BaseTank : MonoBehaviour
    {
        protected GameObject skin; //Model

        // Speed
        protected float steer = 50f;
        protected float speed = 10f;

        // Turret
        protected float turretSpeed = 30f;
        protected Transform turretTF;
        protected Transform gunTF;
        protected Transform firePoint;

        // Fire
        protected float fireCD = 1f;
        protected float lastFireTime = 0f;

        // HP
        public int hp = 100;
        protected bool isDied = false;

        public string id = "";
        public int camp = 0;

        protected void Start()
        {

        }
        protected void Update()
        {
        }
        public virtual void Init(string skinPath)
        {
            GameObject go = ResManager.LoadPrefab(skinPath);
            skin = Instantiate(go);
            skin.transform.SetParent(this.transform);
            skin.transform.localPosition = Vector3.zero;
            skin.transform.localRotation = Quaternion.identity;
            skin.transform.localScale = Vector3.one;

            turretTF = skin.transform.Find("Turret");
            gunTF = skin.transform.Find("Turret/Barrel");
            firePoint = skin.transform.Find("Turret/Barrel/FirePoint");
        }
        protected Bullet Fire()
        {
            GameObject bulletGO = new GameObject("bullet");
            Bullet bullet = bulletGO.AddComponent<Bullet>();
            bullet.Init("bullet");
            bullet.tank = this;

            bulletGO.transform.position = firePoint.position;
            bulletGO.transform.rotation = firePoint.rotation;
            lastFireTime = Time.time;
            return bullet;
        }
        protected bool IsDie()
        {
            return isDied;
        }
        public void Attacked(int attackHp)
        {
            hp -= attackHp;
            if(hp<= 0)
            {
                Debug.Log("Explosion");
                isDied = true; 
            }
        }
    }
}