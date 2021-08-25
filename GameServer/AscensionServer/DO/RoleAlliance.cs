using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RoleAlliance : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual int AllianceID { get; set; }
        public virtual int AllianceJob { get; set; }
        public virtual int Reputation { get; set; }
        public virtual int ReputationMonth { get; set; }
        public virtual int ReputationHistroy { get; set; }
        public virtual string JoinTime { get; set; }
        public virtual string Offline { get; set; }
        public virtual string RoleName { get; set; }
        public virtual string ApplyForAlliance { get; set; }
        public virtual int RoleLevel { get; set; }

        public override void Release()
        {
            RoleID = -1;
            AllianceID = 0;
            AllianceJob = 4;
            Reputation = 0;
            ReputationMonth = 0;
            ReputationHistroy = 0;
            JoinTime = null;
            Offline = null;
            ApplyForAlliance = null;

            RoleLevel = 0;
        }
        public RoleAlliance()
        {
            RoleID = -1;
            AllianceID = 0;
            AllianceJob = 4;
            Reputation = 0;
            ReputationMonth = 0;
            ReputationHistroy = 0;
            JoinTime = null;
            Offline = "在线";
 
            ApplyForAlliance = null;
            RoleLevel = 0;
        }

    }
}


