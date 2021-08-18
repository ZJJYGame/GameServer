using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RolePurchaseRecordDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual Dictionary<int,int> GoodsPurchasedCount { get; set; }//goodsID为key,count为value


        public override void Release()
        {
            RoleID = -1;
            GoodsPurchasedCount = null;

        }
    }
}
