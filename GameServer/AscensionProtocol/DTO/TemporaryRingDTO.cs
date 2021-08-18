using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class TemporaryRingDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual Dictionary<int, RingItemsDTO> RingItems { get; set; }
        public override void Release()
        {
            RoleID = -1;
            RingItems.Clear();
        }
    }
}
