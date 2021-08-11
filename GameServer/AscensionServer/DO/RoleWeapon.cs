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
    public class RoleWeapon:DataObject
    {

        public virtual int RoleID { set; get; }
        public virtual string WeaponStatusDict { set; get; }
        public virtual string Weaponindex { set; get; }//武器全局id为key,新增id下标为value
        public virtual string MagicStatusDict { set; get; }
        public virtual string Magicindex { set; get; }

        public RoleWeapon()
        {
            RoleID = -1;
            WeaponStatusDict =Utility.Json.ToJson(new Dictionary<int,WeaponDTO>());
            Weaponindex = Utility.Json.ToJson(new Dictionary<int, int>()); 
            MagicStatusDict = Utility.Json.ToJson(new Dictionary<int, WeaponDTO>()); ;
            Magicindex = Utility.Json.ToJson(new Dictionary<int, int>());
        }
        public override void Release()
        {
            RoleID = -1;
            WeaponStatusDict = null;
            Weaponindex = null;
        }
    }
}


