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
    /// 驱散buff事件
    /// </summary>
    public class BattleBuffEvent_DispelBuff : BattleBuffEventBase
    {
        List<int> dispelBuffList;
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
            Utility.Debug.LogError("buff驱散事件触发");
            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID);
            for (int i = 0; i < dispelBuffList.Count; i++)
            {
                owner.BattleBuffController.RemoveBuff(dispelBuffList[i]);
                battleBuffEventTriggerDTO.BuffDTOList.Add(new AddBuffDTO() { BuffId = dispelBuffList[i] });
            }
        }
        public BattleBuffEvent_DispelBuff(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            Utility.Debug.LogError("驱散buff添加");
            dispelBuffList = battleBuffEventData.idList;
        }
    }
}
