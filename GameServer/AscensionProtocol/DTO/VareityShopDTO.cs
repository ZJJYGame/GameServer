using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public  class VareityShopDTO: DataTransferObject
    {
        public virtual byte VareityshopID { get; set; }
        public virtual Dictionary<int, List<GoodsStatus>> AllGoods { get; set; }
        public override void Release()
        {
            AllGoods = null;
        }
    }
    [Serializable]
    public class GoodsStatus
    {
        public virtual int Goods_ID { get; set; }
        public virtual int Price { get;set; }//价格
        public virtual int Limitquantity { get; set; }//限定数量
    }
}
