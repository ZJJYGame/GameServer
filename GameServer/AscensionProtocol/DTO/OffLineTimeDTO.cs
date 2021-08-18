using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
        [Serializable]
        public class OffLineTimeDTO : DataTransferObject
    {
            public virtual int RoleID { get; set; }
            public virtual string OffTime { get; set; }
        public override void Release()
        {
            RoleID = -1;
            OffTime = null;
        }
    }
}
