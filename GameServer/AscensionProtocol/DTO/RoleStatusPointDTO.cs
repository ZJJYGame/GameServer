using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleStatusPointDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int SlnNow { get; set; }
        public virtual Dictionary<int, AbilityDTO> AbilityPointSln { get; set; }

        public RoleStatusPointDTO()
        {
            SlnNow = 0;
            AbilityPointSln = new Dictionary<int, AbilityDTO>();
            AbilityPointSln.Add(0, new AbilityDTO() { SlnName = "方案一" });
            AbilityPointSln.Add(1, new AbilityDTO() { SlnName = "方案二" });
            AbilityPointSln.Add(2, new AbilityDTO() { SlnName = "方案三" });
        }

        public override void Release()
        {
            RoleID = -1;
            SlnNow = 0;
            AbilityPointSln.Clear();
        }
    }
}
