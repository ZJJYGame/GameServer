using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    public class VareityPurchaseRecord: DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string VareityPurchasedCount { get; set; }//goodsID为key,count为value

        public VareityPurchaseRecord()
        {
            RoleID = -1;
            VareityPurchasedCount = null;
        }
        public override void Release()
        {
            RoleID = -1;
            VareityPurchasedCount = null;
        }
    }
}


