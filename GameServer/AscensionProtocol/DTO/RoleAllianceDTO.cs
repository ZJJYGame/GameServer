using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 角色宗门数据
    /// </summary>
    [Serializable]
    public class RoleAllianceDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        /// <summary>
        /// 宗门ID
        /// </summary>
        public virtual int  AllianceID { get; set; }
        /// <summary>
        /// 宗门职位
        /// </summary>
        public virtual int AllianceJob { get; set; }
        /// <summary>
        /// 宗门贡献
        /// </summary>
        public virtual int Reputation { get; set; }
        /// <summary>
        /// 角色月贡
        /// </summary>
        public virtual int ReputationMonth { get; set; }
        /// <summary>
        /// 角色历史贡献
        /// </summary>
        public virtual int ReputationHistroy { get; set; }
        /// <summary>
        /// 玩家加入时间
        /// </summary>
        public virtual string  JoinTime { get; set; }
        /// <summary>
        /// 角色离线时间
        /// </summary>
        public virtual string Offline { get; set; }
        /// <summary>
        /// 玩家名称
        /// </summary>
        public virtual string RoleName { get; set; }
        /// <summary>
        /// 玩家申请的宗门
        /// </summary>
        public virtual List<int>ApplyForAlliance { get; set; }
        /// <summary>
        /// 玩家等级
        /// </summary>
        public virtual int RoleLevel { get; set; }
        public override void Release()
        {
            RoleID = -1;
            AllianceID = 0;
            AllianceJob = 0;
            Reputation = 0;
            ReputationMonth = 0;
            ReputationHistroy = 0;
            JoinTime = null;
            Offline = null;
            RoleName = null;
            ApplyForAlliance =null;
            RoleLevel = 0;
        }
    }
}
