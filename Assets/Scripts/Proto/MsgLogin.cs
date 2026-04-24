using Framework.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto
{
    public class MsgLogin : MsgBase
    {
        public MsgLogin(string id, string pw)
        {
            protoName = "MsgLogin";
            this.id = id;
            this.pw = pw;
            result = 0;
        }
        public string id;
        public string pw;
        public int result;  // success-0 fail->1
    }
}