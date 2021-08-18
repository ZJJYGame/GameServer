using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class DemonicSoul : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string  DemonicSouls { get; set; }
        public virtual string DemonicSoulIndex { get; set; }

        public override void Release()
        {
            DemonicSouls = null;
            DemonicSoulIndex = null;
        }
        public DemonicSoul()
        {
            DemonicSouls = "{}";
            DemonicSoulIndex = "{}";
        }
    }
}


