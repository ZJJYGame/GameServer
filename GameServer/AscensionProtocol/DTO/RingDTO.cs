using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RingDTO: DataTransferObject
    {
        public virtual int ID { set; get; }
        public virtual int RingId { get; set; }
        public virtual Dictionary<int,RingItemsDTO> RingItems { get; set; }
        public virtual Dictionary<int,int> MagicDictServer { get; set; }
        public virtual Dictionary<int, RingItemsDTO> RingAdorn { get; set; }
        public override void Release()
        {
            ID = -1;
            RingId = 0;
            RingItems.Clear();
            MagicDictServer.Clear();
            RingAdorn.Clear();
        }
    }
    [Serializable]
    public class RingItemsDTO
    {
        public virtual int RingItemCount { get; set; }
        public virtual string RingItemAdorn { get; set; }
        public virtual string RingItemTime { get; set; }
        public virtual int RingItemMax { get; set; }
        /// <summary>
        /// 1-8  对应 八个格子
        /// </summary>
        public virtual short RingItemType { get; set; }
    }
}
