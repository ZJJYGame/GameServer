using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public partial class BattleRoomEntity : IReference
    {
        //房间内所有玩家的ID集合
        public HashSet<int> AllPlayerID { get; private set; } = new HashSet<int>() ;
        //已收集到指令的玩家ID
        public HashSet<int> HasGetCmdRoleID { get; private set; } = new HashSet<int>();
        //战斗角色字典，key=>角色ID，value=>战斗角色实体对象
        public Dictionary<int, BattleCharacterEntity> battleCharacterEntityDict { get; private set; } = new Dictionary<int, BattleCharacterEntity>();
        public int RoomId { get; private set; }
        public BattleController BattleController { get; private set; }
        //发送指令事件
        Action<OperationData> playerSendMsgEvent;
        public event Action<OperationData> PlayerSendMsgEvent
        {
            add { playerSendMsgEvent += value; }
            remove { playerSendMsgEvent -= value; }
        }

        /// <summary>
        /// 胜利的阵营---玩家id
        /// byte 1 表示阵营一
        /// byte 2 表示阵营二
        /// </summary>
        public event Action<BattleResultInfo[]> OnBattleEnd
        {
            add { onBattleEnd += value; }
            remove{ onBattleEnd -= value; }
        }
        public Action<BattleResultInfo[]> onBattleEnd;

        //一次性传输战斗信息的集合
        public List<BattleTransferDTO> BattleTransferDTOList { get; set; } = new List<BattleTransferDTO>();
        //记录当前未结束行为传输信息的栈
        //行为开始时压入栈，行为结束时弹出栈,顶部的即为当前占用的记录对象
        public Stack<BattleTransferDTO> NowTransferStack { get; set; } = new Stack<BattleTransferDTO>();
        public BattleTransferDTO SpawnBattleTransfer()
        {
            BattleTransferDTO battleTransferDTO = new BattleTransferDTO();
            NowTransferStack.Push(battleTransferDTO);
            BattleTransferDTOList.Add(battleTransferDTO);
            return battleTransferDTO;
        }
        public BattleTransferDTO ReleaseBattleTransfer()
        {
            BattleTransferDTO result= NowTransferStack.Pop();
            return result;
        }
        public BattleTransferDTO GetBattleTransfer()
        {
            if (NowTransferStack.Count > 0)
                return NowTransferStack.Peek();
            else
                return null;
        }
        /// <summary>
        /// 初始化房间
        /// </summary>
        public void InitRoom(int roomId, BattleInitDTO battleInitDTO)
        {
            RoomId = roomId;
            BattleController =ReferencePool.Accquire<BattleController>();
            BattleController.InitController(this);
            //添加玩家
            BattlePlayerEntity battlePlayerEntity = GameEntry.BattleCharacterManager.AddPlayerCharacter(roomId,battleInitDTO.playerUnits[0].UniqueId, BattleFactionType.FactionOne);
            if (battlePlayerEntity != null)
            {
                BattleController.AddCharacterEntity(battlePlayerEntity);
                AllPlayerID.Add(battlePlayerEntity.UniqueID);
                PlayerSendMsgEvent += battlePlayerEntity.SendMessage;
                battleCharacterEntityDict[battlePlayerEntity.UniqueID] = battlePlayerEntity;
            }
            //添加宠物
            BattlePetEntity battlePetEntity = GameEntry.BattleCharacterManager.AddPetCharacter(roomId,battleInitDTO.playerUnits[0].UniqueId, BattleFactionType.FactionOne);
            if (battlePetEntity != null)
            {
                BattleController.AddCharacterEntity(battlePetEntity);
                battleCharacterEntityDict[battlePetEntity.UniqueID] = battlePetEntity;
            }
            //添加ai
            BattleAIEntity battleAIEntity;
            for (int i = 0; i < battleInitDTO.enemyUnits.Count; i++)
            {
                battleAIEntity = GameEntry.BattleCharacterManager.AddAICharacter(roomId,battleInitDTO.enemyUnits[i].GlobalId, BattleFactionType.FactionTwo);
                if (battleAIEntity != null)
                {
                    BattleController.AddCharacterEntity(battleAIEntity);
                    battleCharacterEntityDict[battleAIEntity.UniqueID] = battleAIEntity;
                }
            }

            SendInitDataS2C();
            StartPrepareWait();
        }
        /// <summary>
        /// 指定角色准备完成
        /// </summary>
        public void CharacterPrepare(int roleID)
        {
            HasGetCmdRoleID.Add(roleID);

            //判断是否房间所有角色准备完成
            if(HasGetCmdRoleID.Count==AllPlayerID.Count)//准备完成
            {
                Utility.Debug.LogError("所有角色准备完成");
                //移除准备计时事件
                GameEntry.BattleRoomManager.TimeAction -= PrepareWait;
                //开始收集指令倒计时
                SendBattleMessagePrepareDataS2C();
                StartGetBattleCmdWait();
            }
        }
        public void GetCharacterCmd(int roleID, BattleCmd battleCmd, BattleTransferDTO battleTransferDTO)
        {
            HasGetCmdRoleID.Add(roleID);
            battleCharacterEntityDict[roleID].SetBattleAction(battleCmd, battleTransferDTO);
            //判断是否房间所有角色都接受指令
            if (HasGetCmdRoleID.Count == AllPlayerID.Count)//准备完成
            {
                Utility.Debug.LogError("所有角色指令接收完成");
                //移除战斗指令计时事件
                GameEntry.BattleRoomManager.TimeAction -= GetBattleCmdWait;
                //todo战斗计算
                BattleController.StartBattle();
            }
        }
        /// <summary>
        /// 指定角色表演完成
        /// </summary>
        /// <param name="roleID"></param>
        public void CharacterPerformOver(int roleID)
        {
            HasGetCmdRoleID.Add(roleID);
            if(HasGetCmdRoleID.Count == AllPlayerID.Count)//所有玩家表演完成
            {
                Utility.Debug.LogError("所有玩家表演完成");
                GameEntry.BattleRoomManager.TimeAction -= PerformWait;
                //发送下一回合开始或战斗结束的消息
                if (BattleController.BattleIsEnd())
                {
                    //onBattleEnd?.Invoke()
                    SendBattleEndMsgS2C();
                }
                else
                    SendNewRoundStartMsgS2C();

            }
            else//有人没表演完成
            {
                if (HasGetCmdRoleID.Count == 1)//收到第一个玩家的表演完成消息开始计时
                {
                    StartPerfomWait();
                }
            }
        }


        //开始准备倒计时等待 
        void StartPrepareWait()
        {
            //清理已准备玩家ID容器
            HasGetCmdRoleID.Clear();
            //开始战斗等待计时
            countDownTargetTime = DateTime.Now.AddSeconds(GameEntry.BattleRoomManager.PrepareWaitTime);
            GameEntry.BattleRoomManager.TimeAction += PrepareWait;
        }
        DateTime countDownTargetTime;   
        void PrepareWait()
        {
            if (DateTime.Compare(DateTime.Now,countDownTargetTime)>0)//倒计时到了
            {
                Utility.Debug.LogInfo("准备倒计时结束");
                GameEntry.BattleRoomManager.TimeAction -= PrepareWait;
                //开始收集指令倒计时
                SendBattleMessagePrepareDataS2C();
            }
        }
        /// <summary>
        /// 开始收集战斗指令倒计时等待
        /// </summary>
        void StartGetBattleCmdWait()
        {
            //清理已收集到指令的玩家ID容器
            HasGetCmdRoleID.Clear();
            //开始战斗结算等待计时
            countDownTargetTime = DateTime.Now.AddSeconds(GameEntry.BattleRoomManager.RoundTIme);
            GameEntry.BattleRoomManager.TimeAction += GetBattleCmdWait;
        }
        void GetBattleCmdWait()
        {
            if (DateTime.Compare(DateTime.Now, countDownTargetTime) > 0)//倒计时到了
            {
                Utility.Debug.LogInfo("收集战斗指令倒计时结束");
                GameEntry.BattleRoomManager.TimeAction -= GetBattleCmdWait;
                //todo战斗计算
                BattleController.StartBattle();
            }
        }
        /// <summary>
        /// 开始玩家表演等待
        /// </summary>
        void StartPerfomWait()
        {
            countDownTargetTime = DateTime.Now.AddSeconds(GameEntry.BattleRoomManager.PerformWaitTime);
            GameEntry.BattleRoomManager.TimeAction += PerformWait;
        }
        void PerformWait()
        {
            if (DateTime.Compare(DateTime.Now, countDownTargetTime) > 0)//倒计时到了
            {
                Utility.Debug.LogInfo("等待玩家表演倒计时结束");
                GameEntry.BattleRoomManager.TimeAction -= PerformWait;
                if (BattleController.BattleIsEnd())
                    SendBattleEndMsgS2C();
                else
                    SendNewRoundStartMsgS2C();
            }
        }

        public void Release()
        {
            RoomId = -1;
        }
    }
}
