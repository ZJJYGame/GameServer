using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using Cosmos;

namespace AscensionServer
{
    /// <summary>
    /// 战斗技能事件基类
    /// </summary>
    public class BattleSkillEventBase
    {
        public BattleCharacterEntity OwnerEntity { get { return ownerSkill.OwnerEntity; } }
        public BattleSkillBase ownerSkill;
        protected BattleSkillEventConditionBase battleSkillEventConditionBase;
        protected BattleSkillEventData battleSkillEventData;
        protected BattleRoomEntity BattleRoomEntity { get { return GameEntry.BattleRoomManager.GetBattleRoomEntity(OwnerEntity.RoomID); } }
        //战斗事件触发
        protected virtual void Trigger(BattleDamageData battleDamageData,ISkillAdditionData skillAdditionData)
        {
            Utility.Debug.LogError("无事件触发");
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;

        }
        public BattleSkillEventBase(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData)
        {
            ownerSkill = battleSkillBase;
            this.battleSkillEventData = battleSkillEventData;
            switch (battleSkillEventData.battleSkillEventTriggerCondition)
            {
                case BattleSkillEventTriggerCondition.None:
                    battleSkillEventConditionBase = new BattleSkillEventConditionBase();
                    break;
                case BattleSkillEventTriggerCondition.Crit:
                    battleSkillEventConditionBase = new BattleSkillEventCondition_Crit();
                    break;
                case BattleSkillEventTriggerCondition.TargetPropertyUnder:
                    battleSkillEventConditionBase = new BattleSkillEventCondition_PropertyLimit(this, battleSkillEventData, false, false);
                    break;
                case BattleSkillEventTriggerCondition.TargetPropertyOver:
                    battleSkillEventConditionBase = new BattleSkillEventCondition_PropertyLimit(this, battleSkillEventData, false, true);
                    break;
                case BattleSkillEventTriggerCondition.SelfPropertyUnder:
                    battleSkillEventConditionBase = new BattleSkillEventCondition_PropertyLimit(this, battleSkillEventData, true, false);
                    break;
                case BattleSkillEventTriggerCondition.SelfPropertyOver:
                    battleSkillEventConditionBase = new BattleSkillEventCondition_PropertyLimit(this, battleSkillEventData, true, true);
                    break;
            }

            switch (battleSkillEventData.battleSkillEventTriggerTime)
            {
                case BattleSkillEventTriggerTime.BeforeAttack:
                    battleSkillBase.SkillEventBeforeAttack += Trigger;
                    break;
                case BattleSkillEventTriggerTime.BehindAttack:
                    battleSkillBase.SkillEventBehindAttack += Trigger;
                    break;
            }
        }
    }
    //释放技能
    public class BattleSkillEvent_Skill: BattleSkillEventBase
    {
        int triggerSkillID;
        protected override void Trigger(BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            Utility.Debug.LogError("触发释放技能事件");
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            List<int> tempTargetList = ownerSkill.OwnerEntity.GetTargetIdList(triggerSkillID,battleSkillEventData.isAutoChangeTarget, new List<int>() { battleDamageData.TargetID });
            ownerSkill.OwnerEntity.BattleSkillController.UseSkill(triggerSkillID, tempTargetList,false);
            Utility.Debug.LogError("触发释放技能事件成功");
        }

        public BattleSkillEvent_Skill(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            triggerSkillID = battleSkillEventData.EventValue;
        }
    }
    //治疗
    public class BattleSkillEvent_Heal: BattleSkillEventBase
    {
        int healValue;
        protected override void Trigger(BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            Utility.Debug.LogError("回血触发");
            int value = OwnerEntity.CharacterBattleData.MaxHp * healValue / 100;
            OwnerEntity.CharacterBattleData.ChangeProperty(BattleSkillDamageTargetProperty.Health, value);
            if (BattleRoomEntity.GetBattleTransfer().TargetInfos == null)
                BattleRoomEntity.GetBattleTransfer().TargetInfos = new List<TargetInfoDTO>();
            BattleRoomEntity.GetBattleTransfer().TargetInfos.Add(new TargetInfoDTO()
            {
                TargetID= OwnerEntity.UniqueID,
                TargetHPDamage= value,
            });
            
        }

        public BattleSkillEvent_Heal(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            healValue = battleSkillEventData.EventValue;
        }
    }
    //技能吸血
    public class BattleSkillEvent_SuckBlood : BattleSkillEventBase
    {
        int healValue;
        protected override void Trigger(BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            int value = Math.Abs(battleDamageData.damageNum) * healValue / 100;
            OwnerEntity.CharacterBattleData.ChangeProperty(BattleSkillDamageTargetProperty.Health, value);
            BattleRoomEntity.GetBattleTransfer().TargetInfos.Add(new TargetInfoDTO()
            {
                TargetID = OwnerEntity.UniqueID,
                TargetHPDamage = value,
            });
        }

        public BattleSkillEvent_SuckBlood(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            healValue = battleSkillEventData.EventValue;
        }
    }

    public class BattleSkillEvent_AddDamage : BattleSkillEventBase
    {
        int addDamageValue;
        protected override void Trigger(BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            Utility.Debug.LogError("技能伤害增加触发");
            //ownerSkill.damageAdditionList.Add(addDamageValue);
            skillAdditionData.DamgeAddition += addDamageValue;
        }
        public BattleSkillEvent_AddDamage(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            addDamageValue = battleSkillEventData.EventValue;
        }
    }

    public class BattleSkillEvent_AddCritProp : BattleSkillEventBase
    {
        int addCritPropValue;
        protected override void Trigger(BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            Utility.Debug.LogError("暴击率增加触发");
            //ownerSkill.critPropList.Add(addCritPropValue);
            skillAdditionData.CritProp += addCritPropValue;
            BattleDamageData temp = ownerSkill.IsCrit(battleDamageData.attackSection, GameEntry.BattleCharacterManager.GetCharacterEntity(battleDamageData.TargetID), skillAdditionData);
            battleDamageData.isCrit = temp.isCrit;
        }
        public BattleSkillEvent_AddCritProp(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            addCritPropValue = battleSkillEventData.EventValue;
        }
    }

    public class BattleSkillEvent_AddCritDamage : BattleSkillEventBase
    {
        int addCritDamageValue;
        protected override void Trigger(BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            Utility.Debug.LogError("暴击伤害增加触发");
            //ownerSkill.critDamageList.Add(addCritDamageValue);
            skillAdditionData.CritDamage += addCritDamageValue;
        }
        public BattleSkillEvent_AddCritDamage(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            addCritDamageValue = battleSkillEventData.EventValue;
        }
    }

    public class BattleSkillEvent_AddIgnoreDefence : BattleSkillEventBase
    {
        int addIgnoreDefenseValue;
        protected override void Trigger( BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (!battleSkillEventConditionBase.CanTrigger(battleDamageData))
                return;
            Utility.Debug.LogError("穿透增加触发");
            //ownerSkill.ignoreDefensiveList.Add(addIgnoreDefenseValue);
            skillAdditionData.IgnoreDefensive += addIgnoreDefenseValue;
        }
        public BattleSkillEvent_AddIgnoreDefence(BattleSkillBase battleSkillBase, BattleSkillEventData battleSkillEventData) : base(battleSkillBase, battleSkillEventData)
        {
            addIgnoreDefenseValue = battleSkillEventData.EventValue;
        }
    }
}
