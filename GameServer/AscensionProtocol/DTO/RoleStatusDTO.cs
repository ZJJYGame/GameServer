using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleStatusDTO : DataTransferObject
    {
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
        public virtual StatusChangeType  StatusChange { get; set; }
        public override bool Equals(object obj)
        {
            RoleStatusDTO other = obj as RoleStatusDTO;
            if (other == null) return false;
            if (this.RoleID == other.RoleID && this.RoleHP == other.RoleHP && this.RoleMP == other.RoleMP
                && this.BestBlood == other.BestBlood && this.BestBloodMax == other.BestBloodMax &&
                this.AttackSpeed == other.AttackSpeed && this.AttackPhysical == other.AttackPhysical &&
                this.DefendPhysical == other.DefendPhysical && this.AttackPower == other.AttackPower &&
                this.DefendPower == other.DefendPower && this.PhysicalCritProb == other.PhysicalCritProb &&
                this.MagicCritProb == other.MagicCritProb && this.ReduceCritProb == other.ReduceCritProb && this.PhysicalCritDamage == other.PhysicalCritDamage && this.MagicCritDamage == other.MagicCritDamage && this.ReduceCritDamage == other.ReduceCritDamage && this.MoveSpeed == other.MoveSpeed && this.RolePopularity == other.RolePopularity && this.RoleMaxPopularity == other.RoleMaxPopularity && this.ValueHide == other.ValueHide)
                return true;
            else return false;
        }
        public override string ToString()
        {
            string str = "";
            str += "roleid:" + RoleID + ">>roleHp" + RoleHP + ">>roleMP" + RoleMP + ">>roleShenhun" + RoleSoul + ">>roleJingxue" + BestBlood;
            return str;
        }
        public override void Release()
        {
            RoleID = -1;
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
            Vitality = 0;
            MaxVitality = 0;
        }

        public enum StatusChangeType
        {
            StatusGet=1,
            /// <summary>
            /// 属性全回复
            /// </summary>
            StatusReplyAll=2
        }
    }
}