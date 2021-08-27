using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionServer.Model;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattlePetEntity:BattleCharacterEntity
    {


        public void InitPet(int roomID, int roleID,BattleFactionType battleFactionType)
        {
            Init();
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            RolePet rolePet = NHibernateQuerier.CriteriaSelect<RolePet>(nHCriteriaRoleID);
            if (rolePet.PetIsBattle == 0)
                return;
            NHCriteria nHCriteriaPetID =ReferencePool.Accquire<NHCriteria>().SetValue("PetID", rolePet.PetIsBattle);
            NHCriteria nHCriteriaPet =ReferencePool.Accquire<NHCriteria>().SetValue("ID", rolePet.PetIsBattle);
            PetStatus petStatus = NHibernateQuerier.CriteriaSelect<PetStatus>(nHCriteriaPetID);
            Pet pet= NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriaPet);
            //todo 拿取宠物数据
            CharacterBattleData =ReferencePool.Accquire<CharacterBattleData>();
            CharacterBattleData.Init(petStatus,this);
            UniqueID = petStatus.PetID;
            GlobalID = Utility.Json.ToObject<Dictionary<int,int>>(rolePet.PetIDDict) [UniqueID];
            MasterID = roleID;
            BattleFactionType = battleFactionType;
            Name = pet.PetName;
            RoomID = roomID;
        }


        public override CharacterBattleDataDTO ToBattleDataBase()
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, MonsterDatas>>(out var monsterDict);
            CharacterBattleDataDTO characterBattleDataDTO = new CharacterBattleDataDTO()
            {
                UniqueId = UniqueID,
                GlobalId = 0,
                MasterId = MasterID,
                ModelPath = monsterDict[22012].Moster_Model,
                CharacterName = Name,
                MaxHealth = CharacterBattleData.MaxHp,
                Health = CharacterBattleData.Hp,
                MaxZhenYuan = CharacterBattleData.MaxMp,
                ZhenYuan = CharacterBattleData.Mp,
                MaxShenHun = CharacterBattleData.MaxSoul,
                ShenHun = CharacterBattleData.Soul,
                MaxJingXue = 0,
                JingXue = 0
            };
            return characterBattleDataDTO;
        }       

        public override void AllocationBattleAction()
        {
            base.AllocationBattleAction();
            //todo 先临时将AI的行为设置为普通攻击
            //指令决定前buff触发事件
            BattleBuffController.TriggerBuffEventBeforeAllocationAction();

            TargetIDList = GetSkillTargetIdList(ActionID, true, TargetIDList);
        }

        public override void SetBattleAction(BattleCmd battleCmd, BattleTransferDTO battleTransferDTO)
        {
            TargetIDList.Clear();
            BattleCmd = battleCmd;
            ActionID = battleTransferDTO.ClientCmdId;
            for (int i = 0; i < battleTransferDTO.TargetInfos.Count; i++)
            {
                TargetIDList.Add(battleTransferDTO.TargetInfos[i].TargetID);
            }
        }

        public override void Release()
        {
        }
    }
}
