using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AlliancesDTO : DataTransferObject
    {
        /// <summary>
        /// 宗门唯一ID
        /// </summary>
        public virtual int ID { get; set; }
        /// <summary>
        /// 储存所有的额宗门ID
        /// </summary>
        public virtual List<int> AllianceList { get; set; }
        /// <summary>
        /// 当前收到的宗门下标
        /// </summary>
        public virtual int Index { get; set; }
        /// <summary>
        /// 宗门的宗门数量
        /// </summary>
        public virtual int AllIndex { get; set; }
        /// <summary>
        /// 是否刷新
        /// </summary>
        public virtual bool ISRefresh { get; set; }
        public override void Release()
        {
            ID = -1;
            AllianceList = null;
            ISRefresh = true;
        }
    }
}
