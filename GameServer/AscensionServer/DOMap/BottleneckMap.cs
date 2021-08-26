using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionServer.Model;
using FluentNHibernate.Mapping;
namespace AscensionServer
{
   public class BottleneckMap:ClassMap<Bottleneck>
    {
        public BottleneckMap()
        {
            Id(x => x.RoleID).GeneratedBy.Assigned().Column("role_id");
            Map(x => x.IsBottleneck).Column("isbottleneck");
            Map(x => x.IsThunder).Column("isthunder");
            Map(x => x.SpiritualRootVaule).Column("spiritualroot_vaule");
            Map(x => x.ThunderRound).Column("thunderround");
            Map(x => x.BreakThroughVauleNow).Column("breakthrough_vaule_now");
            Map(x => x.BreakThroughVauleMax).Column("breakthrough_vaule_max");
            Map(x => x.CraryVaule).Column("craryvaule");
            Map(x => x.DemonID).Column("demon_id");
            Map(x => x.IsDemon).Column("isdemon");
            Map(x => x.DrugID).Column("drugid");
            Map(x => x.DrugPercent).Column("drugpercent");
            Table("role_bottleneck");
        }
    }
}


