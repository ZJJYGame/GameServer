using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class AuctionGoods : DataObject
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
}


