using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class PetAbilityPointDTO : DataTransferObject
    {
        public virtual int ID { get; set; }
        public virtual int SlnNow { get; set; }
        public virtual bool IsUnlockSlnThree { get; set; }
        public virtual Dictionary<int, AbilityDTO> AbilityPointSln { get; set; }
        public virtual AbilityPointType AddPointType { get; set; }

        public PetAbilityPointDTO()
        {
            SlnNow = 0;
            IsUnlockSlnThree = false;
            AbilityPointSln = new Dictionary<int, AbilityDTO>();
            AbilityPointSln.Add(0,new AbilityDTO() { SlnName="方案一"});
            AbilityPointSln.Add(1, new AbilityDTO() { SlnName = "方案二" });
            AddPointType = AbilityPointType.None;
        }

        public override void Release()
        {
            ID = -1;
            SlnNow = 0;
            IsUnlockSlnThree = false;
            AbilityPointSln.Clear();
            AddPointType = AbilityPointType.None;
        }
        public enum AbilityPointType
        {
            None=0,
            Reset=1,
            Update=2,
            Unlock=3,
            Rename=4
        }
    }
}
