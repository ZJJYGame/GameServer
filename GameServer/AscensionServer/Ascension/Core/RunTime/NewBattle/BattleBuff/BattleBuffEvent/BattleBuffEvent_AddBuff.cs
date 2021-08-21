using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleBuffEvent_AddBuff : BattleBuffEventBase
    {
        //添加buff目标:是=>目标,否=>自身
        bool isTarget;
        BattleSkillAddBuffData battleSkillAddBuffData;

        protected override void AddTriggerEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BuffAdd:
                    battleBuffObj.BuffAddEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.RoundStart:
                    battleBuffObj.BattleController.RoundStartEvent += Trigger;
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
        }
        public override void RemoveEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BuffAdd:
                    battleBuffObj.BuffAddEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.RoundStart:
                    battleBuffObj.BattleController.RoundStartEvent -= Trigger;
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
        }
        protected override void TriggerEventMethod( BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            BattleCharacterEntity tempEntity;
            if (isTarget)
                tempEntity = target;
            else
                tempEntity = owner;
            BattleBuffObj tempbattleBuffObj= tempEntity.BattleBuffController.AddBuff(battleSkillAddBuffData, battleBuffObj.OwnerSkill);

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(tempEntity.UniqueID, owner.UniqueID);
            battleBuffEventTriggerDTO.BuffDTOList.Add(new AddBuffDTO(tempEntity.UniqueID, tempbattleBuffObj.BuffId, tempbattleBuffObj.MaxRound));
        }
        public BattleBuffEvent_AddBuff(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            isTarget = battleBuffEventData.flag;
            battleSkillAddBuffData = battleBuffEventData.battleSkillAddBuffData;
        }
    }
}
