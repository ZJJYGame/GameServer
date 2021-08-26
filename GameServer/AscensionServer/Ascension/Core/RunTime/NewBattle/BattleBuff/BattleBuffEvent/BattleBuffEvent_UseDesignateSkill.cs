using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 使用指定技能
    /// </summary>
    public class BattleBuffEvent_UseDesignateSkill : BattleBuffEventBase
    {
        int skillID;
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
                case BattleBuffTriggerTime.BehindUseSkill:
                    owner.BattleBuffController.BehindUseSkill += Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.BehindOnHit:
                    owner.BattleBuffController.BehindOnHitEvent += Trigger;
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
                case BattleBuffTriggerTime.BehindUseSkill:
                    owner.BattleBuffController.BehindUseSkill -= Trigger;
                    break;
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.BehindOnHit:
                    owner.BattleBuffController.BehindOnHitEvent -= Trigger;
                    break;
            }
        }
        protected override void TriggerEventMethod(BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            List<int> tempTargetList;
            if (target != null)
                tempTargetList = owner.GetTargetIdList(skillID, false, new List<int>() { target.UniqueID });
            else
                tempTargetList = owner.GetTargetIdList(skillID, false);

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(tempTargetList[0],owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = skillID;

            //用于恢复记录当前使用的技能
            int oldUseSkillId = owner.BattleSkillController.nowUseSkillId;
            owner.BattleSkillController.UseSkill(skillID, tempTargetList,false);
            owner.BattleSkillController.nowUseSkillId = oldUseSkillId;
        }
        public BattleBuffEvent_UseDesignateSkill(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            skillID = battleBuffEventData.id;
            percentValue = battleBuffEventData.percentValue;
        }
    }
}
