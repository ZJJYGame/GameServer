using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static AscensionProtocol.DTO.BattleTransferDTO;
/*
* 战斗的映射
* Since : 2020 - 09 -22
* Author : xianren*/
namespace AscensionProtocol.DTO
{
    [Serializable]
    public class BattleInitDTO : DataTransferObject
    {
        //public virtual
        /*
         * 
         * 
         * 
         * 11. 攻击对象名字
         * 10.攻击伤害系数
         * 9.捕捉的
         * 8.逃跑的
         * 7.功法、秘术的DTO
         * 6.宠物的DTO
         * 5.法宝的DTO
         * 4.道具的DTO
         * 3.技能的DTO
         * 2.怪物的DTO
         * 1.队伍的DTO*/

        /// <summary>
        /// 当前房间ID
        /// </summary>
        public virtual int RoomId { get; set; }
        /// <summary>
        /// 倒计时秒
        /// </summary>
        public virtual int countDownSec { get; set; }


        /// <summary>
        /// 当前房间内战斗的回合数
        /// </summary>
        public virtual int roundCount { get; set; }
        /// <summary>
        /// 最大的回合数
        /// </summary>
        public virtual int maxRoundCount { get; set; }
        /// <summary>
        /// 所有参战玩家的列表
        /// </summary>
        public virtual List<CharacterBattleDataDTO> playerUnits { get; set; }
        /// <summary>
        /// 所有参战宠物的列表
        /// </summary>
        public virtual List<CharacterBattleDataDTO> petUnits { get; set; }
        /// <summary>
        /// 所有参战敌人的列表
        /// </summary>
        public virtual List<CharacterBattleDataDTO> enemyUnits { get; set; }
        /// <summary>
        /// 所有参战宠物的列表
        /// </summary>
        public virtual List<CharacterBattleDataDTO> enemyPetUnits { get; set; }
        ///// <summary>
        ///// buffer的列表
        ///// </summary>
        //public virtual List<BufferBattleDataDTO> bufferUnits { get; set; }

        public override void Release()
        {
            throw new NotImplementedException();
        }
    }



  

    [Serializable]
    public class CharacterBattleDataDTO
    {
        public virtual int UniqueId { get; set; }
        public virtual int GlobalId { get; set; }
        public virtual int MasterId { get; set; }
        public string ModelPath { get; set; }
        public string CharacterName { get; set; }
        public int MaxHealth { get; set; }//最大血量
        public int Health { get; set; }//血量
        public int MaxZhenYuan { get; set; }//最大真元
        public int ZhenYuan { get; set; }//真元
        public int MaxShenHun { get; set; }//最大神魂
        public int ShenHun { get; set; }//神魂
        public int MaxJingXue { get; set; }//最大精血
        public int JingXue { get; set; }//精血
    }


    #region  战斗传输数据DTO


    /// <summary>
    /// 战斗传输数据DTO   列表 ， 出手速度  是按照玩家speed 排列的
    /// </summary>
    public class BattleTransferDTO
    {
        /// <summary>
        /// 控制每回合的时间
        /// </summary>
        //public virtual TimerManager timer { get; set; }
        public BattleTransferDTO petBattleTransferDTO { get; set; }
        /// <summary>
        /// 是否结束
        /// </summary>
        public bool isFinish { get; set; }

        /// <summary>
        /// 结束行动的角色ID
        /// </summary>
        public int FinishActionRoleID;

        /// <summary>
        /// 角色id
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 客户端目标id
        /// </summary>
        public int ClientCmdId { get; set; }
        /// <summary>
        /// 目标行为信息 列表：
        /// </summary>
        public virtual List<TargetInfoDTO> TargetInfos { get; set; }
        /// <summary>
        /// 添加的buff列表,用于记录技能添加的buff
        /// </summary>
        public virtual List<AddBuffDTO> AddBuffDTOList { get; set; }
        /// <summary>
        /// buff触发事件的信息
        /// </summary>
        public virtual List<BattleBuffEventTriggerDTO> BattleBuffEventTriggerDTOList { get; set; }
        /// <summary>
        /// 每回合战斗指令
        /// </summary>
        public virtual BattleCmd BattleCmd { get; set; }
    }
    #endregion

