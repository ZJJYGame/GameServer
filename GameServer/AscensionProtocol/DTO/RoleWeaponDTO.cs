using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleWeaponDTO : DataTransferObject
    {
       public virtual int RoleID { set; get; }
        public virtual int WeaponID { set; get; }
        public virtual Dictionary<int, WeaponDTO> WeaponStatusDict { set; get; }
        public virtual Dictionary<int, int> Weaponindex { set; get; }//武器全局id为key,新增id下标为value
        public virtual Dictionary<int, WeaponDTO> MagicStatusDict { set; get; }
        public virtual Dictionary<int, int> Magicindex { set; get; }//武器全局id为key,新增id下标为value

        public RoleWeaponDTO()
        {
            WeaponStatusDict = new Dictionary<int, WeaponDTO>();
            MagicStatusDict = new Dictionary<int, WeaponDTO>();
            Weaponindex = new Dictionary<int, int>();
            Magicindex = new Dictionary<int, int>();
        }

        public override void Release()
        {
            RoleID = -1;
            WeaponStatusDict = null;
            Weaponindex =null;
            MagicStatusDict = null;
            Magicindex = null;
        }      
    }
    [Serializable]
    public class WeaponDTO : DataTransferObject
    {
        /// <summary>
        /// 武器法宝评分
        /// </summary>
        public virtual int Score { set; get; }
        public virtual bool IsTreasure { set; get; }
        public virtual List<int> WeaponAttribute { set; get; }
        public virtual int WeaponDurable { set; get; }
        public virtual List<int> WeaponSkill { set; get; }
        public virtual int WeaponType { set; get; }
        public WeaponDTO()
        {
            Score = 1;
            IsTreasure = false;
            WeaponAttribute = new List<int>() ;
            WeaponDurable = 1;
            WeaponSkill = new List<int>() ;
            WeaponType = 0;
        }

        public override void Release()
        {
            Score = 1;
            IsTreasure =false;
            WeaponAttribute = new List<int>() ;
            WeaponDurable = 1;
            WeaponSkill = new List<int>() ;
        }
    }
}
