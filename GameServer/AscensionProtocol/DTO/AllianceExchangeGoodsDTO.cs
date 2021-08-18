using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AllianceExchangeGoodsDTO: DataTransferObject
    {
        public virtual int AllianceID { get; set; }
        public virtual Dictionary<int, ExchangeSetting> ExchangeGoods { get; set; }

        public AllianceExchangeGoodsDTO()
        {
            AllianceID = -1;
            ExchangeGoods = new Dictionary<int, ExchangeSetting>();
        }

        public override void Release()
        {
            AllianceID = -1;
            ExchangeGoods = new Dictionary<int, ExchangeSetting>();
        }
    }
    public class ExchangeSetting : DataTransferObject
    {
        public int Contribution { get; set; }
        public int Job { get; set; }
        public override void Release()
        {
            Contribution = 0;
            Job = 1;
        }
    }
}
