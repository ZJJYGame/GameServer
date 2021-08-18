using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 改变角色属性事件
    /// </summary>
    public class BattleBuffEvent_ChangeProperty : BattleBuffEventBase
    {
        BattleBuffEventType_RolePropertyChange battleBuffEventType_RolePropertyChange;
        BuffEvent_RolePropertyChange_SourceDataType buffRolePropertyChange_SourceDataType;
        int fixedValue;
        int percentValue;
        //记录buff变动的属性数值
        float changeValue;

        protected override void AddTriggerEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BuffAdd:
                    battleBuffObj.BuffAddEvent += Trigger;
                    owner.BattleBuffController.AfterPropertyChangeEvent+= Trigger;
                    break;
                case BattleBuffTriggerTime.RoundStart:
                    battleBuffObj.BattleController.RoundStartEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeAllocation:
                    break;
                case BattleBuffTriggerTime.BeforeUseSkill:
                    owner.BattleBuffController.BeforeUseSkill += Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeAttack:
                    owner.BattleBuffController.BeforeAttackEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BehindAttack:
                    owner.BattleBuffController.BehindAttackEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BehindUseSkill:
                    owner.BattleBuffController.BehindUseSkill += Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BehindOnHit:
                    owner.BattleBuffController.BehindOnHitEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.RoleBeforeDie:
                    owner.BattleBuffController.RoleBeforeDieEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.RoleAfterDie:
                    owner.BattleBuffController.RoleAfterDieEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.RoundEnd:
                    battleBuffObj.BattleController.RoundEndEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BuffRemove:
                    break;
            }
            battleBuffObj.BuffRemoveEvent += RecoverEventMethod;
            battleBuffObj.BuffCoverEvent += RecoverEventMethod;
        }

        public override void RemoveEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BuffAdd:
                    battleBuffObj.BuffAddEvent -= Trigger;
                    owner.BattleBuffController.AfterPropertyChangeEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.RoundStart:
                    battleBuffObj.BattleController.RoundStartEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeAllocation:
                    break;
                case BattleBuffTriggerTime.BeforeUseSkill:
                    owner.BattleBuffController.BeforeUseSkill -= Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeAttack:
                    owner.BattleBuffController.BeforeAttackEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BehindAttack:
                    owner.BattleBuffController.BehindAttackEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BehindUseSkill:
                    owner.BattleBuffController.BehindUseSkill -= Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BehindOnHit:
                    owner.BattleBuffController.BehindOnHitEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.RoleBeforeDie:
                    owner.BattleBuffController.RoleBeforeDieEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.RoleAfterDie:
                    owner.BattleBuffController.RoleAfterDieEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.RoundEnd:
                    battleBuffObj.BattleController.RoundEndEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BuffRemove:
                    break;
            }
            battleBuffObj.BuffRemoveEvent -= RecoverEventMethod;
            battleBuffObj.BuffCoverEvent -= RecoverEventMethod;
        }

        protected override void TriggerEventMethod( BattleCharacterEntity target,BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            float  baseValue = owner.CharacterBattleData.GetBaseProperty(buffRolePropertyChange_SourceDataType, battleBuffObj.OwnerSkill) * percentValue / 100 + fixedValue;
            float nowChangeValue = baseValue * OverlayLayer*triggerCount;
            owner.BattleBuffController.BuffCharacterData.ChangeProperty(nowChangeValue-changeValue, battleBuffEventType_RolePropertyChange);
            changeValue = nowChangeValue;

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = (byte)battleBuffEventType_RolePropertyChange;
            battleBuffEventTriggerDTO.Num_2 = (int)changeValue;
        }
        protected override void RecoverEventMethod()
        {
            owner.BattleBuffController.BuffCharacterData.ChangeProperty(-changeValue, battleBuffEventType_RolePropertyChange);
            changeValue = 0;
        }
        public BattleBuffEvent_ChangeProperty(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj, BattleSkillAddBuffValue battleSkillAddBuffValue) : base(battleBuffEventData, battleBuffObj)
        {
            battleBuffEventType_RolePropertyChange = battleBuffEventData.battleBuffEventType_RolePropertyChange;
            buffRolePropertyChange_SourceDataType = battleBuffEventData.buffRolePropertyChange_SourceDataType;
            fixedValue = battleBuffEventData.fixedValue + battleSkillAddBuffValue.fixedValue;
            percentValue = battleBuffEventData.percentValue + battleSkillAddBuffValue.percentValue;
        }
    }
}
