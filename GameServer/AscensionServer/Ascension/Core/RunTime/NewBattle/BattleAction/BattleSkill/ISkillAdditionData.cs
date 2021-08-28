using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public interface ISkillAdditionData
    {
        int DamgeAddition { get; set; }
        int DamagDeduction { get; set;}
        int CritProp { get; set; }
        int CritDamage { get; set; }
        int IgnoreDefensive { get; set; }
        int DodgeProp { get; set; }
    }
}
