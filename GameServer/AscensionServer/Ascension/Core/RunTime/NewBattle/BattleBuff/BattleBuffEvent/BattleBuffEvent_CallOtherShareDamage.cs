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
    /// 喊别人为我承担伤害
    /// </summary>
    public class BattleBuffEvent_CallOtherShareDamage : BattleBuffEventBase
    {
        int sharePercent;
        BuffEvent_CallOterShareDamage_TargetType buffEvent_CallOterShareDamage_TargetType;

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
                    owner.BattleBuffController.BeforeOnHitEvent += Trigger;
                    break;
                case BattleBuffTriggerTime.RoleBeforeDie:
                    owner.BattleBuffController.RoleBeforeDieEvent += Trigger;
                    break;
            }
        }
        protected override void TriggerEventMethod( BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            BattleCharacterEntity targetEntity = GetTargetEntity();
            if (targetEntity.HasDie)
                return;

            BattleDamageData newBattleDamageData = new BattleDamageData();
            newBattleDamageData.TargetID = targetEntity.UniqueID;
            newBattleDamageData.damageNum = battleDamageData.damageNum * sharePercent / 100;
            newBattleDamageData.battleSkillActionType = BattleSkillActionType.Damage;
            battleDamageData.damageNum = battleDamageData.damageNum - newBattleDamageData.damageNum;

            //将目标的同种buff禁用
            targetEntity.BattleBuffController.ForbiddenBuff.Add(battleBuffObj.BuffId);
            targetEntity.OnActionEffect(newBattleDamageData);
            targetEntity.BattleBuffController.ForbiddenBuff.Remove(battleBuffObj.BuffId);

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(targetEntity.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = newBattleDamageData.damageNum;
        }

        BattleCharacterEntity GetTargetEntity()
        {
            BattleCharacterEntity result = null ;
            switch (buffEvent_CallOterShareDamage_TargetType)
            {
                case BuffEvent_CallOterShareDamage_TargetType.MaxHpCharacter:
                    List<BattleCharacterEntity> tempEntityList = new List<BattleCharacterEntity>();
                    for (int i = 0; i < owner.FriendCharacterEntities.Count; i++)
                    {
                        if (owner.FriendCharacterEntities[i].BattleBuffController.HasBuff(battleBuffObj.BuffId)
                            && owner!= owner.FriendCharacterEntities[i])//都有该buff
                            tempEntityList.Add(owner.FriendCharacterEntities[i]);
                    }
                    result= Utility.Algorithm.Max(tempEntityList.ToArray(), p => p.CharacterBattleData.Hp);
                    break;
                case BuffEvent_CallOterShareDamage_TargetType.BuffOrgin:
                    result = battleBuffObj.OrginRole;
                    break;
            }
            return result;
        }

        public BattleBuffEvent_CallOtherShareDamage(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            //喊血量最高的人承担伤害
            //buff的施加者承担
            sharePercent = battleBuffEventData.percentValue;
        }
    }
}
