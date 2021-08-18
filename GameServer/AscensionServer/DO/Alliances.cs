using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class Alliances : DataObject
    {
        public virtual int ID { get; set; }
        public virtual string AllianceList { get; set; }
        public Alliances()
        {
            AllianceList = null;
        }


        public override void Release()
        {
            ID = -1;
            AllianceList = null;
        }
    }
}


