using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer.Model
{
    [Serializable]
    public class Treasureattic : DataObject
    {
        public virtual int ID { get; set; }
        public virtual string ItemAmountDict { get; set; }
        public virtual string ItemRedeemedDict { get; set; }
        public virtual string ItemNotRefreshDict { get; set; }

        public override void Release()
        {
            ID = -1;
            ItemAmountDict = null;
            ItemRedeemedDict = null;
            ItemNotRefreshDict = null;
        }
    }
}

