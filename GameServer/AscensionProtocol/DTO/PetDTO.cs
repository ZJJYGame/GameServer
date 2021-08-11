using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos;
namespace AscensionProtocol.DTO
{
    [Serializable]
    public class PetDTO: DataTransferObject
    {
        public virtual int ID { get; set; }
        public virtual int PetLevel { get; set; }
        public virtual int PetID { get; set; }
        public virtual int PetExp { get; set; }
        public virtual string PetName { get; set; }
        public virtual List<int> PetSkillArray { get; set; }
        public virtual Dictionary<int,List<int>> DemonicSoul { get; set; }
        public virtual PetOperateType OperateType { get; set; }
        public override void Release()
        {

            PetLevel = 0;
            PetID = 0;
            PetExp = 0;
            PetName = null;
            PetSkillArray.Clear();
            DemonicSoul.Clear();
            OperateType = PetOperateType.None;
        }

        public enum PetOperateType
        {
            None=0,
            Rename=1,
            Equip=2,
            Unequip=3
        }
    }
}
