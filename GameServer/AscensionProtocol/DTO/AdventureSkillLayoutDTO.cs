using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using AscensionProtocol;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class AdventureSkillLayoutDTO : DataTransferObject
    {
        public Dictionary<string, int> SkillLayoutDict { get; set; }
        public Dictionary<string, int> PropLayoutDict { get; set; }
        public Dictionary<string, int> FlyToolLayoutDict { get; set; }
        public override void Release()
        {
            SkillLayoutDict.Clear();
            PropLayoutDict.Clear();
            FlyToolLayoutDict.Clear();
        }
    }
}
