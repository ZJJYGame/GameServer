using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class SutrasAtticDTO : DataTransferObject
    {
        public virtual int ID { get; set; }

        public virtual Dictionary<int,int> SutrasAmountDict { get; set; }
        public virtual  Dictionary<int, int> SutrasRedeemedDictl { get; set; }


        public override void Release()
        {
            ID = -1;
            SutrasAmountDict.Clear();
            SutrasRedeemedDictl.Clear();
        }
    }
}
