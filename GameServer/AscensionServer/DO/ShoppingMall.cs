using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public  class ShoppingMall: DataObject
    {
        public virtual int ID { get; set; }
        public virtual string Materials { get; set; }
        public virtual string NewArrival { get; set; }
        public virtual string QualifiedToBuy { get; set; }
        public virtual string RechargeStore { get; set; }

        public override void Release()
        {
            ID = -1;
            Materials = null;
            NewArrival = null;
            QualifiedToBuy = null;
        }
    }
}


