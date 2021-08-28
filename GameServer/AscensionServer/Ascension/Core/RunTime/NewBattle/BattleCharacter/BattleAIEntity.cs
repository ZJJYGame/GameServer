using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleAIEntity:BattleCharacterEntity
    {
        public void InitAI(int roomID, int aIID, int uniqueID,BattleFactionType battleFactionType)
        {
            Init();
            CharacterBattleData =ReferencePool.Accquire<CharacterBattleData>();
            GameEntry.DataManager.TryGetValue<Dictionary<int, MonsterDatas>>(out var monsterDict);
            if (monsterDict.ContainsKey(aIID))
                CharacterBattleData.Init(monsterDict[aIID],this);
            UniqueID = uniqueID;
            GlobalID = aIID;
            MasterID = -1;
            BattleFactionType= battleFactionType;
            Name = monsterDict[aIID].Monster_Name;
            RoomID = roomID;
            BattleCharacterType = BattleCharacterType.AI;
        }

        public override CharacterBattleDataDTO ToBattleDataBase()
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, MonsterDatas>>(out var monsterDict);
            CharacterBattleDataDTO characterBattleDataDTO = new CharacterBattleDataDTO()
            {
                UniqueId = UniqueID,
                GlobalId = GlobalID,
                MasterId = 0,
                ModelPath = monsterDict[GlobalID].Moster_Model,
                CharacterName = Name,
                MaxHealth = CharacterBattleData.MaxHp,
                Health = CharacterBattleData.Hp,
                MaxZhenYuan = CharacterBattleData.MaxMp,
                ZhenYuan = CharacterBattleData.Mp,
                MaxShenHun = CharacterBattleData.MaxSoul,
                ShenHun = CharacterBattleData.Soul,
                MaxJingXue = (short)CharacterBattleData.BestBloodMax,
                JingXue = (short)CharacterBattleData.BestBloodMax,
            };
            return characterBattleDataDTO;
        }

        public override void AllocationBattleAction()
        {
            base.AllocationBattleAction();
            //todo 先临时将AI的行为设置为普通攻击
            BattleCmd = BattleCmd.SkillInstruction;
            ActionID = 21114;
            //ActionID = 21001;
            TargetIDList.Clear();
            //指令决定前buff触发事件
            BattleBuffController.TriggerBuffEventBeforeAllocationAction();
            TargetIDList = GetSkillTargetIdList(ActionID,true,TargetIDList);
        }

        public override void Release()
        {
        }
    }
}
