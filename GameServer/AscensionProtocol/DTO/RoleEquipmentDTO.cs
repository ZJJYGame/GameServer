using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleEquipmentDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        /// <summary>
        /// 装备的装备位置信息key为部位，value为装备唯一ID
        /// </summary>
        public virtual Dictionary<int,int> Weapon { get; set; }
        public virtual Dictionary<int, int> MagicWeapon { get; set; }

        public RoleEquipmentDTO()
        {
            RoleID = 0;
            Weapon = new Dictionary<int, int>();
            MagicWeapon = new Dictionary<int, int>();
        }

        public override void Release()
        {
            
        }
    }
}
