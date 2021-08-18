using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;

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
            BattleDamageData tempDamageData = owner.ReceiveBattleDamageData;
            if (tempDamageData.damageNum >= 0)
                return;
            tempDamageData.damageNum = tempDamageData.damageNum * percentValue / 100 + fixedValue;
            tempDamageData.damageNum = tempDamageData.damageNum >= 0 ? -1 : tempDamageData.damageNum;

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID);
        }

        public BattleBuffEvent_DamageReduce(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            percentValue = battleBuffEventData.percentValue;
            fixedValue = battleBuffEventData.fixedValue;
        }
    }
}
