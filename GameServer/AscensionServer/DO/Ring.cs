/*
*Author   Don
*Since 	2020-04-20
*Description 玩家当前储物戒指映射模型
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class Ring:DataObject
    {
        public virtual int ID { set; get; }
        public virtual int RingId { get; set; }
        public virtual string RingItems { get; set; }
        public virtual string RingMagicDictServer { get; set; }
        public virtual string RingAdorn { get; set; }
        public override void Release()
        {
            ID = -1;
            RingId = 0;
            RingItems = null;
            RingMagicDictServer = null;
            RingAdorn = null;
        }
    }
}



