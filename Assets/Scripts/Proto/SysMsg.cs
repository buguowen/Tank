using Framework.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto
{
    public class MsgPing : MsgBase
    {
        public MsgPing()
        {
            protoName = "MsgPing";
        }
    }
    public class MsgPong : MsgBase
    {
        public MsgPong()
        {
            protoName = "MsgPong";
        }
    }
}