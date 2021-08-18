using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    [ConfigData]
    class SecondaryJobData
    {
        public byte SecondaryType { get; set; }
        public List<int> SecondaryJobID { get; set; }
        public List<int> SecondaryJobLevel { get; set; }
        public List<int> SecondaryJobExp { get; set; }
    }
}
