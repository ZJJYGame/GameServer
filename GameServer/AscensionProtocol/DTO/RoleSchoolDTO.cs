using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleSchoolDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int RoleJoiningSchool { get; set; }
        public virtual int RoleJoinedSchool { get; set; }
        public virtual Dictionary<int, string> RoleSchoolHatred { get;set;}

        public override void Release()
        {
            RoleID = 0;
            RoleJoiningSchool=0;
            RoleJoinedSchool=0;
            RoleSchoolHatred.Clear();
        }
    }
}
