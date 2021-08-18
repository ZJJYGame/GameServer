using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class AuctionIndex : DataObject
    {
        public virtual int GlobalID { get; set; }
        public virtual List<string> AuctionGoodsIndexList { get; set; }

        public override void Release()
        {
            GlobalID = 0;
            AuctionGoodsIndexList = null;
        }
    }
}


