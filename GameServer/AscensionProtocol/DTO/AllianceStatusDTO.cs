using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 宗门属性
    /// </summary>
    [Serializable]
    public class AllianceStatusDTO : DataTransferObject
    {
        public virtual int ID { get; set; }
        /// <summary>
        /// 宗门等级
        /// </summary>
        public virtual int AllianceLevel { get; set; }
        /// <summary>
        /// 宗门人数
        /// </summary>
        public virtual int AllianceNumberPeople { get; set; }
        /// <summary>
        /// 宗门最大人数
        /// </summary>
        public virtual int AlliancePeopleMax { get; set; }
        /// <summary>
        /// 宗门盟主
        /// </summary>
        public virtual string AllianceMaster { get; set; }
        /// <summary>
        /// 宗门名称
        /// </summary>
        public virtual string AllianceName { get; set; }
        /// <summary>
        /// 宗门人气
        /// </summary>
        public virtual int Popularity { get; set; }
        /// <summary>
        /// 宗门宗旨
        /// </summary>
        public virtual string Manifesto { get; set; }
        /// <summary>
        /// 在线人数
        /// </summary>
        public virtual int OnLineNum { get; set; }

        public override void Release()
        {
            ID = -1;
            AllianceLevel = 0;
            AlliancePeopleMax = 0;
            AllianceMaster = null;
            AllianceNumberPeople = 0;
            AllianceName = null;
            Popularity = 0;
            OnLineNum = 0;
        }
    }
}
