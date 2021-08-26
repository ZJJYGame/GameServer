using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;

namespace AscensionServer
{
    public class BattleBuffEventConditionBase
    {
        protected BattleBuffTriggerCondition battleBuffTriggerCondition;
        protected BattleBuffObj battleBuffObj;
        protected BattleCharacterEntity Owner { get { return battleBuffObj.Owner; } }

        public virtual bool CanTrigger(BattleCharacterEntity target,BattleDamageData battleDamageData)
        {
            return true;
        }
        public BattleBuffEventConditionBase(BattleBuffTriggerCondition battleBuffTriggerCondition,BattleBuffObj battleBuffObj)
        {
            this.battleBuffTriggerCondition = battleBuffTriggerCondition;
            this.battleBuffObj = battleBuffObj;
        }
    }
    //使用指定技能
    public class BattleBuffEventCondition_UseDesignatedSkill : BattleBuffEventConditionBase
    {
        bool isSelf;
        List<uint> skillIdList;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            int nowUseSkillId;
            if(isSelf)
                nowUseSkillId = Owner.BattleSkillController.nowUseSkillId;
            else
                nowUseSkillId = target.BattleSkillController.nowUseSkillId;
            Utility.Debug.LogError("当前使用的技能=>" + nowUseSkillId);
            if (skillIdList.Contains((uint)nowUseSkillId))
                return true;
            else
                return false;
        }
        public BattleBuffEventCondition_UseDesignatedSkill(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            isSelf = battleBuffTriggerCondition.flag;
            skillIdList = battleBuffTriggerCondition.idList;
        }
    }
    //拥有指定技能
    public class BattleBuffEventCondition_HaveDesignatedSkill : BattleBuffEventConditionBase
    {
        bool isSelf;
        List<uint> skillIdList;

        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            BattleCharacterEntity targetEntity;
            if (isSelf)
                targetEntity = Owner;
            else
                targetEntity = target;
            //拥有任一技能即可满足
            for (int i = 0; i < skillIdList.Count; i++)
            {
                if (targetEntity.BattleSkillController.HasSkill((int)skillIdList[i]))
                    return true;
            }
            return false;
        }
        public BattleBuffEventCondition_HaveDesignatedSkill(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            isSelf = battleBuffTriggerCondition.flag;
            skillIdList = battleBuffTriggerCondition.idList;
        }
    }
    /// <summary>
    /// 不拥有指定技能
    /// </summary>
    public class BattleBuffEventCondition_NotHaveDesignatedSkill : BattleBuffEventConditionBase
    {
        bool isSelf;
        List<uint> skillIdList;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            BattleCharacterEntity targetEntity;
            if (isSelf)
                targetEntity = Owner;
            else
                targetEntity = target;
            //拥有任一技能即可返回，必须所有技能都得拥有
            for (int i = 0; i < skillIdList.Count; i++)
            {
                if (targetEntity.BattleSkillController.HasSkill((int)skillIdList[i]))
                    return false;
            }
            return true;
        }
        public BattleBuffEventCondition_NotHaveDesignatedSkill(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            isSelf = battleBuffTriggerCondition.flag;
            skillIdList = battleBuffTriggerCondition.idList;
        }
    }

    public class BattleBuffEventCondition_LimitSkillTargetNum : BattleBuffEventConditionBase
    {
        bool isSelf;
        int targetCount;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            BattleCharacterEntity targetEntity;
            if (isSelf)
                targetEntity = Owner;
            else
                targetEntity = target;
            GameEntry.DataManager.TryGetValue<Dictionary<int, BattleSkillData>>(out var skillDict);
            BattleSkillData battleSkillData = skillDict[targetEntity.BattleSkillController.nowUseSkillId];
            if (battleSkillData.TargetNumber == targetCount)
                return true;
            else
                return false;
        }
        public BattleBuffEventCondition_LimitSkillTargetNum(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            isSelf = battleBuffTriggerCondition.flag;
            targetCount = battleBuffTriggerCondition.targetCount;
        }
    }

    public class BattleBuffEventCondition_DesignatedDamageType : BattleBuffEventConditionBase
    {
        BattleSkillDamageType battleSkillDamageType;

        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            if (battleDamageData == null) return false;
            if (battleDamageData.damageType == battleSkillDamageType)
                return true;
            else
                return false;
        }
        public BattleBuffEventCondition_DesignatedDamageType(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            battleSkillDamageType = battleBuffTriggerCondition.battleSkillDamageType;
        }
    }
    public class BattleBuffEventCondition_DamageCrit : BattleBuffEventConditionBase
    {
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            if (battleDamageData.isCrit)
                return true;
            else
                return false;
        }
        public BattleBuffEventCondition_DamageCrit(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
        }
    }
    /// <summary>
    /// 目标属性限定
    /// </summary>
    public class BattleBuffEventCondition_DesignatedPropertyLimit : BattleBuffEventConditionBase
    {
        BattleBuffCondition_SourcePropertyType battleBuffCondition_SourcePropertyType;
        bool isSelf;
        bool isUp;
        int criticalValue;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            BattleCharacterEntity targetEntity;
            if (isSelf)
                targetEntity = Owner;
            else
                targetEntity = target;
            int percentValue= targetEntity.CharacterBattleData.GetPropertyPercent(battleBuffCondition_SourcePropertyType);
            if (isUp)
                return percentValue > criticalValue ? true : false;
            else
                return percentValue > criticalValue ? false : true;
        }
        public BattleBuffEventCondition_DesignatedPropertyLimit(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            battleBuffCondition_SourcePropertyType = battleBuffTriggerCondition.battleBuffCondition_SourcePropertyType;
            isSelf = battleBuffTriggerCondition.flag;
            isUp = battleBuffTriggerCondition.isUp;
            criticalValue = battleBuffTriggerCondition.criticalValue;
        }
    }
    public class BattleBuffEventCondition_BothDesignatedPropertyCompare : BattleBuffEventConditionBase
    {
        BattleBuffCondition_SourcePropertyType battleBuffCondition_SourcePropertyType;
        bool isUp;
        int criticalValue;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            int selfProperty = Owner.CharacterBattleData.GetProperty(battleBuffCondition_SourcePropertyType);
            int targetProperty = target.CharacterBattleData.GetProperty(battleBuffCondition_SourcePropertyType);
            Utility.Debug.LogError("selfProperty=>" + selfProperty);
            Utility.Debug.LogError("targetProperty=>" + targetProperty);
            int compareValue;
            if (targetProperty != 0)
                compareValue = selfProperty * 100 / targetProperty;
            else
                compareValue = 0;
            if (isUp)
                return compareValue > criticalValue ? true : false;
            else
                return compareValue > criticalValue ? false : true;
        }
        public BattleBuffEventCondition_BothDesignatedPropertyCompare(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            battleBuffCondition_SourcePropertyType = battleBuffTriggerCondition.battleBuffCondition_SourcePropertyType;
            isUp = battleBuffTriggerCondition.isUp;
            criticalValue = battleBuffTriggerCondition.criticalValue;
        }
    }
    public class BattleBuffEventCondition_TargetHaveDesignatedBuff : BattleBuffEventConditionBase
    {
        List<uint> buffIdList;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            for (int i = 0; i < buffIdList.Count; i++)
            {
                if (target.BattleBuffController.HasBuff((int)buffIdList[i]))
                    return true;
            }
            return false;
        }
        public BattleBuffEventCondition_TargetHaveDesignatedBuff(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            buffIdList = battleBuffTriggerCondition.idList;
        }
    }
    public class BattleBuffEventCondition_CharacterTypeLimit : BattleBuffEventConditionBase
    {
        BattleBuffCondition_CharacterType battleBuffCondition_CharacterType;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            switch (battleBuffCondition_CharacterType)
            {
                case BattleBuffCondition_CharacterType.Player:
                    if (target.GetType() == typeof(BattlePlayerEntity))
                        return true;
                    break;
                case BattleBuffCondition_CharacterType.Pet:
                    if (target.GetType() == typeof(BattlePetEntity))
                        return true;
                    break;
                case BattleBuffCondition_CharacterType.Summon:
                    if (target.GetType() == typeof(BattleAIEntity))
                        return true;
                    break;
            }
            return false;
        }
        public BattleBuffEventCondition_CharacterTypeLimit(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            battleBuffCondition_CharacterType = battleBuffTriggerCondition.battleBuffCondition_CharacterType;
        }
    }

   /// <summary>
   /// 近战or远程攻击限定
   /// </summary>
    public class BattleBuffEventCondition_CloseOrRangeAttack : BattleBuffEventConditionBase
    {
        bool isSelf;
        bool isCloseAttack;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            BattleCharacterEntity targetEntity;
            if (isSelf)
                targetEntity = Owner;
            else
                targetEntity = target;
            GameEntry.DataManager.TryGetValue<Dictionary<int, BattleSkillData>>(out var skillDict);
            BattleSkillData battleSkillData = skillDict[targetEntity.BattleSkillController.nowUseSkillId];
            if (isCloseAttack == battleSkillData.IsCloseAttack)
                return true;
            else
                return false;
        }
        public BattleBuffEventCondition_CloseOrRangeAttack(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            isCloseAttack = battleBuffTriggerCondition.isUp;
            isSelf = battleBuffTriggerCondition.flag;
        }
    }
    public class BattleBuffEventCondition_TargetNotHaveDesignatedBuff : BattleBuffEventConditionBase
    {
        List<uint> buffIdList;
        public override bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            for (int i = 0; i < buffIdList.Count; i++)
            {
                if (target.BattleBuffController.HasBuff((int)buffIdList[i]))
                    return false;
            }
            return true;
        }
        public BattleBuffEventCondition_TargetNotHaveDesignatedBuff(BattleBuffTriggerCondition battleBuffTriggerCondition, BattleBuffObj battleBuffObj) : base(battleBuffTriggerCondition, battleBuffObj)
        {
            buffIdList = battleBuffTriggerCondition.idList;
        }
    }
}
