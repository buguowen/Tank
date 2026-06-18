
using Proto;
using UnityEngine;

namespace Tank
{
    public class SyncTank : BaseTank
    {
        private Vector3 lastPos;
        private Vector3 lastRot;
        private Vector3 forecastPos;
        private Vector3 forecastRot;
        private float forecastTime; //上次收到同步包的时间

        public override void Init(string skinPath)
        {
            base.Init(skinPath);

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.useGravity = false;

            lastPos = transform.position;
            lastRot = transform.eulerAngles;
            forecastPos = transform.position;
            forecastRot = transform.eulerAngles;
            forecastTime = Time.time;
        }
        private new void Update()
        {
            base.Update();

            ForecastUpdate();
        }

        public void SyncPos(MsgSyncTank msg)
        {
            Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
            Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
            forecastPos = pos + 2 * (pos - lastPos);
            forecastRot = rot + 2 * (rot - lastRot);

            lastPos = pos;
            lastRot = rot;
            forecastTime = Time.time;

            Vector3 turretRot = turretTF.localEulerAngles;
            turretRot.y = msg.turrelY;
            turretTF.localEulerAngles = turretRot;

        }
        public void SyncFire(MsgFire msg)
        {
            Bullet bullet = Fire();

            Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
            Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);

            bullet.transform.position = pos;
            bullet.transform.eulerAngles = rot;
        }
        
        private void ForecastUpdate()
        {
            float t = (Time.time - forecastTime) / CtrlTank.syncInterval;
            t = Mathf.Clamp(t, 0f, 1f);

            Vector3 pos = transform.position;
            pos = Vector3.Lerp(pos, forecastPos, t);
            transform.position = pos;

            Quaternion quaternion = transform.rotation;
            Quaternion forecastQuat = Quaternion.Euler(forecastRot);
            quaternion = Quaternion.Lerp(quaternion, forecastQuat, t);
            transform.rotation = quaternion;
        }
    }
}