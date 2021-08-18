using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class PetDrugRefreshDTO : DataTransferObject
    {
        public virtual int PetID { get; set; }
        public virtual Dictionary<int, int> PetUsedDrug { get; set; }

        public override void Release()
        {
            PetID = 0;
            PetUsedDrug.Clear();
        }
    }
}
