using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionServer.Model;
using FluentNHibernate.Mapping;

namespace AscensionServer.Model
{
    [Serializable]
    public class RoleSchool : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual int RoleJoiningSchool { get; set; }
        public virtual int RoleJoinedSchool { get; set; }
        public virtual string RoleSchoolHatred { get; set; }


        public override void Release()
        {
            RoleID = 0;
            RoleJoiningSchool = 0;
            RoleJoinedSchool = 0;
            RoleSchoolHatred=null;
        }
    }
}


