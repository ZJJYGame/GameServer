using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    public class VareityShop : DataObject
    {
        public virtual byte VareityshopID { get; set; }
        public virtual string AllGoods { get; set; }
        public VareityShop()
        {
            AllGoods = null;
        }

        public override void Release()
        {
            AllGoods = null;
        }
    }
}


