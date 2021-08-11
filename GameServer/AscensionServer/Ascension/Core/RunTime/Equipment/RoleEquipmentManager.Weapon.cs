using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;

namespace AscensionServer
{
   public partial class RoleEquipmentManager
    {
        /// <summary>
        /// 装备武器
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
       async void EquipWeaponS2C(RoleEquipmentDTO equipmentDTO)
        {
            var roleequipExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
            var roleweaponExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
            var roleStatusExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;

            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", equipmentDTO.RoleID);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);

            if (roleequipExist&&roleweaponExist&& roleStatusExist)
            {
                var roleequip = RedisHelper.Hash.HashGetAsync<RoleEquipmentDTO>(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
                var roleweapon = RedisHelper.Hash.HashGetAsync<RoleWeaponDTO>(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
                var roleStatus = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;

                if (roleequip != null && roleweapon != null&& roleStatus!=null)
                {
                    foreach (var item in equipmentDTO.Weapon)
                    {
                        if (!InventoryManager.VerifyIsExist(item.Value, 1, ringServer.RingIdArray))
                        {
                            EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
                            return;
                        }
                        var result = roleequip.Weapon.TryGetValue(item.Key, out var weaponid);
                        if (result)
                        {
                            InventoryManager.AddNewItem(equipmentDTO.RoleID, weaponid, 1);
                            InventoryManager.Remove(equipmentDTO.RoleID, item.Value);
                            roleequip.Weapon[item.Key] = item.Value;
                        }
                        else
                        {
                            roleequip.Weapon.Add(item.Key, item.Value);
                            InventoryManager.Remove(equipmentDTO.RoleID, item.Value);
                        }
                        var status = await GameEntry.practiceManager.RoleEquip(roleStatus,roleweapon, roleequip);


                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.RoleEquipment, roleequip);
                        dict.Add((byte)ParameterCode.RoleStatus, status);
                        EquipmentSuccessS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon, dict);

                       await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString(), roleequip);
                        await NHibernateQuerier.UpdateAsync(ChangeDataType(roleequip));

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString(), status);
                        await NHibernateQuerier.UpdateAsync(status);
                    }
                }
                else
                    EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
            }
            else
                EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
        }
        /// <summary>
        /// 装备法宝
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
       async void EquipMagicWeaponS2C(RoleEquipmentDTO equipmentDTO)
        {
            var roleequipExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
            var roleweaponExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
            var roleStatusExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;

            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", equipmentDTO.RoleID);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);

            if (roleequipExist && roleweaponExist&& roleStatusExist)
            {
                var roleequip = RedisHelper.Hash.HashGetAsync<RoleEquipmentDTO>(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
                var roleweapon = RedisHelper.Hash.HashGetAsync<RoleWeaponDTO>(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
                var roleStatus = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;

                if (roleequip != null && roleweapon != null&& roleStatus!=null)
                {
                    foreach (var item in equipmentDTO.MagicWeapon)
                    {
                        if (!InventoryManager.VerifyIsExist(item.Value, 1, ringServer.RingIdArray))
                        {
                            EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
                            return;
                        }
                        Utility.Debug.LogInfo("角色装备装备成功收到"+ item.Key);
                        var result = roleequip.MagicWeapon.TryGetValue(item.Key, out var weaponid);
                        if (result)
                        {
                            InventoryManager.AddNewItem(equipmentDTO.RoleID, weaponid,1);
                            InventoryManager.Remove(equipmentDTO.RoleID, item.Value);
                            roleequip.MagicWeapon[item.Key] = item.Value;
                        }
                        else
                        {
                            roleequip.MagicWeapon.Add(item.Key, item.Value);
                            InventoryManager.Remove(equipmentDTO.RoleID, item.Value);
                        }

                        var status = await GameEntry.practiceManager.RoleEquip(roleStatus, roleweapon, roleequip);

                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.RoleEquipment, roleequip);
                        dict.Add((byte)ParameterCode.RoleStatus, roleStatus);
                        EquipmentSuccessS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon, dict);

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString(), roleequip);
                        await NHibernateQuerier.UpdateAsync(ChangeDataType(roleequip));

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString(), status);
                        await NHibernateQuerier.UpdateAsync(status);
                    }
                }
                else
                    EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
            }
            else
                EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
        }


        /// <summary>
        /// 卸下装备
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
       async void RemoveWeaponS2C(RoleEquipmentDTO equipmentDTO)
        {
            var roleequipExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
            var roleweaponExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
            var roleStatusExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", equipmentDTO.RoleID);

            if (roleequipExist && roleweaponExist&& roleStatusExist)
            {
                foreach (var item in equipmentDTO.Weapon)
                {
                    var roleequip = RedisHelper.Hash.HashGetAsync<RoleEquipmentDTO>(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
                    var roleweapon = RedisHelper.Hash.HashGetAsync<RoleWeaponDTO>(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
                    var roleStatus = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;
                    if (roleequip != null && roleweapon != null&& roleStatus!=null)
                    {
                        var result = roleequip.Weapon.TryGetValue(item.Key, out var weaponid);
                        if (result)
                        {
                            InventoryManager.AddNewItem(equipmentDTO.RoleID, item.Value, 1);
                            roleequip.Weapon.Remove(item.Key);
                            var status = await GameEntry.practiceManager.RoleEquip(roleStatus, roleweapon, roleequip);

                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.RoleEquipment, roleequip);
                            dict.Add((byte)ParameterCode.RoleStatus, roleStatus);
                            EquipmentSuccessS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon, dict);

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString(), roleequip);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(roleequip));


                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString(), status);
                            await NHibernateQuerier.UpdateAsync(status);
                        }
                    }
                    else
                        EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
                }
            }
        }
        /// <summary>
        /// 卸下法宝
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
       async void RemoveMagicWeaponS2C(RoleEquipmentDTO equipmentDTO)
        {
            var roleequipExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
            var roleweaponExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
            var roleStatusExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;

            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", equipmentDTO.RoleID);

            if (roleequipExist && roleweaponExist&& roleStatusExist)
            {
                foreach (var item in equipmentDTO.MagicWeapon)
                {
                    var roleequip = RedisHelper.Hash.HashGetAsync<RoleEquipmentDTO>(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString()).Result;
                    var roleweapon = RedisHelper.Hash.HashGetAsync<RoleWeaponDTO>(RedisKeyDefine._RoleWeaponPostfix, equipmentDTO.RoleID.ToString()).Result;
                    var roleStatus = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString()).Result;
                    if (roleequip != null && roleweapon != null&& roleStatus!=null)
                    {
                        var result = roleequip.MagicWeapon.TryGetValue(item.Key, out var weaponid);
                        if (result)
                        {
                            InventoryManager.AddNewItem(equipmentDTO.RoleID, item.Value, 1);
                            roleequip.MagicWeapon.Remove(item.Key);
                            var status = await GameEntry.practiceManager.RoleEquip(roleStatus, roleweapon, roleequip);

                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.RoleEquipment, roleequip);
                            dict.Add((byte)ParameterCode.RoleStatus, roleStatus);
                            EquipmentSuccessS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon, dict);


                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleEquipmentPerfix, equipmentDTO.RoleID.ToString(), roleequip);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(roleequip));


                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, equipmentDTO.RoleID.ToString(), status);
                            await NHibernateQuerier.UpdateAsync(status);
                        }
                    }
                    else
                        EquipmentFailS2C(equipmentDTO.RoleID, EquipmentOpCode.EquipWeapon);
                }
            }
        }
        RoleEquipment ChangeDataType(RoleEquipmentDTO equipmentDTO)
        {
            RoleEquipment equipment = new RoleEquipment();
            equipment.RoleID = equipmentDTO.RoleID;
            equipment.MagicWeapon = Utility.Json.ToJson(equipmentDTO.MagicWeapon);
            equipment.Weapon = Utility.Json.ToJson(equipmentDTO.Weapon);
            return equipment;
        }
    }
}
