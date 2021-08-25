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
    /// 护盾抵挡伤害类型buff事件
    /// </summary>
    public class BattleBuffEvent_ShieldResistDamages : BattleBuffEventBase
    {
        int shieldValue;

        protected override void AddTriggerEvent()
        {
            owner.BattleBuffController.BeforePropertyChangeEvent += Trigger;
        }
        public override void RemoveEvent()
        {
            owner.BattleBuffController.BeforePropertyChangeEvent -= Trigger;
        }
        protected override void TriggerEventMethod( BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            if (battleDamageData.battleSkillActionType != BattleSkillActionType.Damage)
                return;
            if (battleDamageData.damageType != BattleSkillDamageType.Physic && battleDamageData.damageType != BattleSkillDamageType.Magic)
                return;
            //护盾抵消值
            int counteractValue;
            if (Math.Abs(battleDamageData.damageNum) < shieldValue)//伤害值小于护盾值
            {
                counteractValue= battleDamageData.damageNum;
                battleDamageData.shieldDamage = battleDamageData.damageNum;
                battleDamageData.damageNum = 0;
                shieldValue += battleDamageData.shieldDamage;
            }
            else//伤害值大于护盾值
            {
                counteractValue = shieldValue;
                battleDamageData.shieldDamage = -shieldValue;
                battleDamageData.damageNum += shieldValue;
                shieldValue = 0;
                owner.BattleBuffController.RemoveBuff(battleBuffObj);
            }

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID, owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = counteractValue;
            battleBuffEventTriggerDTO.Num_2 = 0;
        }

        public BattleBuffEvent_ShieldResistDamages(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            Utility.Debug.LogError(battleBuffObj.BuffId);
            if (battleBuffEventData.flag)//数据来自于buff挂载的人
            {
                shieldValue= battleBuffObj.Owner.CharacterBattleData.GetProperty(battleBuffEventData.buffEvent_Shield_SourceDataType);
            }else//施加buff的人
            {
                shieldValue = battleBuffObj.OrginRole.CharacterBattleData.GetProperty(battleBuffEventData.buffEvent_Shield_SourceDataType);
            }
            shieldValue = shieldValue * battleBuffEventData.percentValue / 100 + battleBuffEventData.fixedValue;
            battleBuffTriggerTime = BattleBuffTriggerTime.BeforeOnHit;
        }
    }
}
