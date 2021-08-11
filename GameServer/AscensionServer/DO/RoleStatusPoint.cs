using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    public class RoleStatusPoint : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual int SlnNow { get; set; }
        public virtual string AbilityPointSln { get; set; }

        public RoleStatusPoint()
        {
            SlnNow = 0;
            AbilityPointSln = "{}";
        }
        public override void Release()
        {
            RoleID = -1;
            SlnNow = 0;
            AbilityPointSln = "{}";
        }
    }
}
