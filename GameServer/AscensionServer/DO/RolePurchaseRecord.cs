using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RolePurchaseRecord : DataObject
    {

        public virtual int RoleID { get; set; }
        public virtual string GoodsPurchasedCount { get; set; }//goodsID为key,count为value

        public RolePurchaseRecord()
        {
            RoleID = -1;
            GoodsPurchasedCount = null;
        }
        public override void Release()
        {
            RoleID = -1;
            GoodsPurchasedCount = null;
        }
    }
}


