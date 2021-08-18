using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
   public class RoleUseItemDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int PetID { get; set; }
        public virtual int UseItemID { get; set; }
        public override void Release()
        {
        }
    }
}
