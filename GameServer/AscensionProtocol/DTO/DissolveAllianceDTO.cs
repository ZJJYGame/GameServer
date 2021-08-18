using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class DissolveAllianceDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual bool IsDissolve { get; set; }
        public DissolveAllianceDTO()
        {
            RoleID = 0;
            IsDissolve = false;
        }
        public override void Release()
        {
           
        }
    }
}
