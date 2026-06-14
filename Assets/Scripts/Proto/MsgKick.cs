using Framework.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proto
{
    class MsgKick : MsgBase
    {
        public MsgKick()
        {
            protoName = "MsgKick";
            reason = 0;
        }
        public int reason;
    }
}
