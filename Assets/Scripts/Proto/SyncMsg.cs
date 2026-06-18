using Framework.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto
{

    public class MsgSyncTank : MsgBase
    { 
        public MsgSyncTank()
        {
            protoName = "MsgSyncTank";
        }

        public float x = 0;
        public float y = 0;
        public float z = 0; 
        public float ex = 0;
        public float ey = 0;
        public float ez = 0;
        public float turrelY = 0;

        public string id = "";
    }

    public class MsgFire : MsgBase
    { 
        public MsgFire()
        {
            protoName = "MsgFire";
        }
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float ex = 0f;
        public float ey = 0f;
        public float ez = 0f;

        public string id = "";
    }

    public class MsgHit : MsgBase
    { 
        public MsgHit()
        {
            protoName = "MsgHit";
        }
        public string targetId = "";
        //击中点
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;

        //服务端补充
        public string id = "";
        public int hp = 0;
        public int damage = 0;

    }
        

}