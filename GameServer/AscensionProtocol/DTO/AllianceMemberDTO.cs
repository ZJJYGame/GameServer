using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AllianceMemberDTO : DataTransferObject
    {
        public virtual int AllianceID { get; set; }
        /// <summary>
        /// 宗门申请人员列表
        /// </summary>
        public virtual List<int> ApplyforMember { get; set; }
        /// <summary>
        /// 宗门成员列表
        /// </summary>
        public virtual List<int> Member { get; set; }

        public virtual Dictionary<int,int> JobNumDict { get; set; }
        public AllianceMemberDTO()
        {
            AllianceID = -1;
            ApplyforMember = new List<int>();
            Member = null;
            JobNumDict = new Dictionary<int, int>();
            JobNumDict.Add(931, 0);
            JobNumDict.Add(932, 0);
            JobNumDict.Add(933, 0);
            JobNumDict.Add(934, 0);
            JobNumDict.Add(935, 0);
            JobNumDict.Add(936, 0);
        }
        public override void Release()
        {
            AllianceID = -1;
            ApplyforMember =new List<int>();
            Member = null;
            JobNumDict = new Dictionary<int, int>();
            JobNumDict.Add(931, 0);
            JobNumDict.Add(932, 0);
            JobNumDict.Add(933, 0);
            JobNumDict.Add(934, 0);
            JobNumDict.Add(935, 0);
            JobNumDict.Add(936, 0);
        }
    }
}
