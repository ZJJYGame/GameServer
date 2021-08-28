using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public class BattleActionBase
    {
        BattleCharacterEntity Owner { get; set; }

        public BattleActionBase(BattleCharacterEntity battleCharacterEntity)
        {
            Owner = battleCharacterEntity;
        }
    }
}
