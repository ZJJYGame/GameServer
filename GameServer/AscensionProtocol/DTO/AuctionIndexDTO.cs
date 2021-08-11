using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AuctionIndexDTO : DataTransferObject
    {
        public virtual int GlobalID { get; set; }
        public List<AuctionGoodsIndex> AuctionGoodsIndexList { get; set; }

        public override void Release()
        {
            GlobalID = 0;
            AuctionGoodsIndexList = null;
        }
    }

    [Serializable]
    public class AuctionGoodsIndex:IComparable<AuctionGoodsIndex>
    {
        public string RedisKey { get; set; }//redis当中存储的key
        public int Price { get; set; }//价格，用于排序


        public int CompareTo(AuctionGoodsIndex other)
        {
            if (this.Price == other.Price)
                return 0;
            else if (this.Price < other.Price)
                return -1;
            else
                return 1;
        }
    }
}
