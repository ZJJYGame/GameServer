using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    ///发送日常消息的DTO，如人物升级获得珍惜物品等记录消息内容等
    public class DailyMessageDTO : DataTransferObject
    {
        public virtual int CurrentIndex { get; set; }
        public virtual string DataTimeNow { get; set; }
        public virtual bool IsFirstGet { get; set; }
        public virtual Dictionary<string, List<DailyMessageData>> DailyMessageDict { get; set; }
        public override void Release()
        {
            CurrentIndex = 0;
            DataTimeNow = null;
            IsFirstGet = false;
            DailyMessageDict = new Dictionary<string, List<DailyMessageData>>();
        }
    }

    public class DailyMessageData
    {
        public virtual string Name { get; set; }
        public virtual string Describe { get; set; }
        public virtual string EventContent { get; set; }

        public DailyMessageData()
        {
            Name = null;
            Describe = null;
            EventContent = null;
        }
    }
}
