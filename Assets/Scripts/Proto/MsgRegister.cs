using Framework.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proto
{
    class MsgRegister : MsgBase
    {
        public MsgRegister(string id, string pw)
        {
            protoName = "MsgRegister";
            this.id = id;
            this.pw = pw;
            result = 0;
        }
        public string id;
        public string pw;
        public int result;
    }
}
