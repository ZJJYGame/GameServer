using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 宗门技能等级
    /// </summary>
    [Serializable]
    public class RoleAllianceSkillDTO :DataTransferObject
    {
        public virtual int RoleID { get; set; }
        /// <summary>
        /// 强体
        /// </summary>
        public virtual int SkillStrong { get; set; }
        /// <summary>
        /// 顿悟
        /// </summary>
        public virtual int SkillInsight { get; set; }
        /// <summary>
        /// 冥想
        /// </summary>
        public virtual int SkillMeditation { get; set; }
        /// <summary>
        /// 迅速
        /// </summary>
        public virtual int SkillRapid { get; set; }

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
