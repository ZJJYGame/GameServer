using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class PetStatusDTO: DataTransferObject
    {
        public virtual int PetID { get; set; }
        public virtual int PetMaxHP { get; set; }
        public virtual int PetHP { get; set; }
        public virtual int PetMaxMP { get; set; }
        public virtual int PetMP { get; set; }
        public virtual int PetMaxShenhun { get; set; }
        public virtual int PetShenhun { get; set; }
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
        public virtual int ExpLevelUp { get; set; }
        public override void Release()
        {
            PetID = 0;
            PetMaxHP = 0;
            PetHP = 0;
            PetMaxMP = 0;
            PetMP = 0;
            PetMaxShenhun = 0;
            PetShenhun = 0;
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
            ExpLevelUp = 0;
        }
    }
}
