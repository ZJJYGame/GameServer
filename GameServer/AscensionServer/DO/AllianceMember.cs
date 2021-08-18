using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer.Model
{
    [Serializable]
    public class AllianceMember : DataObject
    {
        public virtual int AllianceID { get; set; }
        public virtual string ApplyforMember { get; set; }
        public virtual string Member { get; set; }
        public virtual string JobNumDict { get; set; }
        public AllianceMember()
        {
            ApplyforMember ="[]";
            Member = null;
            Dictionary<int, int> dict = new Dictionary<int, int>();
            dict.Add(931,0);
            dict.Add(932, 0);
            dict.Add(933, 0);
            dict.Add(934, 0);
            dict.Add(935, 0);
            dict.Add(936, 0);
            JobNumDict = Utility.Json.ToJson(dict);
        }


        public override void Release()
        {
            AllianceID = -1;
            ApplyforMember = null;
            Member = null;
            JobNumDict = "{}";
        }
    }
}


