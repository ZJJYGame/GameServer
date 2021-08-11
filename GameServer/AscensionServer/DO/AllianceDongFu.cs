using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class AllianceDongFu : DataObject
    {
        public virtual int AllianceID { get; set; }
        public virtual int SpiritRangeID { get; set; }
        public virtual int Occupant { get; set; }
        public virtual string PreemptOne { get; set; }
        public virtual string PreemptTow { get; set; }
        public virtual string PreemptThree { get; set; }
        public virtual string PreemptFour { get; set; }
        public virtual string PreemptFive { get; set; }

        public override void Release()
        {
            AllianceID = - 1;
            SpiritRangeID = 0;
        }
    }
}
