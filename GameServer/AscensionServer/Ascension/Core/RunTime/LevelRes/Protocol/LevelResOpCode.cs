using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public enum LevelResOpCode : byte
    {
        None=0,
        SYN=1,
        Gather=2,
        StartCombat= 3,
        FIN=4,
        CombatResult= 5,
    }
}
