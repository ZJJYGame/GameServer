using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class Pet:DataObject
    {
        public Pet()
        {
            PetLevel = 1;
            PetID = 23001;
            PetExp = 0;
            PetName = "水貂鸽头";
            PetSkillArray=null;
            DemonicSoul = "{}";
        }
        public virtual int ID { get; set; }
        public virtual int PetLevel { get; set; }
        public virtual int PetID { get; set; }
        public virtual int PetExp { get; set; }
        public virtual string PetName { get; set; }
        public virtual string PetSkillArray { get; set; }
        public virtual string DemonicSoul { get; set; }
        public override void Release()
        {
            PetLevel =1;
            PetID = 0;
            PetExp = 0;
            PetSkillArray= "{}";
        }
    }
}


