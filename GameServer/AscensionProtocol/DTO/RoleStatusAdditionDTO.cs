using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleStatusAdditionDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int FreeAttributes { get; set; }
        public virtual int RoleHP { get; set; }
        public virtual int RoleMaxHP { get; set; }
        public virtual int RoleMP { get; set; }
        public virtual int RoleMaxMP { get; set; }
        public virtual int RoleSoul { get; set; }
        public virtual int RoleMaxSoul { get; set; }
        public virtual short BestBlood { get; set; }
        public virtual short BestBloodMax { get; set; }
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

        /// <summary>
        /// 百分比加成
        /// </summary>
        public Dictionary<int,int> Percentage { get; set; }
        /// <summary>
        /// 固定值加成
        /// </summary>
        public Dictionary<int, int> Fixed { get; set; }

        public RoleStatusAdditionDTO()
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
            Percentage = new Dictionary<int, int>();
            Fixed = new Dictionary<int, int>();
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
    }
}
