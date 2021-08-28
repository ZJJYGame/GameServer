using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 战斗所有行动的基类
    /// </summary>
    public abstract class BattleActionObjBase
    {
        CharacterBattleData CharacterBattleData { get { return OwnerEntity.CharacterBattleData; } }
        public BattleCharacterEntity OwnerEntity { get; private set; }

        public  int ActionID{ get; private set; }
        public int NowCold { get; protected set; }
        public int MaxCold { get; protected set; }

        //进入冷却
        public virtual void EnterCold()
        {
            NowCold = MaxCold;
        }
        //减少冷却
        public virtual void ReduceCold()
        {
            if (NowCold > 0)
                NowCold--;
        }

        public abstract bool CanUseAction(BattleCharacterEntity target);
        public abstract bool ActionCost(out ActionCost actionCost);

        public BattleActionObjBase(int actionId, BattleCharacterEntity battleCharacterEntity)
        {
            ActionID = actionId;
            OwnerEntity = battleCharacterEntity;
        }
    }
}
