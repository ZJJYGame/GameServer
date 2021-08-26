using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using Cosmos;
namespace AscensionServer
{
    /// <summary>
    /// 添加护盾buff事件
    /// </summary>
    public class BattleBuffEvent_ShieldAdd : BattleBuffEventBase
    {
        int shieldValue;

        protected override void AddTriggerEvent()
        {
            battleBuffObj.BuffAddEvent += AddBuffEvent;
        }
        public override void RemoveEvent()
        {
            battleBuffObj.BuffAddEvent -= AddBuffEvent;
        }
        protected void AddBuffEvent(BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(owner.UniqueID, owner.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = shieldValue;
            battleBuffEventTriggerDTO.Num_2 = 1;
        }

        public BattleBuffEvent_ShieldAdd(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            if (battleBuffEventData.flag)//数据来自于buff挂载的人
            {
                shieldValue = battleBuffObj.Owner.CharacterBattleData.GetProperty(battleBuffEventData.buffEvent_Shield_SourceDataType);
            }
            else//施加buff的人
            {
                shieldValue = battleBuffObj.OrginRole.CharacterBattleData.GetProperty(battleBuffEventData.buffEvent_Shield_SourceDataType);
            }
            Utility.Debug.LogError(battleBuffEventData.buffEvent_Shield_SourceDataType);
            Utility.Debug.LogError(battleBuffEventData.percentValue);
            Utility.Debug.LogError(battleBuffEventData.fixedValue);
            shieldValue = shieldValue * battleBuffEventData.percentValue / 100 + battleBuffEventData.fixedValue;
            battleBuffTriggerTime = BattleBuffTriggerTime.BuffAdd;
        }
    }
}

