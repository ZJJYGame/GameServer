using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public  class RolePuppetDTO: DataTransferObject
    {
        public virtual int RoleID { set; get; }
        public virtual int IsBattle { set; get; }
        public virtual Dictionary<int,int> PuppetDict { set; get; }

        public RolePuppetDTO()
        {
            RoleID = 0;
            IsBattle = 0;
            PuppetDict = new Dictionary<int, int>();
        }
        public override void Release()
        {
          
        }
    }

}
