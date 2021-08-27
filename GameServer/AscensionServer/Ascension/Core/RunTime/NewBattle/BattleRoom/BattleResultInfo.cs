using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public struct BattleResultInfo
    {
        public bool isWin;
        public int[] CharacterId;
        public bool ContainsCharacter(int characterId)
        {
            if (CharacterId == null)
                return false;
            if (CharacterId.Contains(characterId))
                return true;
            else
                return false;
        }
    }
}
