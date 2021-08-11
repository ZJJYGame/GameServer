using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class PuppetDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int JobLevel { get; set; }
        public virtual int JobLevelExp { get; set; }
        public virtual HashSet<int> Recipe_Array { get; set; }
        public PuppetDTO()
        {
            JobLevel = 0;
            JobLevelExp = 0;
            Recipe_Array = new HashSet<int>();
        }

        public override void Release()
        {
            RoleID = -1;
            JobLevel = 0;
            JobLevelExp = 0;
            Recipe_Array = null;
        }
    }
}
