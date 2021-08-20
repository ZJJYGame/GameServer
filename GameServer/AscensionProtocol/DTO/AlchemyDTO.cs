using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AlchemyDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int JobLevel { get; set; }
        public virtual int JobLevelExp { get; set; }
        public virtual HashSet<int> Recipe_Array { get; set; }
        public AlchemyDTO()
        {
            JobLevel = 1;
            Recipe_Array = new HashSet<int>();
            JobLevelExp = 0;
        }


        public override void Release()
        {
            RoleID = -1;
            JobLevel = 0;
            Recipe_Array = new HashSet<int>();
            JobLevelExp = 0;
        }
    }  
}
