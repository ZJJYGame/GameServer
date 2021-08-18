using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    public class RoleAdventureSkillDTO : DataTransferObject
    {
        public int RoleID { get; set; }
        public bool IsInUse { get; set; }
        public int SkillID { get; set; }
        public int CDInterval { get; set; }
        public int BuffeInterval { get; set; }
        public short featureSkillTypeEnum { get; set; }

        public override void Release()
        {
            RoleID = -1;
            IsInUse = false;
            SkillID = 0;
            CDInterval = 0;
            BuffeInterval = 0;
            featureSkillTypeEnum = 0;
        }

    }
}
