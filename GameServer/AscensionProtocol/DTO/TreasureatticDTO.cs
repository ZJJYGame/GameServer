using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class TreasureatticDTO : DataTransferObject
    {
        public virtual int ID { get; set; }
        //public virtual string ItemAmountDict { get; set; }
        //public virtual string ItemRedeemedDict { get; set; }

        public virtual Dictionary<int, int> ItemAmountDict { get; set; }
        public virtual Dictionary<int, int> ItemRedeemedDict { get; set; }
        public virtual Dictionary<int, int> ItemNotRefreshDict { get; set; }
    
        public override void Release()
        {
            ID = -1;
            ItemAmountDict=null;
            ItemRedeemedDict= null;
            ItemNotRefreshDict = null;
        }
    }
}