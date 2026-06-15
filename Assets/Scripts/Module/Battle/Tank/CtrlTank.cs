using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tank
{
    public class CtrlTank : BaseTank
    {
        new void Update()
        {
            base.Update();
            MoveUpdate();
            TurretUpdate();
            FireUpdate();
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
                Fire();
            }
        }
    }
}