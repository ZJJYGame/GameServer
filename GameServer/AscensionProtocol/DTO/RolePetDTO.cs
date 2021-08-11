using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RolePetDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual Dictionary<int,int> PetIDDict { get; set; }
        public virtual int PetIsBattle { get; set; }
        public virtual int AddRemovePetID { get; set; }
        public virtual string AddPetName { get; set; }
        public virtual RolePetOperationalOrder RolePetOrderType { get; set; }
        public override void Release()
        {
            RoleID = -1;
            PetIDDict .Clear();
            PetIsBattle = 0;
            AddRemovePetID = 0;
            AddPetName = null;
        }
        public enum RolePetOperationalOrder
        {
           Battle=1,
           GetAllPet=2,
           RemovePet=3,
           AddPet=4,

        }
    }
}
