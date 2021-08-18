using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
namespace Protocol
{
    [Serializable]
    [MessagePackObject(true)]
    public class S2CMapResource : IDataContract
    {
        public FixContainer Container { get; set; }
        /// <summary>
        /// 一组C2SWildResource的list集合；
        /// </summary>
        public List<C2SMapRes> MapResourceList { get; set; }
    }
}
