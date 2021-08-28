using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    public class FixCollectable : IDisposable
    {
        /// <summary>
        /// GId
        /// </summary>
        public int Id { get; set; }
        public Dictionary<int, FixResObject> CollectableDict { get; set; }
        public void RenewalAll()
        {
            foreach (var col in CollectableDict.Values)
            {
                col.Occupied = false;
            }
        }
        public void Renewal(int eleId)
        {
            if (CollectableDict.TryGetValue(eleId, out var cr))
            {
                cr.Occupied = false;
            }
        }
        public void Dispose()
        {
            Id = -1;
            CollectableDict.Clear();
        }
    }
}
