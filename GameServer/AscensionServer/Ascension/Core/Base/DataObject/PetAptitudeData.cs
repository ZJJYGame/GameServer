using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    [ConfigData]
    public class PetAptitudeData
    {
        public int PetID { get; set; }
        public List<int> NaturalHP { get; set; }
        public List<int> NaturalSoul { get; set; }
        public List<int> NaturalAttackSpeed { get; set; }
        public List<int> NaturalAttackPhysical { get; set; }
        public List<int> NaturalDefendPhysical { get; set; }
        public List<int> NaturalAttackPower { get; set; }
        public List<int> NaturalDefendPower { get; set; }
        public List<int> NaturalGrowUp { get; set; }
        public List<int> SkillArray { get; set; }
        public int NeedItemNumber { get; set; }        
    }
}


