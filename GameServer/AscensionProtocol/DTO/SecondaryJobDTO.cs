using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    public class SecondaryJobDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int UseItemID { get; set; }
        public virtual List<int> Units { get; set; }
        public override void Release()
        {
            RoleID = -1;
            UseItemID = 0;
            Units = new List<int>();
        }
    }
}
