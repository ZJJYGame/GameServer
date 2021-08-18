using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleAuctionItemsDTO : DataTransferObject
    {
        public int RoleID { get; set; }
        public List<RoleAuctionItem> AuctionItemsGuidList { get; set; }
        public override void Release()
        {
            RoleID = 0;
            AuctionItemsGuidList = null;
        }
    }
    [Serializable]
    public class RoleAuctionItem
    {
        public AuctionGoodsDTO AuctionGoods { get; set; }
        public bool IsPutAway { get; set; }
    }
}


  
