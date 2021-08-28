using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;

namespace AscensionServer
{
    public class BattleSkillEventConditionBase
    {
        protected BattleSkillEventBase battleSkillEventBase;
        protected BattleSkillEventData battleSkillEventData;
        public virtual bool CanTrigger(BattleDamageData battleDamageData)
        {
            return true;
        }
        public BattleSkillEventConditionBase(BattleSkillEventBase battleSkillEventBase, BattleSkillEventData battleSkillEventData)
        {
            this.battleSkillEventBase = battleSkillEventBase;
            this.battleSkillEventData = battleSkillEventData;
        }
        public BattleSkillEventConditionBase() { }
    }
    public class BattleSkillEventCondition_Crit: BattleSkillEventConditionBase
    {
        public override bool CanTrigger(BattleDamageData battleDamageData)
        {
            if (battleDamageData.isCrit)
                return true;
            else
                return false;
        }
    }
    public class  BattleSkillEventCondition_PropertyLimit: BattleSkillEventConditionBase
    {
        CharacterBattleData selfData;
        //true=>根据自身判断,false=>根据技能目标判断
        bool isSelf;
        //true=>目标属性高于,false=>目标属性低于
        bool isUp;
        public override bool CanTrigger(BattleDamageData battleDamageData)
        {
            CharacterBattleData targetData;
            if (isSelf)
                targetData = selfData;
            else
                targetData = GameEntry.BattleCharacterManager.GetCharacterEntity(battleDamageData.TargetID).CharacterBattleData;
            int value;
            int limitValue;
            if (battleSkillEventData.conditionFixedNum == 0 && battleSkillEventData.conditionPercentNum == 0)
            {
                value = targetData.GetPropertyPercent(battleSkillEventData.battleSkillEventTriggerNumSourceType);
                limitValue = battleSkillEventData.conditionPercentNum;
            }
            else if (battleSkillEventData.conditionFixedNum != 0)
            {
                value = targetData.GetProperty(battleSkillEventData.battleSkillEventTriggerNumSourceType);
                limitValue = battleSkillEventData.conditionFixedNum;
            }
            else
            {
                value = targetData.GetPropertyPercent(battleSkillEventData.battleSkillEventTriggerNumSourceType);
                limitValue = battleSkillEventData.conditionPercentNum;
            }
            if (isUp)
                return value >= limitValue ? true : false;
            else
                return value <= limitValue ? true : false;
        }
        public BattleSkillEventCondition_PropertyLimit(BattleSkillEventBase battleSkillEventBase,BattleSkillEventData battleSkillEventData,bool isSelf,bool isUp) : base(battleSkillEventBase, battleSkillEventData)
        {
            this.isSelf = isSelf;
            this.isUp = isUp;
            selfData = battleSkillEventBase.OwnerEntity.CharacterBattleData;
        }
    }

}
