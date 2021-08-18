using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class TacticalDTO : DataTransferObject
    {
        /// <summary>
        ///阵法释放者
        /// </summary>
        public int RoleID { get; set; }
        /// <summary>
        /// 此ID表示为添加时候分配的资源ID，而非全局ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 所处地图块ID
        /// </summary>
        public int LevelID { get; set; }
        /// <summary>
        /// 阵法的全局ID
        /// </summary>
        public int GlobalID { get; set; }
        /// <summary>
        /// 阵法的持续时间
        /// </summary>
        public int Duration { get; set; }
        public override void Release()
        {
            ID = -1;
            RoleID = 0;
            GlobalID = 0;
            Duration = 0;
        }
    }
}
