using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public struct BattleResultInfo
    {
        public int CharacterId { get;private set; }
        public bool IsWin { get;private set; }
        public BattleCharacterType BattleCharacterType { get; private set; }
        public BattleResultInfo(int characterId,bool isWin,BattleCharacterType battleCharacterType)
        {
            CharacterId = characterId;
            IsWin = isWin;
            BattleCharacterType = battleCharacterType;
        }
    }
    public enum BattleCharacterType
    {
        Player=0,
        Pet=1,
        AI=2,
    }
}
