using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionServer.Model;
using FluentNHibernate.Mapping;

namespace AscensionServer.DOMap
{
    public class RoleAllianceMap: ClassMap<RoleAlliance>
    {
        
        public RoleAllianceMap()
        {
            Id(x => x.RoleID).GeneratedBy.Assigned().Column("roleid");
            Map(x => x.AllianceID).Column("alliance_id");
            Map(x => x.AllianceJob).Column("alliance_job");
            Map(x => x.Reputation).Column("reputation");
            Map(x => x.ReputationMonth).Column("reputation_month");
            Map(x => x.ReputationHistroy).Column("reputation_history");
            Map(x => x.JoinTime).Column("jointime");
            Map(x => x.Offline).Column("offline");
            Map(x => x.RoleName).Column("role_name");
            Map(x => x.ApplyForAlliance).Column("apply_for_alliance_list");
            Map(x => x.RoleLevel).Column("role_level");
            Table("role_alliance");
        }
    }
}


