using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RoleRepository:DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string Items { get; set; }
        public override void Release()
        {
            RoleID = -1;
            Items = null;
        }
    }
}


