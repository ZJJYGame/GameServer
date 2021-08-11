using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 用于历练展示别的玩家的数值信息DTO
    /// 角色ID
    /// 角色宗门
    /// 角色性别
    /// 角色名称
    /// 角色血量
    /// 角色真元
    /// 角色精血
    /// 角色物理攻击
    /// 角色物理防御
    /// 角色法术攻击
    /// 角色法术防御
    /// 角色攻击速度
    /// 角色神魂
    /// 角色神魂攻击
    /// 角色神魂防御
    /// 角色暴击
    /// 角色暴击防御
    /// 角色隐匿值
    /// 人物业障
    /// 人物活力
    /// 人物煞气
    /// </summary>
    [Serializable]
    public class AdventurePlayerDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int  RoleFaction { get; set; }
        public virtual bool RoleGender { get; set; }
        public virtual string RoleName { get; set; }
        public virtual int RoleLevel { get; set; }
        public virtual int RoleHP { get; set; }
        public virtual int RoleMP { get; set; }
        public virtual int RoleJingXue { get; set; }
        public virtual int RoleAttackDamage { get; set; }
        public virtual int RoleResistanceDamage { get; set; }
        public virtual int RoleAttackPower { get; set; }
        public virtual int RoleResistancePower { get; set; }
        public virtual int RoleSpeedAttack { get; set; }
        public virtual int  RoleShenhun { get; set; }
        public virtual int RoleShenHunDamage { get; set; }
        public virtual int RoleShenHunResistance { get; set; }
        public virtual int RoleCrit { get; set; }
        public virtual int  RoleCritResistance { get; set; }
        public virtual int RoleDormant { get; set; }
        public virtual int RoleVileSpawn { get; set; }
        public virtual int RoleVitality { get; set; }
        public virtual int RoleKillingIntent { get; set; }
        public virtual string RoleAllianceName { get; set; }
        public virtual int RoleMoveSpeed { get; set; }
        public override void Release()
        {
            RoleFaction = -1;
            RoleName = null;
            RoleLevel = -1;
            RoleHP = -1;
            RoleMP = -1;
            RoleJingXue = -1;
            RoleAttackDamage = -1;
            RoleResistanceDamage = -1;
            RoleAttackPower = -1;
            RoleSpeedAttack = -1;
            RoleShenhun = -1;
            RoleShenHunDamage = -1;
            RoleShenHunResistance = -1;
            RoleCrit = -1;
            RoleCritResistance = -1;
            RoleDormant = -1;
            RoleVileSpawn = -1;
            RoleVitality = -1;
            RoleKillingIntent = -1;
            RoleAllianceName = null;
            RoleMoveSpeed = -1;
        }
    }
}