    /// <summary>
    /// 一段技能对一个角色的行为信息
    /// </summary>
    public class TargetInfoDTO
    {
        /// <summary>
        /// 全局id
        /// </summary>
        //public virtual int GlobalId { get; set; }
        /// <summary>
        /// 目标id
        /// </summary>
        public virtual int TargetID { get; set; }
        /// <summary>
        /// 目标血量伤害
        /// </summary>
        public virtual int TargetHPDamage { set; get; }

        /// <summary>
        /// 目标蓝量伤害
        /// </summary>
        public virtual int TargetMPDamage { set; get; }
        /// <summary>
        /// 目标神魂伤害
        /// </summary>
        public virtual int TargetShenHunDamage { set; get; }
        /// <summary>
        /// 目标护盾值
        /// </summary>
        public virtual int TargetShieldVaule { get; set; }
        /// <summary>
        ///添加 目标Buff,主要应用于buff事件添加的buff；
        /// </summary>
        public virtual List<AddBuffDTO> AddBuffDTOList { get; set; }
        /// <summary>
        ///移除 目标Buff
        /// </summary>
        public virtual List<int> RemoveTargetBuff { get; set; }

        public virtual List<BattleBuffEventTriggerDTO> battleBuffDTOs { get; set; }

    }
    
    /// <summary>
    /// 行动添加buff的信息
    /// </summary>
    [Serializable]
    public class AddBuffDTO
    {
        public virtual int TargetId { get; set; }
        public virtual int BuffId { get; set; }
        public virtual int Round { get; set; }
        public AddBuffDTO() { }
        public AddBuffDTO(int targetId,int buffId,int round) {
            TargetId = targetId;
            BuffId = buffId;
            Round = round;
        }
    }

    /*事件对应的参数：
        1.添加buff=>BuffDTOList:添加的buff；
        2.喊别人承担伤害=>Num_1:承担的伤害数字
        3.buff属性变动=>Num_1:buff属性改变类型枚举（BuffEvent_PropertyChangeType）；Num_2:改变数值（百分比）
        4.改变行为指令=>Num_1:指令类型枚举（BattleCmd）；Num_2:指令id
        5.改变角色属性=>Num_1:改变属性类型枚举（BattleBuffEventType_RolePropertyChange）；Num_2:变动的数值
        6.改变行为目标
        7.伤害或治疗=>Num_1:伤害或治疗的目标属性（BuffEvent_DamageOrHeal_EffectTargetType）；Num_2:伤害或治疗的数值
        8.伤害减免=>Num_1:减少伤害数值（待定）
        9.驱散buff=>BuffDTOList:移除的buff；
        10.免疫buff=>BuffDTOList：免疫的buff
        11.为他人承受伤害=>Num_1:承担的伤害数字
        12.护盾=>Num_1:护盾抵挡的数值
        13.使用指定技能=>Num_1:使用的技能ID
     */
    /// <summary>
    /// buff触发的事件
    /// </summary>
    [Serializable]
    public class BattleBuffEventTriggerDTO
    {
        //事件触发者的Id
        public int TriggerId { get; set; }
        public int TargetId { get; set; }
        //事件监听者ID
        public int EventListenerId { get; set; }
        public int BuffId { get; set; }
        //事件触发时机的枚举
        public byte TriggerTime { get; set; }
        //触发事件类型的枚举
        public byte TriggerEventType { get; set; }
        //具体客户端需要知道的参数，根据事件类型不同，参数代表的值不一样(详细见注释)
        public int Num_1 { get; set; }
        public int Num_2 { get; set; }
        //buff信息列表，目前用于buff事件所导致的buff添加和buff移除
        public List<AddBuffDTO> BuffDTOList { get; set; }

        public BattleBuffEventTriggerDTO()
        {
            BuffDTOList = new List<AddBuffDTO>();
        }
    }
}

