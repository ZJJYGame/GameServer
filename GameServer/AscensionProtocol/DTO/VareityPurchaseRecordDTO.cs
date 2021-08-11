using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    public class VareityPurchaseRecordDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual Dictionary<int, int> VareityPurchasedCount { get; set; }//goodsID为key,count为value


        public override void Release()
        {
            RoleID = -1;
            VareityPurchasedCount = null;

        }
    }
    }
