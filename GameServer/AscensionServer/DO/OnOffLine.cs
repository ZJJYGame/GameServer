using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class OnOffLine :DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual int MsGfID { get; set; }
        public virtual int ExpType { get; set; }
        public virtual string OffTime { get; set; }
        public override void Release()
        {
            RoleID = -1;
            MsGfID = 0;
            OffTime = null;
        }
    }
}


