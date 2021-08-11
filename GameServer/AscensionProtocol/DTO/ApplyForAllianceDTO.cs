using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 用于传输仙盟申请列表的数据类型
    /// </summary>
    [Serializable]
    public class ApplyForAllianceDTO : DataTransferObject
    {
        /// <summary>
        /// 申请角色名称
        /// </summary>
        public virtual string MemberName { get; set; }
        public virtual int Level  { get; set; }
        public virtual int RoleID { get; set; }

        public override void Release()
        {
            MemberName = null;
            Level = 0;
            RoleID = 0;
        }
    }
}
