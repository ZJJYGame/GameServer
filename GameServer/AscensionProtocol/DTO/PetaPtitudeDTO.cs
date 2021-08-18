using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class PetAptitudeDTO : DataTransferObject
    {
        public virtual int PetID { get; set; }
        public virtual int HPAptitude { get; set; }
        public virtual int SoulAptitude { get; set; }
        public virtual int AttackspeedAptitude { get; set; }
        public virtual int AttackphysicalAptitude { get; set; }
        public virtual int DefendphysicalAptitude { get; set; }
        public virtual int AttackpowerAptitude { get; set; }
        public virtual int DefendpowerAptitude { get; set; }
        public virtual float Petaptitudecol { get; set; }
        public virtual Dictionary<int, int> PetAptitudeDrug { get; set; }
        public override void Release()
        {
            PetID = 0;
            HPAptitude = 0;
            SoulAptitude = 0;
            AttackspeedAptitude = 0;
            AttackphysicalAptitude = 0;
            DefendphysicalAptitude = 0;
            AttackpowerAptitude = 0;
            DefendpowerAptitude = 0;
            Petaptitudecol = 0;
            PetAptitudeDrug = null;
        }
    }
}
