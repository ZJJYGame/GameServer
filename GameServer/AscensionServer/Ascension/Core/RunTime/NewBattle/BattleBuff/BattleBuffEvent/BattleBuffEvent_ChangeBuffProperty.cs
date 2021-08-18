using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 改变buff属性
    /// </summary>
    public class BattleBuffEvent_ChangeBuffProperty : BattleBuffEventBase
    {
        BuffEvent_PropertyChangeType buffEvent_PropertyChangeType;
        int fixedValue;
        int percentValue;

        protected override void AddTriggerEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BeforeUseSkill:
                    owner.BattleBuffController.BeforeUseSkill += Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeAttack:
                    owner.BattleBuffController.BeforeAttackEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BehindAttack:
                    owner.BattleBuffController.BehindAttackEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent += Trigger;
                    break;
            }
        }
        public override void RemoveEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BeforeUseSkill:
                    owner.BattleBuffController.BeforeUseSkill -= Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeAttack:
                    owner.BattleBuffController.BeforeAttackEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BehindAttack:
                    owner.BattleBuffController.BehindAttackEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent -= Trigger;
                    break;
            }
        }

        protected override void TriggerEventMethod(BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            switch (buffEvent_PropertyChangeType)
            {
                case BuffEvent_PropertyChangeType.DamageAddition:
                    skillAdditionData.DamgeAddition += percentValue;
                    break;
                case BuffEvent_PropertyChangeType.DamageDeduction:
                    skillAdditionData.DamagDeduction += percentValue;
                    break;
                case BuffEvent_PropertyChangeType.IgnoreDefend:
                    skillAdditionData.IgnoreDefensive += percentValue;
                    break;
                case BuffEvent_PropertyChangeType.BasicDodgeRate:
                    skillAdditionData.DodgeProp += percentValue;
                    break;
            }
            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = (byte)buffEvent_PropertyChangeType;
            battleBuffEventTriggerDTO.Num_2 = percentValue;
        }

        public BattleBuffEvent_ChangeBuffProperty(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            buffEvent_PropertyChangeType = battleBuffEventData.buffPropertyChangeType;
            fixedValue = battleBuffEventData.fixedValue;
            percentValue = battleBuffEventData.percentValue;
        }
    }
}
