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
    /// 分担伤害buff事件
    /// </summary>
    public class BattleBuffEvent_ShareDamageForOther : BattleBuffEventBase
    {
        int sharePercent;
        protected override void AddTriggerEvent()
        {
            List<BattleCharacterEntity> battleCharacterEntities = GetFriendEntity();
            switch (battleBuffEventData.buffEvent_TakeDamageForOther_TargetType)
            {
                case BuffEvent_TakeDamageForOther_TargetType.Master:
                    battleCharacterEntities = GetMasterEntity();
                    break;
                case BuffEvent_TakeDamageForOther_TargetType.AnyTeammate:
                    battleCharacterEntities = GetFriendEntity();
                    break;
            }
            for (int i = 0; i < battleCharacterEntities.Count; i++)
            {
                battleCharacterEntities[i].BattleBuffController.BeforeOnHitEvent += Trigger;
            }

        }
        public override void RemoveEvent()
        {
            List<BattleCharacterEntity> battleCharacterEntities = GetFriendEntity();
            switch (battleBuffEventData.buffEvent_TakeDamageForOther_TargetType)
            {
                case BuffEvent_TakeDamageForOther_TargetType.Master:
                    battleCharacterEntities = GetMasterEntity();
                    break;
                case BuffEvent_TakeDamageForOther_TargetType.AnyTeammate:
                    battleCharacterEntities = GetFriendEntity();
                    break;
            }
            for (int i = 0; i < battleCharacterEntities.Count; i++)
            {
                battleCharacterEntities[i].BattleBuffController.BeforeOnHitEvent -= Trigger;
            }
        }
        protected override void TriggerEventMethod( BattleCharacterEntity target, BattleDamageData battleDamageData, ISkillAdditionData skillAdditionData)
        {
            //自己为别人承担伤害

            BattleDamageData newBattleDamageData = new BattleDamageData();
            newBattleDamageData.TargetID = owner.UniqueID;
            newBattleDamageData.damageNum = battleDamageData.damageNum * sharePercent / 100;
            newBattleDamageData.battleSkillActionType = BattleSkillActionType.Damage;
            battleDamageData.damageNum = battleDamageData.damageNum - newBattleDamageData.damageNum;
            owner.OnActionEffect(newBattleDamageData);

            if (LastBattleTransfer.TargetInfos == null)
                LastBattleTransfer.TargetInfos = new List<TargetInfoDTO>();
            LastBattleTransfer.TargetInfos.Add(new TargetInfoDTO()
            {
                TargetID = owner.UniqueID,
                TargetHPDamage = newBattleDamageData.damageNum,
            });

            BattleBuffEventTriggerDTO battleBuffEventTriggerDTO = GetBuffEventTriggerDTO(target.UniqueID);
            battleBuffEventTriggerDTO.Num_1 = newBattleDamageData.damageNum;
        }


        List<BattleCharacterEntity> GetFriendEntity()
        {
            List<BattleCharacterEntity> battleCharacterEntities = new List<BattleCharacterEntity>();
            for (int i = 0; i < owner.FriendCharacterEntities.Count; i++)
            {
                if (owner.FriendCharacterEntities[i] != owner)
                    battleCharacterEntities.Add(owner.FriendCharacterEntities[i]);
            }
            return battleCharacterEntities;
        }
        List<BattleCharacterEntity> GetMasterEntity()
        {
            List<BattleCharacterEntity> battleCharacterEntities = new List<BattleCharacterEntity>();
            if (owner.MasterID != -1)
                battleCharacterEntities.Add(GameEntry.BattleCharacterManager.GetCharacterEntity(owner.MasterID));
            return battleCharacterEntities;
        }
        public BattleBuffEvent_ShareDamageForOther(BattleBuffEventData battleBuffEventData, BattleBuffObj battleBuffObj) : base(battleBuffEventData, battleBuffObj)
        {
            //为主人承担伤害
            //为任意一个目标抵挡伤害

            sharePercent = battleBuffEventData.percentValue;
        }
    }
}
