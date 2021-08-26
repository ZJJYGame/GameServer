using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionServer.Model;
using FluentNHibernate.Mapping;
namespace AscensionServer
{
    public class RoleMap:ClassMap<Role>
    {
        public RoleMap()
        {
            Id(x => x.RoleID).GeneratedBy.Increment().Column("role_id");
            Map(x => x.RoleFaction).Column("role_faction");
            Map(x => x.RoleGender).Column("role_gender");
            Map(x => x.RoleTalent).Column("role_talent");
            Map(x => x.RoleRoot).Column("role_root");
            Map(x => x.RoleName).Column("role_name").Unique();
            Map(x => x.RoleLevel).Column("role_level");
            Map(x => x.roleExp).Column("role_exp");
            Table("role");
        }
    }
}


