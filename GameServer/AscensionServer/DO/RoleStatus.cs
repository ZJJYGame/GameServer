/*
*Author : xianrenZhang
*Since :	2020-04-28
*Description :  角色属性 
*/
using System.Collections.Generic;
using System;
namespace AscensionServer.Model
{
    [Serializable]
    public class RoleStatus:DataObject
    {
        public RoleStatus()
        {
            FreeAttributes = 0;
            RoleHP = 0;
            RoleMaxHP = 0;
            RoleMP = 0;
            RoleMaxMP = 0;
            RoleSoul = 0;
            RoleMaxSoul = 0;
            BestBlood = 0;
            BestBloodMax = 0;
            AttackSpeed = 0;
            AttackPhysical = 0;
            DefendPhysical = 0;
            AttackPower = 0;
            DefendPower = 0;
            PhysicalCritProb = 0;
            MagicCritProb = 0;
            ReduceCritProb = 0;
            PhysicalCritDamage = 0;
            MagicCritDamage = 0;
            ReduceCritDamage = 0;
            MoveSpeed = 0;
            RolePopularity = 0;
            RoleMaxPopularity = 0;
            ValueHide = 0;
        }       /// <summary>
        /// 角色id
        /// 角色属性点
        /// 角色血量
        /// 角色最大血量
        /// 角色真元
        /// 角色最大真元
        /// 角色神魂
        /// 角色最大神魂
        /// 角色精血
        /// 攻击速度
        /// 物理攻击
        /// 物理防御
        /// 法术攻击
        /// 法术防御
        /// 物理暴击几率
        /// 法术暴击几率
        /// 暴免率
        /// 物理暴击伤害
        /// 法术暴击伤害
        /// 防暴伤害
        /// 移动速度
        /// 业果值
        /// 最大业果值(最大值为固定值及表中初始数值)
        /// 隐匿值
        /// </summary>
        public virtual int RoleID { get; set; }
        public virtual int FreeAttributes { get; set; }
        public virtual int RoleHP { get; set; }
        public virtual int RoleMaxHP { get; set; }
        public virtual int RoleMP { get; set; }
        public virtual int RoleMaxMP { get; set; }
        public virtual int RoleSoul { get; set; }
        public virtual int RoleMaxSoul { get; set; }
        public virtual int BestBlood { get; set; }
        public virtual int BestBloodMax { get; set; }
        public virtual int Vitality { get; set; }
        public virtual int MaxVitality { get; set; }
        public virtual int AttackSpeed { get; set; }
        public virtual int AttackPhysical { get; set; }
        public virtual int DefendPhysical { get; set; }
        public virtual int AttackPower { get; set; }
        public virtual int DefendPower { get; set; }
        public virtual int PhysicalCritProb { get; set; }
        public virtual int MagicCritProb { get; set; }
        public virtual int ReduceCritProb { get; set; }
        public virtual int PhysicalCritDamage { get; set; }
        public virtual int MagicCritDamage { get; set; }
        public virtual int ReduceCritDamage { get; set; }
        public virtual int MoveSpeed { get; set; }
        public virtual int RolePopularity { get; set; }
        public virtual int RoleMaxPopularity { get; set; }
        public virtual int ValueHide { get; set; }
        public virtual int GongfaLearnSpeed { get; set; }
        public virtual int MishuLearnSpeed { get; set; }
        public override void Release()
        {
            FreeAttributes = 0;
            RoleHP = 0;
            RoleMaxHP = 0;
            RoleMP = 0;
            RoleMaxMP = 0;
            RoleSoul = 0;
            RoleMaxSoul = 0;
            BestBlood = 0;
            BestBloodMax = 0;
            AttackSpeed = 0;
            AttackPhysical = 0;
            DefendPhysical = 0;
            AttackPower = 0;
            DefendPower = 0;
            PhysicalCritProb = 0;
            MagicCritProb = 0;
            ReduceCritProb = 0;
            PhysicalCritDamage = 0;
            MagicCritDamage = 0;
            ReduceCritDamage = 0;
            MoveSpeed = 0;
            RolePopularity = 0;
            RoleMaxPopularity = 0;
            ValueHide = 0;
            GongfaLearnSpeed = 0;
            MishuLearnSpeed = 0;
            Vitality = 0;
            MaxVitality = 0;
    }
    }
}


