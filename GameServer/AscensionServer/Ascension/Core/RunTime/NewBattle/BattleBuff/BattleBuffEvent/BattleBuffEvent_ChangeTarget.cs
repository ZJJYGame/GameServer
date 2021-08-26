using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleBuffEvent_ChangeTarget : BattleBuffEventBase
    {
        BuffEvent_ChangeTarget_TargetType buffEvent_ChangeTarget_TargetType;
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

            Utility.Debug.LogError("改变目标事件触发");
            switch (buffEvent_ChangeTarget_TargetType)
            {
                case BuffEvent_ChangeTarget_TargetType.BuffOrginer:
                    owner.TargetIDList.Clear();
                    BattleCharacterEntity targetEntity = GameEntry.BattleCharacterManager.GetCharacterEntity(battleBuffObj.OrginRole.UniqueID);
                    if (!targetEntity.HasDie)
                    {
                        owner.TargetIDList.Add(battleBuffObj.OrginRole.UniqueID);
                        //BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID, owner.UniqueID);
                        //battleBuffEventTriggerDTO.Num_2 = targetEntity.UniqueID;
                    }
                    break;
                case BuffEvent_ChangeTarget_TargetType.RnadomFromAll:
                    List<int> targetList= BattleRoomEntity.BattleController.RandomGetTarget(1, BattleFactionType.All, true, true, null, new List<int>() { owner.UniqueID });

                    owner.TargetIDList = targetList;
                    break; 
            }
        }

        public BattleBuffEvent_ChangeTarget(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            buffEvent_ChangeTarget_TargetType = battleBuffEventData.buffEvent_ChangeTarget_TargetType;
            battleBuffTriggerTime = BattleBuffTriggerTime.BeforeAllocation;
        }
    }
}
