using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleRingDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int RingIdArray { get; set; }
        public override void Release()
        {
            RoleID = -1;
            RingIdArray = 0;
        }
    }
}
