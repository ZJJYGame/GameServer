using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleBuffEventBase
    {
        protected BattleCharacterEntity owner;
        protected BattleBuffObj battleBuffObj;
        int prob;
        BattleBuffEventConditionBase battleBuffEventConditionBase;
        protected BattleBuffTriggerTime battleBuffTriggerTime;
        protected int triggerCount;
        protected int maxTriggerCount;
        protected BattleBuffEventData battleBuffEventData;
        

        protected int OverlayLayer { get { return battleBuffObj.OverlayLayer; } }

        bool CanTrigger(BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            int randomValue = Utility.Algorithm.RandomRange(0, 99);
            if (randomValue >= prob)
                return false;
            if (!battleBuffObj.CanTrigger(target,battleDamageData))
                return false;
            return true;
            //return battleBuffEventConditionBase.CanTrigger();
        }
        /// <summary>
        /// 具体buff触发事件实现
        /// </summary>
        /// <param name="skillAdditionData">用于记录临时的加成数据，比如技能加成</param>
        public void Trigger(BattleTransferDTO battleTransferDTO, BattleCharacterEntity target,BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {

            if (owner.BattleBuffController.ForbiddenBuff.Contains(battleBuffObj.BuffId))//该buff被禁用，返回
                return;
            if (!CanTrigger(target, battleDamageData))
                return;
            //最大触发次数超过限制
            triggerCount++;
            if (maxTriggerCount != -1 && triggerCount >= maxTriggerCount)
                triggerCount = maxTriggerCount;
            TriggerEventMethod(battleTransferDTO,target, battleDamageData, skillAdditionData);

        }
        //恢复到未触发
        public void Recover()
        {
            if (triggerCount==0)
                return;
            triggerCount = 0;
            TriggerEventMethod(null,null,null,null);

        }
        protected virtual void TriggerEventMethod(BattleTransferDTO battleTransferDTO, BattleCharacterEntity target, BattleDamageData battleDamageData,ISkillAdditionData skillAdditionData) { }
        protected virtual void RecoverEventMethod() { }
        protected virtual void AddTriggerEvent()
        {
            
        }
        public virtual void RemoveEvent()
        {
           
        }
        /// <summary>
        /// buff覆盖时重新设置buff参数
        /// </summary>
        public virtual void SetValue(BattleBuffEventData battleBuffEventData,BattleSkillAddBuffValue battleSkillAddBuffValue )
        {

        }
        /// <summary>
        /// 转为发送数据
        /// </summary>
        protected virtual void ToTransferData()
        {

        }
        public BattleBuffEventBase(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj)
        {
            this.battleBuffEventData = battleBuffEventData;
            battleBuffTriggerTime = battleBuffEventData.battleBuffTriggerTime;
            prob = battleBuffEventData.probability;
            this.battleBuffObj = battleBuffObj;
            owner = battleBuffObj.Owner;
            maxTriggerCount = battleBuffEventData.maxTriggerCount;
            AddTriggerEvent();
        }
    }

 
}
