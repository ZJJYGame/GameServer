using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public class SkillAdditionData : ISkillAdditionData
    {
        public int DamgeAddition { get; set; }
        public int CritProp { get; set; }
        public int CritDamage { get; set; }
        public int IgnoreDefensive { get; set; }
        public int DamagDeduction { get; set; }
        public int DodgeProp { get; set; }
    }
}
