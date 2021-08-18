using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
namespace AscensionServer.Model
{
    [Serializable]
    public class RoleEquipment:DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string Weapon { get; set; }
        public virtual string MagicWeapon { get; set; }

        public RoleEquipment()
        {
            RoleID = 0;
            Weapon = "{}";
            MagicWeapon="{}";
        }

        public override void Release()
        {
           
        }
    }
}
