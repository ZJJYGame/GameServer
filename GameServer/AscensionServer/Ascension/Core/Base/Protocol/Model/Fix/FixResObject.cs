using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    public class FixResObject:IDisposable
    {
        public int Index { get; set; }
        public bool Occupied { get; set; }
        public FixTransform FixTransform { get; set; }
        public void Dispose()
        {
            FixTransform = FixTransform.Zero;
            Index = -1;
            Occupied = false;
        }
    }
}
