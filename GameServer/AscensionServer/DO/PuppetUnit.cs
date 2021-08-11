using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
   public class PuppetUnit:DataObject
    {
        public virtual int RoleID { set; get; }
        public virtual string PuppetUnitInfoDict { set; get; }
        public virtual string UnitIndesDict { set; get; }

        public PuppetUnit()
        {
            PuppetUnitInfoDict = "{}";
            UnitIndesDict = "{}";
        }
        public override void Release()
        {
            
        }
    }
}
