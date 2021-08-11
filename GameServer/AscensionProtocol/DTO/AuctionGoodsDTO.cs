using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AuctionGoodsDTO : DataTransferObject
    {
        public virtual string GUID { get; set; }
        public virtual int RoleID { get; set; }
        public virtual int GlobalID { get; set; }
        public virtual int Price { get; set; }
        public virtual string ItemData { get; set; }
        public virtual int Count { get; set; }

        public override void Release()
        {
            GUID = null;
            RoleID = 0;
            GlobalID = 0;
            Price = 0;
            ItemData = null;
            Count = 0;
        }
    }
    public enum SyncAuctionType:byte
    {
        GetAuctionGoods=0,
        PutAwayAuctionGoods=1,
        SoldOutAuctionGoods=2,
        BuyAuctionGoods=3,
        AuctionGoodsBeBought=4,
    }
}
