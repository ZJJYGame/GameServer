using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RoleAuctionItems : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string AuctionItemsGuidList { get; set; }
        public override void Release()
        {
            RoleID = 0;
            AuctionItemsGuidList = null;
        }
    }
}


