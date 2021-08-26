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
    public class BattleCharacterEntity : IReference, IComparable<BattleCharacterEntity>
    {
        /// <summary>
        /// 房间ID
        /// </summary>
        public int RoomID { get; protected set; }
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int UniqueID { get; protected set; }
        /// <summary>
        /// 公共ID
        /// </summary>
        public int GlobalID { get; protected set; }
        public int MasterID { get; protected set; }
        public string Name { get; protected set; }
        /// <summary>
        /// 角色是否死亡
        /// </summary>
        public bool HasDie { get { return CharacterBattleData.Hp <= 0 ? true : false; } }
        /// <summary>
        /// 属性数据
        /// </summary>
        public CharacterBattleData CharacterBattleData { get; protected set; }
        public BattleFactionType BattleFactionType { get; protected set; }

        public List<BattleCharacterEntity> FriendCharacterEntities { get; protected set; }
        public List<BattleCharacterEntity> EnemyCharacterEntities { get; protected set; }

        public BattleCmd BattleCmd { get;  set; }
        public int ActionID { get;  set; }
        public List<int> TargetIDList { get;  set; }

        public BattleSkillController BattleSkillController { get; protected set; }
        public BattleBuffController BattleBuffController { get; protected set; }

        /// <summary>
        /// 造成的伤害数据
        /// </summary>
        public BattleDamageData TakeBattleDamageData { get; set; }
        /// <summary>
        /// 受到的伤害数据
        /// </summary>
        public BattleDamageData ReceiveBattleDamageData { get; set; }

        /// <summary>
        /// 设置该角色的敌方和友方
        /// </summary>
        public void SetFriendAndEnemy(List<BattleCharacterEntity> factionOneCharacterEntites, List<BattleCharacterEntity> factionTwoCharacterEntites)
        {
            switch (BattleFactionType)
            {
                case BattleFactionType.FactionOne:
                    FriendCharacterEntities = factionOneCharacterEntites;
                    EnemyCharacterEntities = factionTwoCharacterEntites;
                    break;
                case BattleFactionType.FactionTwo:
                    FriendCharacterEntities = factionTwoCharacterEntites;
                    EnemyCharacterEntities = factionOneCharacterEntites;
                    break;
            }
        }

        /// <summary>
        /// 转换成发送用的人物初始化数据
        /// </summary>
        /// <returns></returns>
        public virtual CharacterBattleDataDTO ToBattleDataBase()
        {
            return default;
        }
        /// <summary>
        /// 设置角色该回合战斗行为
        /// </summary>
        public virtual void SetBattleAction(BattleCmd battleCmd, BattleTransferDTO battleTransferDTO)
        {
        }
        /// <summary>
        /// 出手前判断并分配战斗行为
        /// </summary>
        public virtual void AllocationBattleAction()
        {
            //todo buff判断，强制控制角色行动
        }
        /// <summary>
        /// 获取技能目标ID列表
        /// </summary>
        /// <param name="skillID">技能id</param>
        /// <param name="targetList">初始指定的目标列表</param>
        /// <returns></returns>
        public virtual List<int> GetTargetIdList(int skillID, bool isAutoChangeTarget, List<int> targetList = null)
        {
            List<int> resultList = new List<int>();
            GameEntry.DataManager.TryGetValue<Dictionary<int, BattleSkillData>>(out var battleskillDataDict);
            BattleSkillData battleSkillData = battleskillDataDict[skillID];
            BattleFactionType battleFactionType = default;
            switch (battleSkillData.battleSkillFactionType)
            {
                case BattleSkillFactionType.Enemy:
                    battleFactionType = (BattleFactionType == BattleFactionType.FactionOne) ? BattleFactionType.FactionTwo : BattleFactionType.FactionOne;
                    break;
                case BattleSkillFactionType.TeamMate:
                    battleFactionType = (BattleFactionType == BattleFactionType.FactionOne) ? BattleFactionType.FactionOne : BattleFactionType.FactionTwo;
                    break;
            }
            resultList = GameEntry.BattleRoomManager.GetBattleRoomEntity(RoomID).BattleController.RandomGetTarget(battleSkillData.TargetNumber, battleFactionType, true, isAutoChangeTarget, targetList);
            return resultList;
        }
        /// <summary>
        /// 角色进行行动的方法
        /// </summary>
        public void Action()
        {
            List<BattleTransferDTO> battleTransferDTOList = GameEntry.BattleRoomManager.GetBattleRoomEntity(RoomID).BattleTransferDTOList;
            if (HasDie)
                return;
            switch (BattleCmd)
            {
                case BattleCmd.PropsInstruction:
                    break;
                case BattleCmd.SkillInstruction:
                     BattleSkillController.UseSkill(ActionID, TargetIDList,true);
                    if (battleTransferDTOList.Count > 1)
                        battleTransferDTOList[battleTransferDTOList.Count - 1].FinishActionRoleID = UniqueID;
                    else if (battleTransferDTOList.Count == 1)
                        battleTransferDTOList[0].FinishActionRoleID = UniqueID;
                    return;
                case BattleCmd.MagicWeapon:
                    break;
                case BattleCmd.CatchPet:
                    break;
                case BattleCmd.SummonPet:
                    break;
                case BattleCmd.RunAwayInstruction:
                    break;
                case BattleCmd.Tactical:
                    break;
                case BattleCmd.Defend:
                    break;
            }
            return;
        }
        /// <summary>
        /// 受到技能等效果的结算
        /// </summary>
        public void OnActionEffect(BattleDamageData battleDamageData)
        {
            if (battleDamageData == null)
                return;
            switch (battleDamageData.battleSkillActionType)
            {
                case BattleSkillActionType.Damage:
                    //todo受击时的触发判断
                    CharacterBattleData.ChangeProperty(battleDamageData);
                    break;
                case BattleSkillActionType.Heal:
                    CharacterBattleData.ChangeProperty(battleDamageData);
                    break;
                case BattleSkillActionType.Resurrection:
                    break;
                case BattleSkillActionType.Summon:
                    break;
            }
        }

        protected void Init()
        {
            FriendCharacterEntities = new List<BattleCharacterEntity>();
            EnemyCharacterEntities = new List<BattleCharacterEntity>();
            BattleSkillController = new BattleSkillController(this);
            BattleBuffController = new BattleBuffController(this);
            TargetIDList = new List<int>();

        }


        public virtual void Release()
        {

        }

        public int CompareTo(BattleCharacterEntity other)
        {
            if (CharacterBattleData.AttackSpeed > other.CharacterBattleData.AttackSpeed)
                return -1;
            else if (CharacterBattleData.AttackSpeed == other.CharacterBattleData.AttackSpeed)
                return 0;
            else
                return 1;
        }
    }
}
