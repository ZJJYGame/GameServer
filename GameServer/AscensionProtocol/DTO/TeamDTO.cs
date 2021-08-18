using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/* 
 * 
 * Author： xianren
 * Since : 2020 - 09 - 11 
 * Description : 组队的映射
 */
namespace AscensionProtocol.DTO
{
    [Serializable]
    public class TeamDTO:DataTransferObject
    {
        /// <summary>
        /// 队长Id
        /// </summary>
        public virtual int LeaderId { get; set; }
        /// <summary>
        /// 队伍Id
        /// </summary>
        public virtual int TeamId { get; set; }
        /// <summary>
        /// 队伍等级上限
        /// </summary>
        public virtual int TeamLevelUp { get; set; }
        /// <summary>
        /// 队伍等级下限
        /// </summary>
        public virtual  int TeamLevelDown { get; set; }
        /// <summary>
        /// 队伍成员列表
        /// </summary>
        public virtual List<RoleDTO> TeamMembers { get; set; }
        /// <summary>
        /// 队伍申请成员列表
        /// </summary>
        public virtual List<int> ApplyMebers { get; set; }
        /// <summary>
        /// 队伍同意列表
        /// </summary>
        public virtual List<int> AgreeMebers { get; set; }

        public override void Release()
        {
            LeaderId = 0;
            TeamId = 0;
            TeamLevelUp =0;
            TeamLevelDown = 0;
            TeamMembers = null;
            ApplyMebers = null;
            AgreeMebers = null;
        }
    }
}
