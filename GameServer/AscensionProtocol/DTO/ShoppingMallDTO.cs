using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class ShoppingMallDTO : DataTransferObject
    {
        public virtual int ID { get; set; }
        public virtual List<ShoppingGoods> Materials { get; set; }
        public virtual List<ShoppingGoods> NewArrival { get; set; }
        public virtual List<ShoppingGoods> QualifiedToBuy { get; set; }
        public virtual List<RechargeGoods> RechargeStore { get; set; }

        public override void Release()
        {
            ID = -1;
            Materials = null;
            NewArrival = null;
            QualifiedToBuy = null;
            RechargeStore = null;
        }
    }

    [Serializable]
    public class ShoppingGoods
    {
        public virtual int Price { get; set; }//价格
        public virtual int GoodsID { get; set; }//物品id
        public virtual float Discount { get; set; }//折扣
        public virtual bool IsRecommend { get; set; }
        public virtual int SelfLimitQuantity { get; set; }//个人限量
    }

    [Serializable]
    public class RechargeGoods
    {
        public virtual int Price { get; set; }//价格
        public virtual int GoodsID { get; set; }//物品id
        public virtual int ExtraImmortalJade { get; set; }//额外仙玉
        public virtual int ImmortalJade { get; set; }//仙玉
    }
}
