using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual byte RoleFaction { get; set; }
        public virtual bool RoleGender { get; set; }
        public virtual int RoleTalent { get; set; }
        public virtual string RoleRoot { get; set; }
        public virtual string RoleName { get; set; }
        public virtual int RoleLevel { get; set; }

        #region team  
        public virtual TeamInstructions teamInstructions { get; set; }
        public virtual TeamDTO teamDTO { get; set; } 
        #endregion
        public virtual BattleInitDTO BattleInitDTO { get; set; }
        /// <summary>
        /// 战斗指令
        /// </summary>
        public virtual BattleCmd SendBattleCmd { get; set; }
        public override void Release()
        {
            RoleID = -1;
            RoleFaction = 0;
            RoleTalent = 0;
            RoleRoot = null;
            RoleName = null;
            RoleLevel = 0;
            teamInstructions = 0;
            teamDTO = null;
        }
        public override string ToString()
        {
            string str = "RoleID : "+ RoleID+ " ; RoleGender : "+ RoleGender+ " ; RoleName : "+ RoleName;
            return str;
        }
    }
    /// <summary>
    /// 背包指令
    /// </summary>
    public enum InventoryInstructions
    {
        GetData = 0,
        AddData = 1,
        UpdateData = 2,
        RemoveData = 3,
        SortingData = 4
    }


    /// <summary>
    /// 组队Cmd
    /// </summary>
    public enum TeamInstructions
    {
        Init = 1,
        /// <summary>
        /// 创建队伍
        /// </summary>
        CreateTeam = 2,
        /// <summary>
        /// 加入队伍
        /// </summary>
        JoinTeam = 3,
        /// <summary>
        /// 同意入队
        /// </summary>
        ApplyTeam = 4,
        /// <summary>
        /// 拒绝入队
        /// </summary>
        RefusedTeam = 5,
        /// <summary>
        /// 解散队伍
        /// </summary>
        DissolveTeam = 6,
        /// <summary>
        ///队伍踢人
        /// </summary>
        KickTeam = 7,
        /// <summary>
        /// 离开队伍
        /// </summary>
        LevelTeam = 8,
        /// <summary>
        /// 退出队伍
        /// </summary>
        ExitTeam = 9,
        /// <summary>
        /// 委任指挥
        /// </summary>
        CommandTeam = 10,
        /// <summary>
        /// 调整站位
        /// </summary>
        PositionTeam = 11,
        /// <summary>
        /// 加好友
        /// </summary>
        FrindTeam = 12,
        /// <summary>
        /// 自动匹配
        /// </summary>
        MatchTeam = 13,
        /// <summary>
        /// 转让队长
        /// </summary>
        TransferTeam = 14,
        /// <summary>
        /// 发送消息
        /// </summary>
        MessageTeam = 15
    }

    #region 战斗Cmd
    /// <summary>
    /// 战斗指令列表
    /// </summary>
    public enum BattleCmd
    {
        /// <summary>
        /// 初始化战斗
        /// </summary>
        Init,
        /// <summary>
        /// 准备战斗指令
        /// </summary>
        Prepare,
        /// <summary>
        /// 使用道具指令
        /// </summary>
        PropsInstruction,
        /// <summary>
        /// 使用技能指令
        /// </summary>
        SkillInstruction,
        /// <summary>
        /// 逃跑指令
        /// </summary>
        RunAwayInstruction,
        /// <summary>
        /// 战斗表演完成 
        /// </summary>
        PerformBattleComplete,
        /// <summary>
        /// 法宝指令
        /// </summary>
        MagicWeapon,
        /// <summary>
        /// 捕捉指令
        /// </summary>
        CatchPet,
        /// <summary>
        /// 召唤指令
        /// </summary>
        SummonPet,
        /// <summary>
        /// 阵法指令
        /// </summary>
        Tactical,
        /// <summary>
        /// 防御指令
        /// </summary>
        Defend
    }
    #endregion
}   
