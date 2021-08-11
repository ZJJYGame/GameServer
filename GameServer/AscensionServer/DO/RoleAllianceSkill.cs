using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RoleAllianceSkill : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual int SkillStrong { get; set; }
        public virtual int SkillInsight { get; set; }
        public virtual int SkillMeditation { get; set; }
        public virtual int SkillRapid { get; set; }

        public RoleAllianceSkill()
        {
            SkillStrong =0;
            SkillInsight =0;
            SkillMeditation = 0;
            SkillRapid =0;
        }

        public override void Release()
        {
            RoleID = -1;
            SkillStrong = 0;
            SkillInsight = 0;
            SkillMeditation = 0;
            SkillRapid = 0;

        }
    }
}


