using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using Cosmos;

namespace AscensionServer
{
    public class BattleBuffEvent_DamageReduce : BattleBuffEventBase
    {
        int percentValue;
        int fixedValue;
        protected override void AddTriggerEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.RoleBeforeDie:
                    owner.BattleBuffController.RoleBeforeDieEvent += Trigger;
                    break;
            }
        }
        public override void RemoveEvent()
        {
            switch (battleBuffTriggerTime)
            {
                case BattleBuffTriggerTime.BeforeOnHit:
                    owner.BattleBuffController.BeforeOnHitEvent -= Trigger;
                    break;
                case BattleBuffTriggerTime.RoleBeforeDie:
                    owner.BattleBuffController.RoleBeforeDieEvent -= Trigger;
                    break;
            }
        }
        protected override void TriggerEventMethod( BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            Utility.Debug.LogError("触发伤害减免");
            BattleDamageData tempDamageData = owner.ReceiveBattleDamageData;
            if (tempDamageData.damageNum >= 0)
                return;

            int oldDamage = tempDamageData.damageNum;
            int newDamage= oldDamage * percentValue / 100 + fixedValue;
            newDamage = newDamage > 0 ? 0 : newDamage;
            tempDamageData.damageNum = newDamage;

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID, owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = newDamage - oldDamage;

        }

        public BattleBuffEvent_DamageReduce(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            percentValue = battleBuffEventData.percentValue;
            fixedValue = battleBuffEventData.fixedValue;
        }
    }
}
