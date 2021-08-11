using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AllianceSigninDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        /// <summary>
        /// 宗门ID
        /// </summary>
        public virtual int AllianceID { get; set; }
        /// <summary>
        /// 是否签到
        /// </summary>
        public virtual bool IsSignin { get; set; }
        public override void Release()
        {
            RoleID = 0;
            AllianceID = 0;
            IsSignin = false;
        }
    }
}
