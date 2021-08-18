using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionServer.Model;
using NHibernate.Linq.Clauses;
using AscensionProtocol.DTO;
using Renci.SshNet.Security;
using Cosmos;

namespace AscensionServer
{
    /// <summary>
    /// InventoryManager  InventoryManager.Client    就这两个有用拉   剩下的都可以删除拉
    /// </summary>
    public partial class InventoryManager
    {

        /// <summary>
        /// 转换为五位的全局id
        /// </summary>
        /// <param name="_ItemID"></param>
        /// <returns></returns>
        public static int ConvertInt32(int _ItemID)
        {
            string sub = _ItemID.ToString().Substring(0, 5);
            return Int32.Parse(sub);
        }

        /// <summary>
        /// 装备的类型
        /// </summary>
        /// <param name="_ItemID"></param>
        /// <returns></returns>
        public static EquipType EquipTypeMethod(int _ItemID)
        {
            GameEntry. DataManager.TryGetValue<Dictionary<int, EquipmentData>>(out var EquipDict);
            GameEntry. DataManager.TryGetValue<Dictionary<int, RingData>>(out var RingDict);
            GameEntry. DataManager.TryGetValue<Dictionary<int, MagicWeaponData>>(out var MagicWeaponDict);
            _ItemID = ConvertInt32(_ItemID);
            if (EquipDict.ContainsKey(_ItemID))
                return EquipDict[_ItemID].Weapon_Type;
            if (RingDict.ContainsKey(_ItemID))
                return RingDict[_ItemID].Ring_Type;
            if (MagicWeaponDict.ContainsKey(_ItemID))
                return MagicWeaponDict[_ItemID].Magic_Type;
            return EquipType.Default;
        }
        /// <summary>
        /// 添加的类型
        /// </summary>
        /// <param name="_itemId"></param>
        /// <returns></returns>
        public static short AddRingItemType(int _itemId)
        {
            short type = 0;
            switch (EquipTypeMethod(_itemId))
            {
                case EquipType.PlayerEquip:
                    type = 1;
                    break;
                case EquipType.PlayerOuterWear:
                    type = 2;
                    break;
                case EquipType.PlayerUnderWear:
                    type = 3;
                    break;
                case EquipType.PlayerShoe:
                    type = 4;
                    break;
                case EquipType.Player1:
                    type = 5;
                    break;
                case EquipType.Player2:
                    type = 6;
                    break;
                case EquipType.PlayerStorageBag:
                    type = 7;
                    break;
                case EquipType.PlayerSpiritBeastBag:
                    type = 8;
                    break;
                case EquipType.MagicWeapon:
                    break;
            }
            return type;
        }

        /// <summary>
        /// json 转换为字典中
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, ItemBagBaseData> InventotyDict()
        {
            GameEntry. DataManager.TryGetValue<Dictionary<int, ItemBagBaseData>>(out var ItemBagBaseDict);
            return ItemBagBaseDict;
        }

        /// <summary>
        /// 返回一个NHCriteria  映射对象
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static NHCriteria NHCriteria(int roleId)
        {
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleId);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteriaRoleID);
            return ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);
        }

        /// <summary>
        /// 单个物品
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_itemId"></param>
        /// <param name="_RingItemCount"></param>
        /// <param name="_RingItemAdorn"></param>
        public static void AddNewItem(int roleId, int _itemId, int _RingItemCount, string _RingItemAdorn = "0")
        {
            AddDataCmdS2C(roleId, new RingDTO() { RingItems = new Dictionary<int, RingItemsDTO>() { { _itemId, new RingItemsDTO() { RingItemAdorn = _RingItemAdorn, RingItemCount = _RingItemCount, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = InventotyDict()[Int32.Parse(_itemId.ToString().Substring(0, 5))].ItemMax, RingItemType = AddRingItemType(Int32.Parse(_itemId.ToString().Substring(0, 5))) } } } }, NHCriteria(roleId));
        }

        /// <summary>
        /// 更新一个物品
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_itemId"></param>
        /// <param name="_RingItemCount"></param>
        /// <param name="_RingItemAdorn"></param>
        public static void UpdateNewItem(int roleId, int _itemId, int _RingItemCount, string _RingItemAdorn = "0")
        {
            UdpateCmdS2C(roleId, new RingDTO() { RingItems = new Dictionary<int, RingItemsDTO>() { { _itemId, new RingItemsDTO() { RingItemCount = _RingItemCount, RingItemAdorn = _RingItemAdorn, RingItemType = AddRingItemType(Int32.Parse(_itemId.ToString().Substring(0, 5))) } } } }, NHCriteria(roleId));
        }


        /// <summary>
        /// 移除功能
        /// </summary>
        /// <param name="_ItemId"></param>
        public static void Remove(int roleId, int _ItemId)
        {
            RemoveCmdS2C(roleId, new RingDTO() { RingItems = new Dictionary<int, RingItemsDTO>() { { _ItemId, new RingItemsDTO() } } }, NHCriteria(roleId));
        }

    }
}


