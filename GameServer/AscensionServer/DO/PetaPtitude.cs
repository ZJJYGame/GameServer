using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class PetAptitude : DataObject
    {
        public virtual int PetID { get; set; }
        public virtual int HPAptitude { get; set; }
        public virtual int MPAptitude { get; set; }
        public virtual int SoulAptitude { get; set; }
        public virtual int AttackspeedAptitude { get; set; }
        public virtual int AttackphysicalAptitude { get; set; }
        public virtual int DefendphysicalAptitude { get; set; }
        public virtual int AttackpowerAptitude { get; set; }
        public virtual int DefendpowerAptitude { get; set; }
        public virtual int AttacksoulAptitude { get; set; }
        public virtual int DefendsoulAptitude { get; set; }
        public virtual float Petaptitudecol { get; set; }
        public virtual string PetAptitudeDrug { get; set; }
        public PetAptitude()
        {
            HPAptitude = 0;
            MPAptitude = 0;
            SoulAptitude = 0;
            AttackspeedAptitude = 0;
            AttackphysicalAptitude = 0;
            DefendphysicalAptitude = 0;
            AttackpowerAptitude = 0;
            DefendpowerAptitude = 0;
            AttacksoulAptitude = 0;
            DefendsoulAptitude = 0;
            Petaptitudecol = 0;
            PetAptitudeDrug ="{}";
        }

        public override void Release()
        {
            PetID = -1;
            HPAptitude = 0;
            MPAptitude = 0;
            SoulAptitude = 0;
            AttackspeedAptitude = 0;
            AttackphysicalAptitude = 0;
            DefendphysicalAptitude = 0;
            AttackpowerAptitude = 0;
            DefendpowerAptitude = 0;
            AttacksoulAptitude = 0;
            DefendsoulAptitude = 0;
            Petaptitudecol = 0;
            PetAptitudeDrug = "{}";
        }
    }
}


