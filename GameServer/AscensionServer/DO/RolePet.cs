using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RolePet:DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string PetIDDict { get; set; }
        public virtual int PetIsBattle { get; set; }

        public RolePet()
        {
            PetIDDict = null;
            PetIsBattle = 0;
        }

        public override void Release()
        {
            RoleID = -1;
            PetIDDict = null;
            PetIsBattle=0;
        }
    }
}


