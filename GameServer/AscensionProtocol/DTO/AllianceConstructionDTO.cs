using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 宗门建设数据
    /// </summary>
    [Serializable]
   public  class AllianceConstructionDTO:DataTransferObject
    {
        /// <summary>
        /// 宗门ID
        /// </summary>
        public virtual int AllianceID { get; set; }
        /// <summary>
        /// 宗门演武场
        /// </summary>
        public virtual int AllianceArmsDrillSite { get; set; }
        /// <summary>
        /// 丹药房
        /// </summary>
        public virtual int AllianceAlchemyStorage { get; set; }
        /// <summary>
        /// 宗门藏经阁
        /// </summary>
        public virtual int AllianceScripturesPlatform { get; set; }
        /// <summary>
        /// 宗门议事厅
        /// </summary>
        public virtual int AllianceChamber { get; set; }
        /// <summary>
        /// 宗门资产
        /// </summary>
        public virtual int AllianceAssets { get; set; }


        public override void Release()
        {
            AllianceID = -1;
            AllianceArmsDrillSite = 0;
            AllianceAlchemyStorage = 0;
            AllianceScripturesPlatform = 0;
            AllianceChamber = 0;
            AllianceAssets = 0;
        }
    }
}
