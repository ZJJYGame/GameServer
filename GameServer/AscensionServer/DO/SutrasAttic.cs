using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer.Model
{
    [Serializable]
    public  class SutrasAttic:DataObject
    {
        public virtual int ID { get; set; }

        public virtual String SutrasAmountDict { get; set; }
        public virtual String SutrasRedeemedDictl { get; set; }

        public override void Release()
        {
            ID = -1;
            SutrasAmountDict=null;
            SutrasRedeemedDictl = null;
        }
    }
}


