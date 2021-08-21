using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleController : IReference
    {
        BattleRoomEntity battleRoomEntity;

        //所有角色列表
        public List<BattleCharacterEntity> AllCharacterEntities { get; private set; }
        //阵营一角色列表
        public List<BattleCharacterEntity> FactionOneCharacterEntites { get;private set; }
        //阵营二角色列表
        public List<BattleCharacterEntity> FactionTwoCharacterEntites { get; private set; }

        Action< BattleCharacterEntity,BattleDamageData, ISkillAdditionData> roundStartEvent;
        public event Action< BattleCharacterEntity, BattleDamageData, ISkillAdditionData> RoundStartEvent
        {
            add { roundStartEvent += value; }
            remove { roundStartEvent -= value; }
        }
        //回合结束事件,在roundFinishEvent之前，处理buff事件
        Action< BattleCharacterEntity, BattleDamageData, ISkillAdditionData> roundEndEvent;
        public event Action< BattleCharacterEntity, BattleDamageData, ISkillAdditionData> RoundEndEvent
        {
            add { roundEndEvent += value; }
            remove { roundEndEvent -= value; }
        }
        //回合结束事件，用来处理冷却，持续时间
        Action roundFinishEvent;
        public event Action RoundFinishEvent
        {
            add { roundFinishEvent += value; }
            remove { roundFinishEvent -= value; }
        }

        public void AddCharacterEntity(BattleCharacterEntity battleCharacterEntity)
        {
            AllCharacterEntities.Add(battleCharacterEntity);
            switch (battleCharacterEntity.BattleFactionType)
            {
                case BattleFactionType.FactionOne:
                    FactionOneCharacterEntites.Add(battleCharacterEntity);
                    break;
                case BattleFactionType.FactionTwo:
                    FactionTwoCharacterEntites.Add(battleCharacterEntity);
                    break;
            }
            battleCharacterEntity.SetFriendAndEnemy(FactionOneCharacterEntites, FactionTwoCharacterEntites);
        }
        public void InitController(BattleRoomEntity battleRoomEntity)
        {
            AllCharacterEntities = new List<BattleCharacterEntity>();
            FactionOneCharacterEntites = new List<BattleCharacterEntity>();
            FactionTwoCharacterEntites = new List<BattleCharacterEntity>();
            this.battleRoomEntity = battleRoomEntity;
        }

        public void StartBattle()
        {
            List<BattleTransferDTO> battleTransferDTOs = battleRoomEntity.BattleTransferDTOList;
            battleTransferDTOs.Clear();
            //角色按速度排序
            AllCharacterEntities.Sort();

            //按角色出手顺序开始出手
            int allCount = AllCharacterEntities.Count;
            BattleCharacterEntity actCharacter;

            //回合开始事件统一触发
            BattleTransferDTO roundStartTransfer= battleRoomEntity.SpawnBattleTransfer();
            roundStartTransfer.RoleId = -1;
            Utility.Debug.LogError("回合开始事件触发");
            roundStartEvent?.Invoke(null,null,null);
            battleRoomEntity.ReleaseBattleTransfer();

            for (int i = 0; i < allCount; i++)
            {
                //检测是否有一个阵营全部死亡

                actCharacter = AllCharacterEntities[i];

                //分配角色行为
                actCharacter.AllocationBattleAction();
                //开始计算角色行动
                actCharacter.Action();
            }

            //回合结束事件统一开始触发
            BattleTransferDTO roundEndTransfer=battleRoomEntity.SpawnBattleTransfer();
            roundEndTransfer.RoleId = -1;
            roundEndEvent?.Invoke(null,null,null);
            battleRoomEntity.ReleaseBattleTransfer();

            //处理回合计数
            roundFinishEvent?.Invoke();

            battleRoomEntity.SendBattlePerformDataS2C(battleTransferDTOs);
        }

        /// <summary>
        /// 随机获得行为目标列表
        /// </summary>
        /// <param name="count"></param>
        /// <param name="battleSkillFactionType"></param>
        /// <param name="targetIsAlive">目标是否是活着的状态</param>
        /// <param name="targetIDList"></param>
        /// <returns></returns>
        public List<int> RandomGetTarget(int count,BattleFactionType battleSkillFactionType,bool targetIsAlive,bool isAutoChangeTarget,List<int> targetIDList=null)
        {
            List<int> resultList = new List<int>();
            List<BattleCharacterEntity> targetCharacterList;
            if (battleSkillFactionType == BattleFactionType.FactionOne)
                targetCharacterList = FactionOneCharacterEntites;
            else
                targetCharacterList = FactionTwoCharacterEntites;
            //筛选出未死亡或者死亡的目标列表
            List<BattleCharacterEntity> battleCharacterEntities = new List<BattleCharacterEntity>();
            for (int i = 0; i < targetCharacterList.Count; i++)
            {
                if (targetCharacterList[i].HasDie && !targetIsAlive)
                    battleCharacterEntities.Add(targetCharacterList[i]);
                else if (!targetCharacterList[i].HasDie && targetIsAlive)
                    battleCharacterEntities.Add(targetCharacterList[i]);
            }
            targetCharacterList = battleCharacterEntities;
            //开始随机目标
            List<int> AllIndexList = new List<int>();
            for (int i = 0; i < targetCharacterList.Count; i++)
            {
                AllIndexList.Add(i);
            }
            if (targetIDList != null) {
                for (int i = 0; i < targetIDList.Count; i++)
                {
                    //判断当前选中目标是否可以作为目标
                    BattleCharacterEntity tempEntity = targetCharacterList.Find((k) => k.UniqueID == targetIDList[i]);
                    if (tempEntity==null&&isAutoChangeTarget)
                        continue;
                    AllIndexList.Remove(targetCharacterList.IndexOf(tempEntity));
                    resultList.Add(targetIDList[i]);
                }

            }

            while (resultList.Count < count)
            {
                if (AllIndexList.Count == 0)
                    break;
                int randomValue = Utility.Algorithm.RandomRange(0, AllIndexList.Count);
                int value = AllIndexList[randomValue];
                resultList.Add(targetCharacterList[value].UniqueID);
                AllIndexList.RemoveAt(randomValue);
            }

            return resultList;
        }

        /// <summary>
        /// 判断战斗是否结束
        /// 双方阵营有一边全部死亡则代表战斗结束
        /// </summary>
        /// <returns></returns>
        public bool BattleIsEnd()
        {
            bool resultFlagOne = true;
            for (int i = 0; i < FactionOneCharacterEntites.Count; i++)
            {
                resultFlagOne = resultFlagOne && FactionOneCharacterEntites[i].HasDie;
            }
            bool resultFlagTwo = true;
            for (int i = 0; i < FactionTwoCharacterEntites.Count; i++)
            {
                resultFlagTwo = resultFlagTwo && FactionTwoCharacterEntites[i].HasDie;
            }
            //如果战斗结束，回调战斗胜利失败信息
            if(resultFlagOne || resultFlagTwo)
            {
                BattleResultInfo battleResultInfoOne = new BattleResultInfo();
                BattleResultInfo battleResultInfoTwo = new BattleResultInfo();
                BattleResultInfo[] battleResultInfos = new BattleResultInfo[2] { battleResultInfoOne , battleResultInfoTwo };
                battleResultInfoOne.isWin = !resultFlagOne;
                battleResultInfoOne.CharacterId = GetCharactID2Adventure(FactionOneCharacterEntites);
                battleResultInfoTwo.isWin = !resultFlagTwo;
                battleResultInfoTwo.CharacterId = GetCharactID2Adventure(FactionTwoCharacterEntites);
                battleRoomEntity.onBattleEnd?.Invoke(battleResultInfos);
            }
            return resultFlagOne || resultFlagTwo;
        }

        int[] GetCharactID2Adventure(List<BattleCharacterEntity> battleCharacterEntities)
        {
            int[] arr = new int[battleCharacterEntities.Count];
            for (int i = 0; i < battleCharacterEntities.Count; i++)
            {
                switch (battleCharacterEntities[i].GetType().Name)
                {
                    case "BattlePlayerEntity":
                        arr[i] = battleCharacterEntities[i].UniqueID;
                        break;
                    case "BattlePetEntity":
                        arr[i] = battleCharacterEntities[i].UniqueID;
                        break;
                    case "BattleAIEntity":
                        arr[i] = battleCharacterEntities[i].GlobalID;
                        break;
                }
            }
            return arr;
        }

        public void Release()
        {
        }
    }
}
