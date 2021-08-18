using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AllianceDongFuDTO : DataTransferObject
    {
        public virtual int AllianceID { get; set; }
        /// <summary>
        /// 用於判斷是否發起占領
        /// </summary>
        public virtual bool IsPreempt { get; set; }
        /// <summary>
        /// 占領人，用於顯示自身占領與傳輸他人占領請求
        /// </summary>
        public virtual int Occupant { get; set; }
        public virtual int SpiritRangeID { get; set; }
        public virtual List<PreemptInfo> PreemptOne { get; set; }
        public virtual List<PreemptInfo> PreemptTow { get; set; }
        public virtual List<PreemptInfo> PreemptThree { get; set; }
        public virtual List<PreemptInfo> PreemptFour { get; set; }
        public virtual List<PreemptInfo> PreemptFive { get; set; }

        public AllianceDongFuDTO()
        {
            AllianceID = -1;
            IsPreempt = false;
            SpiritRangeID = -1;
            PreemptOne = new List<PreemptInfo>();
            PreemptTow = new List<PreemptInfo>();
            PreemptThree = new List<PreemptInfo>();
            PreemptFour = new List<PreemptInfo>();
            PreemptFive = new List<PreemptInfo>();
        }

        public override void Release()
        {
            AllianceID = -1;
            SpiritRangeID = -1;
        }
    }
    [Serializable]
    public class PreemptInfo
    {
        public virtual int RoleID { get; set; }
        public virtual string RoleName { get; set; }
        public virtual int HeadPortrait { get; set; }
    }
}
