using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    /// <summary>
    /// 场景资源对象；
    /// </summary>
    [Serializable]
    public struct LevelResObject
    {
        /// <summary>
        /// 元素生成时的序号；
        /// </summary>
        public int Index { get; set;}
        /// <summary>
        /// 资源的全局id;
        /// </summary>
        public int GId { get; set; }
        /// <summary>
        /// 被生成时，某一类型的单个元素的序号；
        /// 大组(index)---小组(gId)---元素(eleId)；
        /// </summary>
        public int EleId { get; set; }
    }
}
