using Framework.Web;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tank
{
    public class CtrlTank : BaseTank
    {
        private float lastSendSyncTime = 0f;
        public static float syncInterval = 0.1f;

        new void Update()
        {
            base.Update();
            MoveUpdate();
            TurretUpdate();
            FireUpdate();
            SyncUpdate();
        }

        private void SyncUpdate()
        {
            if (Time.time < lastSendSyncTime + syncInterval) return;

            lastSendSyncTime = Time.time;
            MsgSyncTank msg = new MsgSyncTank();
            msg.x = transform.position.x;
            msg.y = transform.position.y;
            msg.z = transform.position.z;
            msg.ex = transform.eulerAngles.x;
            msg.ey = transform.eulerAngles.y;
            msg.ez = transform.eulerAngles.z;
            msg.turrelY = turretTF.localEulerAngles.y;
            NetManager.Send(msg);
            Debug.Log("[Send SyncMsg]");
        }

        private void MoveUpdate()
        {
            if (IsDie()) return;
            float x = Input.GetAxis("Horizontal");
            transform.Rotate(0, x * steer * Time.deltaTime, 0);

            float y = Input.GetAxis("Vertical");
            Vector3 s = y * speed * Time.deltaTime * transform.forward;
            transform.position += s;
        }
        private void TurretUpdate()
        {
            if (IsDie()) return;
            float axis = 0;
            if(Input.GetKey(KeyCode.Q))
            {
                axis = -1;
            }
            if(Input.GetKey(KeyCode.E))
            {
                axis = 1;
            }

            Vector3 angles = turretTF.transform.localEulerAngles;
            angles.y = angles.y + axis * turretSpeed * Time.deltaTime;
            turretTF.transform.localEulerAngles = angles;
        }
        private void FireUpdate()
        {
            if (IsDie()) return;
            if(Input.GetKeyDown(KeyCode.Space) && Time.time-lastFireTime > fireCD)
            {
                Bullet bullet = Fire();
                MsgFire msg = new MsgFire();
                msg.x = bullet.transform.position.x;
                msg.y = bullet.transform.position.y;
                msg.z = bullet.transform.position.z;
                msg.ex = bullet.transform.eulerAngles.x;
                msg.ey = bullet.transform.eulerAngles.y;
                msg.ez = bullet.transform.eulerAngles.z;
                NetManager.Send(msg);
            }
        }
    }
}