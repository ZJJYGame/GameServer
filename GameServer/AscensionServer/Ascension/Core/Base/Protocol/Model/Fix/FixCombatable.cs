using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    public class FixCombatable
    {
        /// <summary>
        /// GId;
        /// </summary>
        public int GId { get; set; }
        /// <summary>
        /// EleId---FixResObject;
        /// </summary>
        public Dictionary<int, FixResObject> CombatableDict { get; set; }
        public void RenewalAll()
        {
            foreach (var col in CombatableDict.Values)
            {
                col.Occupied = false;
            }
        }
        public void Renewal(int eleId)
        {
            if (CombatableDict.TryGetValue(eleId, out var cr))
            {
                cr.Occupied = false;
            }
        }
        public void Dispose()
        {
            GId = -1;
            CombatableDict.Clear();
        }
    }
}
