using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionServer.Model;

namespace AscensionServer
{
    [Module]
    public class BattleCharacterManager:Module,IBattleCharacterManager
    {

        // 角色实体对象缓存字典
        // key=>角色唯一ID，value=>角色实体对象
        public Dictionary<int, BattleCharacterEntity> CharacterEntityDict { get; private set; } = new Dictionary<int, BattleCharacterEntity>();

        //ai的唯一ID起始分配点
         int aIStartID = 1000;

        /// <summary>
        /// 添加玩家角色
        /// </summary>
        public BattlePlayerEntity AddPlayerCharacter(int roomID,int roleID,BattleFactionType battleFactionType)
        {
            BattlePlayerEntity battleCharacterEntity =ReferencePool.Accquire<BattlePlayerEntity>();
            battleCharacterEntity.InitPlayer(roomID,roleID,battleFactionType);
            CharacterEntityDict[battleCharacterEntity.UniqueID] = battleCharacterEntity;
            return battleCharacterEntity;
        }
        /// <summary>
        /// 添加宠物角色
        /// </summary>
        public BattlePetEntity AddPetCharacter(int roomID,int roleID,BattleFactionType battleFactionType)
        {
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            RolePet rolePet = NHibernateQuerier.CriteriaSelect<RolePet>(nHCriteriaRoleID);
            if (rolePet.PetIsBattle == 0)
                return null;
            BattlePetEntity battleCharacterEntity =ReferencePool.Accquire<BattlePetEntity>();
            battleCharacterEntity.InitPet(roomID, roleID, battleFactionType);
            CharacterEntityDict[battleCharacterEntity.UniqueID] = battleCharacterEntity;
            return battleCharacterEntity;
        }
        /// <summary>
        /// 添加Ai角色
        /// </summary>
        public BattleAIEntity AddAICharacter(int roomID,int aIID,BattleFactionType battleFactionType)
        {
            BattleAIEntity battleCharacterEntity =ReferencePool.Accquire<BattleAIEntity>();
            battleCharacterEntity.InitAI(roomID,aIID, aIStartID++, battleFactionType);
            CharacterEntityDict[battleCharacterEntity.UniqueID] = battleCharacterEntity;
            return battleCharacterEntity;
        }

        public BattleCharacterEntity GetCharacterEntity(int uniqueID)
        {
            return CharacterEntityDict[uniqueID];
        }
        /// <summary>
        /// 移除角色
        /// </summary>
        public void DestoryCharacter(int roleID)
        {
            ReferencePool.Release(CharacterEntityDict[roleID]);
            CharacterEntityDict.Remove(roleID);
        }

    }
}
