using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleBuffEvent_ImmuneBuff: BattleBuffEventBase
    {
        List<int> immuneBuffIdList;
        protected override void AddTriggerEvent()
        {
            battleBuffObj.BuffAddEvent += Trigger;
            battleBuffObj.BuffRemoveEvent += RecoverEventMethod;
        }
        public override void RemoveEvent()
        {
            battleBuffObj.BuffAddEvent -= Trigger;
            battleBuffObj.BuffRemoveEvent -= RecoverEventMethod;
        }
        protected override void TriggerEventMethod(BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            HashSet<int> immuneBuffIdHash = owner.BattleBuffController.ImmuneBuffId;
            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID, owner.UniqueID);
            for (int i = 0; i < immuneBuffIdList.Count; i++)
            {
                if (!immuneBuffIdHash.Contains(immuneBuffIdList[i]))
                {
                    immuneBuffIdHash.Add(immuneBuffIdList[i]);
                    //移除所有该id的buff
                    owner.BattleBuffController.RemoveBuff(immuneBuffIdList[i]);
                }
                battleBuffEventTriggerDTO.BuffDTOList.Add(new AddBuffDTO() { BuffId = immuneBuffIdList[i] });
            }
        }
        protected override void RecoverEventMethod()
        {
            HashSet<int> immuneBuffIdHash = owner.BattleBuffController.ImmuneBuffId;
            for (int i = 0; i < immuneBuffIdList.Count; i++)
            {
                if (immuneBuffIdHash.Contains(immuneBuffIdList[i]))
                {
                    immuneBuffIdHash.Remove(immuneBuffIdList[i]);
                }
            }
        }
        public BattleBuffEvent_ImmuneBuff(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            immuneBuffIdList = battleBuffEventData.idList;
        }
    }
}
