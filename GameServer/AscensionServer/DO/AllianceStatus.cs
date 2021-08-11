using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    public class AllianceStatus : DataObject
    {
        public virtual int ID { get; set; }
        public virtual int AllianceLevel { get; set; }
        public virtual int AllianceNumberPeople { get; set; }
        public virtual int AlliancePeopleMax { get; set; }
        public virtual string AllianceMaster { get; set; }
        public virtual string AllianceName { get; set; }
        public virtual int Popularity { get; set; }
        public virtual string Manifesto { get; set; }
        public virtual int OnLineNum { get; set; }
        public AllianceStatus()
        {
            AllianceLevel =1;
            AllianceNumberPeople = 1;
            AlliancePeopleMax = 30;
            Popularity =500;
            OnLineNum = 0;
        }



        public override void Release()
        {
            ID = -1;
            AllianceLevel = 0;
            AlliancePeopleMax = 0;
            AllianceMaster = null;
            AllianceNumberPeople = 0;
            AllianceName = null;
            Popularity = 0;
            OnLineNum = 0;
        }
    }
}


