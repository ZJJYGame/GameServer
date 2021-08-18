using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
   public class OnOffLineDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int MsGfID { get; set; }//当前秘术的唯一ID
        public virtual int ExpType { get; set; }//1为功法2为秘术
        public virtual string OffTime { get; set; }
        public override void Release()
        {
            RoleID = -1;
            MsGfID = 0;
            OffTime = null;
        }
    }
}
