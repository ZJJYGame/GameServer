using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using Cosmos;

namespace AscensionServer
{
    public class BattleBuffEvent_ChangeCmd : BattleBuffEventBase
    {
        int skillId;

        protected override void AddTriggerEvent()
        {
            owner.BattleBuffController.BeforeAllocationActionEvent += Trigger;
        }
        public override void RemoveEvent()
        {
            owner.BattleBuffController.BeforeAllocationActionEvent -= Trigger;
        }


        protected override void TriggerEventMethod(BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            owner.BattleCmd = BattleCmd.SkillInstruction;
            owner.ActionID = skillId;

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = (byte)BattleCmd.SkillInstruction;
            battleBuffEventTriggerDTO.Num_2 = skillId;
        }


        public BattleBuffEvent_ChangeCmd(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            skillId = battleBuffEventData.id;
        }
    }
}
