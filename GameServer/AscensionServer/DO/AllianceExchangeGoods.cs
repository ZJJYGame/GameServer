using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class AllianceExchangeGoods : DataObject
    {
        public virtual int AllianceID { get; set; }
        public virtual string  ExchangeGoods { get; set; }
        public AllianceExchangeGoods()
        {
            AllianceID = -1;
            ExchangeGoods = "{}";
        }

        public override void Release()
        {
            AllianceID = -1;
            ExchangeGoods = "{}";
        }

    }
}
