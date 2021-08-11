using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    public class TacticalDeploymentDTO : DataTransferObject
    {
        /// <summary>
        ///地图块ID
        /// </summary>
        public int LevelID { get; set; }
        /// <summary>
        /// 释放的阵法
        /// 增加的生成ID为key,ID循环使用，具体的阵法数据为value
        /// </summary>
        public Dictionary<int, TacticalDTO> tacticDict { get; set; }
        //public List<TacticalDTO> tacticDict { get; set; }

        public TacticalDeploymentDTO()
        {
            tacticDict = new Dictionary<int, TacticalDTO>();
        }

        public override void Release()
        {
            LevelID = -1;
            tacticDict = null;

        }

    }
}
