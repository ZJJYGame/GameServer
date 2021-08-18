using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{    /// <summary>
     /// 宗门丹药数据
     /// </summary>
    [Serializable]
    public class ExchangeDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int AllianceID { get; set; }
        public virtual int GoodsNum { get; set; }
        public virtual int GoodsID { get; set; }
        public override void Release()
        {
            RoleID = -1;
            AllianceID = -1;
            GoodsNum = 0;
            GoodsID = 0;
        }
    }
}
