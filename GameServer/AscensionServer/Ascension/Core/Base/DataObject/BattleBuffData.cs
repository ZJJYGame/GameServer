using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    public class BattleBuffData
    {
        public string name;
        public int id;
        public string describe;
        public int coldTime;
        //[Header("Buff覆盖类型")]
        public BuffCoverType buffCoverType;
        //[Header("Buff层级")]
        public int buffLayer;
        public int maxSuperpositionCount;
        //[Header("BUFF触发条件列表")]
        public List<BattleBuffTriggerCondition> battleBuffTriggerConditionList;
        //[Header("BUFF触发事件列表")]
        public List<BattleBuffEventData> battleBuffEventDataList;


    }

    //一、属性（包括护盾）类：相同BUFFID的效果覆盖，不同BUFFID的效果叠加
    //二、异常类：高等级覆盖低等级，同类型高等级BUFF存在时低等级无法添加
    public enum BuffCoverType : byte
    {
        Property = 0,
        Bleed = 1,
        Burn = 2,
        Poisoning = 3,
        Frozen = 4,
    }


    public enum BattleBuffTriggerTime : byte
    {
        BuffAdd = 0,
        RoundStart = 1,
        BeforeAllocation = 2,
        BeforeUseSkill = 3,
        BeforeAttack = 4,
        BehindAttack = 5,
        BehindUseSkill = 6,
        BeforeOnHit = 7,
        BehindOnHit = 8,
        RoleBeforeDie = 9,
        RoleAfterDie = 10,
        RoundEnd = 11,
        BuffRemove = 12,
    }

    #region BattleBuffTriggerCondition
    [Serializable]
    /// <summary>
    /// buff触发条件数据类
    /// 0使用指定技能 UseDesignatedSkill=>idList
    /// 1拥有指定技能 HaveDesignatedSkill=>idList
    /// 2不拥有指定技能 NotHaveDesignatedSkill=>idList
    /// 3技能目标数量限定 LimitSkillTargetNum=>targetCount
    /// 4指定类型伤害 DesignatedDamageType=>battleBuffCondition_DamageType
    /// 5伤害暴击 DamageCrit=>无数据
    /// 6指定属性限定 DesignatedPropertyLimit=>battleBuffCondition_SourcePropertyType,criticalValue,isUp,flag
    /// 7双方指定属性比较 BothDesignatedPropertyCompare=>battleBuffCondition_SourcePropertyType,criticalValue,isUp
    /// 8目标存在指定buff TargetHaveDesignatedBuff=>idList
    /// 9角色类型限定 CharacterTypeLimit=>battleBuffCondition_CharacterType
    /// </summary>
    public class BattleBuffTriggerCondition
    {
        public BattleBuffConditionType battleBuffConditionType;
        public BattleBuffCondition_TargetCharacterType battleBuffCondition_TargetCharacterType;

        public List<uint> idList;
        public int targetCount;
        public BattleBuffCondition_DamageType battleBuffCondition_DamageType;
        public BattleSkillDamageType battleSkillDamageType;
        public BattleBuffCondition_SourcePropertyType battleBuffCondition_SourcePropertyType;
        public int criticalValue;
        public bool isUp;
        public bool flag;
        public BattleBuffCondition_CharacterType battleBuffCondition_CharacterType;
    }

    public enum BattleBuffConditionType : byte
    {
        /// <summary>
        /// 无触发条件
        /// </summary>
        //// [Tooltip("无触发条件")]
        None,
        //// [Tooltip("使用指定技能")]
        /// <summary>
        /// 使用指定技能
        /// </summary>
        UseDesignatedSkill,
        //// [Tooltip("拥有指定技能")]
        /// <summary>
        /// 拥有指定技能
        /// </summary>
        HaveDesignatedSkill,
        //// [Tooltip("不拥有指定技能")]
        /// <summary>
        /// 使用指定技能
        /// </summary>
        NotHaveDesignatedSkill,
        //// [Tooltip("技能目标数量限定")]
        /// <summary>
        /// 技能目标数量限定
        /// </summary>
        LimitSkillTargetNum,
        //// [Tooltip("指定类型伤害")]
        /// <summary>
        /// 指定类型伤害
        /// </summary>
        DesignatedDamageType,
        //// [Tooltip("伤害暴击")]
        /// <summary>
        /// 伤害暴击
        /// </summary>
        DamageCrit,
        //// [Tooltip("指定属性限定")]
        /// <summary>
        /// 指定属性限定
        /// </summary>
        DesignatedPropertyLimit,
        //// [Tooltip("双方指定属性比较")]
        /// <summary>
        /// 双方指定属性比较
        /// </summary>
        BothDesignatedPropertyCompare,
        //// [Tooltip("目标存在指定buff")]
        /// <summary>
        /// 目标存在指定buff
        /// </summary>
        TargetHaveDesignatedBuff,
        //// [Tooltip("角色类型限定")]
        /// <summary>
        /// 角色类型限定
        /// </summary>
        CharacterTypeLimit,
        /// <summary>
        /// 近身或远程攻击
        /// </summary>
        //// [Tooltip("近身或远程攻击")]
        CloseOrRangeAttack,
        /// <summary>
        /// 目标不存在指定buff
        /// </summary>
        TargetNotHaveDesignatedBuff
    }

    /// <summary>
    /// buff触发条件目标类型限定
    /// </summary>
    public enum BattleBuffCondition_TargetCharacterType : byte
    {
        Self = 0,
        AttackTarget = 1,
        Master = 2,
    }
    /// <summary>
    /// 伤害类型条件
    /// </summary>
    public enum BattleBuffCondition_DamageType : byte
    {
        Physic,
        Magic,
        ShenHun,
        Reality,
        Metals,
        Wooden,
        Water,
        Fire,
        Soil,
        Thunder,
    }
    public enum BattleBuffCondition_SourcePropertyType : byte
    {
        Health,
        ZhenYuan,
        ShenHun,
        JingXue,
    }
    public enum BattleBuffCondition_CharacterType : byte
    {
        Player = 0,
        Pet = 1,
        Summon = 2,
    }
    #endregion

    #region BattleBuffEventData
    /// <summary>
    /// 0角色属性变动 RolePropertyChange=>
    /// 1buff属性变动 BuffPropertyChange=>
    /// 2禁用buff ForbiddenBuff=>
    /// 3角色状态改变 RoleStateChange=>
    /// 4使用指定技能 UseDesignateSkill=>
    /// 5伤害或治疗 DamageOrHeal=>
    /// 6护盾 Shield=>
    /// 7该次伤害减免 DamageReduce=>
    /// 8替他人承担伤害 TakeHurtForOther=>
    /// 9施加buff AddBuff=>
    /// 10驱散buff DispelBuff=>
    /// 11无法复活 Resurgence=>
    /// </summary>
    [Serializable]
    public class BattleBuffEventData
    {
        //[Header("事件附带的触发条件")]
        /// <summary>
        /// 每个事件可能有一个附加的触发条件
        /// </summary>
        public BattleBuffTriggerCondition eventTriggerCondition;
        //[header("9、takehurtforother(替他人承担伤害)；10、addbuff(施加buff)；11、dispelbuff(驱散buff)；12、resurgence(无法复活)")]
        //[header("5、usedesignateskill(使用指定技能)；6、damageorheal(伤害或治疗)；7、shield(护盾)；8、damagereduce(该次伤害减免)；")]
        //[header("1、rolepropertychange(角色属性变动)；2、buffpropertychange(buff属性变动)；3、forbiddenbuff(禁用buff)；4、rolestatechange(角色状态改变)；")]

        public int probability;
        public int maxTriggerCount;

        public BattleBuffTriggerTime battleBuffTriggerTime;

        public BattleBuffEventType battleBuffEventType;
        //[Header("事件类型:")]
        public BattleBuffEventType_RolePropertyChange battleBuffEventType_RolePropertyChange;
        public BuffEvent_RolePropertyChange_SourceDataType buffRolePropertyChange_SourceDataType;
        public BuffEvent_PropertyChangeType buffPropertyChangeType;
        public BuffEvent_RoleStateChangeType buffRoleStateChangeType;
        public BuffEvent_DamageOrHeal_ChangeDataType buffEvent_DamageOrHeal_ChangeDataType;
        public BuffEvent_DamageOrHeal_SourceDataType buffEvent_DamageOrHeal_SourceDataType;
        public BuffEvent_DamageOrHeal_EffectTargetType buffEvent_DamageOrHeal_EffectTargetType;
        public BattleSkillDamageType BattleSkillDamageType;
        public BuffEvent_Shield_SourceDataType buffEvent_Shield_SourceDataType;
        public BuffEvent_TakeDamageForOther_TargetType buffEvent_TakeDamageForOther_TargetType;
        public BuffEvent_CallOterShareDamage_TargetType buffEvent_CallOterShareDamage_TargetType;
        public BuffEvent_ChangeTarget_TargetType buffEvent_ChangeTarget_TargetType;
        public int fixedValue;
        public int percentValue;
        public List<int> idList;
        public bool flag;
        public bool flag_2;
        public bool flag_3;
        public int id;
        public BattleSkillAddBuffData battleSkillAddBuffData;

    }

    public enum BattleBuffEventType : byte
    {
        // [Tooltip("角色属性变动")]
        RolePropertyChange,
        // [Tooltip("buff属性变动")]
        BuffPropertyChange,
        // [Tooltip("禁用buff")]
        ForbiddenBuff,
        // [Tooltip("角色状态改变")]
        RoleStateChange,
        // [Tooltip("使用指定技能")]
        UseDesignateSkill,
        // [Tooltip("伤害或治疗")]
        DamageOrHeal,
        // [Tooltip("护盾")]
        Shield,
        // [Tooltip("该次伤害减免")]
        DamageReduce,
        // [Tooltip("替他人承担伤害")]
        ShareDamageForOther,
        // [Tooltip("施加buff")]
        AddBuff,
        // [Tooltip("驱散buff")]
        DispelBuff,
        // [Tooltip("无法复活")]
        NotResurgence,
        //[Tooltip("免疫buff")]
        ImmuneBuff,
        //[Tooltip("改变行为指令")]
        ChangeCmd,
        //[Tooltip("改变行为目标")]
        ChangeTagrget,
        CallOtherShareDamage
    }
    public enum BattleBuffEventType_RolePropertyChange : byte
    {
        Health,
        ZhenYuan,
        ShenHun,
        JingXue,
        PhysicAttack,
        PhysicDefend,
        MagicAttack,
        MagicDefend,
        AttackSpeed,
        HitRate,
        BasicDodgeRate,
        PhysicDodgeRate,
        MagicDodgeRate,
        PhysicCritRate,
        MagicCritRate,
        ReduceCritRate,
        PhysicCritDamage,
        MagicCritDamage,
        ReduceCritDamage,
        TakeDamage,
        ReceiveDamage,
        IgnoreDefend,
        DamageNumFluctuations,
        HealEffect,
    }
    /// <summary>
    /// buff事件：人物属性变动对应的数据来源类型
    /// </summary>
    public enum BuffEvent_RolePropertyChange_SourceDataType : byte
    {
        MaxHealth,
        MaxZhenYuan,
        MaxShenHun,
        PhysicAttack,
        MagicAttack,
        PhysicDefend,
        MagicDefend,
        TakeDamage,
        SkillDamage,
        AttackSpeed,
    }
    /// <summary>
    /// buff事件：buff属性改变类型
    /// </summary>
    public enum BuffEvent_PropertyChangeType : byte
    {
        DamageAddition,
        DamageDeduction,
        IgnoreDefend,
        BasicDodgeRate

    }
    /// <summary>
    /// buff事件：修改人物状态类型
    /// </summary>
    public enum BuffEvent_RoleStateChangeType : byte
    {
        Dizziness,
        Frozen,
        Chaos,
        LostHeart,
        Hide,
    }
    /// <summary>
    /// buff事件：伤害或治疗改变的数据类型
    /// </summary>
    public enum BuffEvent_DamageOrHeal_ChangeDataType : byte
    {
        Health,
        ZhenYuan,
        ShenHun,
    }
    /// <summary>
    /// buff事件：伤害或治疗数值的数据来源类型
    /// </summary>
    public enum BuffEvent_DamageOrHeal_SourceDataType : byte
    {
        MaxHealth,
        MaxZhenYuan,
        MaxShenHun,
        TakeDamageNum,
        ReceiveDamageNum
    }
    public enum BuffEvent_DamageOrHeal_EffectTargetType : byte
    {
        Health,
        ShenHun,
        ZhenYuan
    }
    /// <summary>
    /// buff事件：护盾的数据来源类型
    /// </summary>
    public enum BuffEvent_Shield_SourceDataType : byte
    {
        MaxHealth,
        MaxZhenYuan,
        MaxShenHun,
        TakeDamageNum,
        ReceiveDamageNum,
        PhysicalAttack
    }
    public enum BuffEvent_TakeDamageForOther_TargetType : byte
    {
        Master,
        AnyTeammate,
    }
    public enum BuffEvent_CallOterShareDamage_TargetType : byte
    {
        MaxHpCharacter,
        BuffOrgin
    }
    public enum BuffEvent_ChangeTarget_TargetType : byte
    {
        BuffOrginer,
        RnadomFromAll
    }
    #endregion
}


