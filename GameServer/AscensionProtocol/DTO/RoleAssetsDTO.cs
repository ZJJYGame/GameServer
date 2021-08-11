using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AscensionProtocol;

namespace AscensionProtocol.DTO
{
    [Serializable]
   public class RoleAssetsDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual long SpiritStonesLow { get; set; }
        public virtual long XianYu { get; set; }
        public override void Release()
        {
            RoleID = -1;
            SpiritStonesLow = 0;
            XianYu = 0;
        }
    }
}
