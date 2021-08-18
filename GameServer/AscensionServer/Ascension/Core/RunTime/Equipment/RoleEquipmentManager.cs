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
    [Module]
   public partial class RoleEquipmentManager : Cosmos.Module, IRoleEquipmentManager
    {
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncEquipment, ProcessHandlerC2S);
        }

        void ProcessHandlerC2S(int seeionid, OperationData packet)
        {
            Utility.Debug.LogInfo("角色装备装备成功收到" + Utility.Json.ToJson(packet.DataMessage));
            Utility.Debug.LogInfo("角色装备装备成功收到" + packet.SubOperationCode);
            var roleequip  =new RoleEquipmentDTO();
            switch ((EquipmentOpCode)packet.SubOperationCode)
            {
                case EquipmentOpCode.EquipWeapon:
                    roleequip = Utility.Json.ToObject<RoleEquipmentDTO>(packet.DataMessage.ToString());
                    EquipWeaponS2C(roleequip);
                    break;
                case EquipmentOpCode.EquipMagicWeapon:
                    roleequip = Utility.Json.ToObject<RoleEquipmentDTO>(packet.DataMessage.ToString());
                    EquipMagicWeaponS2C(roleequip);
                    break;
                case EquipmentOpCode.RemoveWeapon:
                    roleequip = Utility.Json.ToObject<RoleEquipmentDTO>(packet.DataMessage.ToString());
                    RemoveWeaponS2C(roleequip);
                    break;
                case EquipmentOpCode.RemoveMagicWeapon:
                    roleequip = Utility.Json.ToObject<RoleEquipmentDTO>(packet.DataMessage.ToString());
                    RemoveMagicWeaponS2C(roleequip);
                    break;
                case EquipmentOpCode.GetEquipStatus:
                    var role = Utility.Json.ToObject<RoleDTO>(packet.DataMessage.ToString());
                    GetEquipStatusS2C(role.RoleID);
                    break;
                default:
                    break;
            }
        }
        #region Redis模块
        /// <summary>
        /// 获取人物装备数据及装备位置信息
        /// </summary>
        /// <param name="roleid"></param>
        void GetEquipStatusS2C(int roleid)
        {
            var roleweaponExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleWeaponPostfix, roleid.ToString()).Result;
            var roleequipExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleEquipmentPerfix, roleid.ToString()).Result;
            if (roleweaponExist && roleequipExist)
            {
                var roleweapon = RedisHelper.Hash.HashGetAsync<RoleWeaponDTO>(RedisKeyDefine._RoleWeaponPostfix, roleid.ToString()).Result;
                var roleequip = RedisHelper.Hash.HashGetAsync<RoleEquipmentDTO>(RedisKeyDefine._RoleEquipmentPerfix, roleid.ToString()).Result;
                if (roleequip != null && roleweapon != null)
                {
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.RoleEquipment, roleequip);
                    dict.Add((byte)ParameterCode.GetWeapon, roleweapon);
                    EquipmentSuccessS2C(roleid, EquipmentOpCode.GetEquipStatus, dict);
                }
                else
                    GetEquipStatusMySql(roleid);
            }
            else
                GetEquipStatusMySql(roleid);
        }


        #endregion
        #region MySql模块
        void GetEquipStatusMySql(int roleid)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var roleweapon = NHibernateQuerier.CriteriaSelect<RoleWeapon>(nHCriteria);
            var roleequip = NHibernateQuerier.CriteriaSelect<RoleEquipment>(nHCriteria);
            if (roleweapon!=null&& roleequip!=null)
            {
                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.RoleEquipment, ChangeDataType(roleequip) );
                dict.Add((byte)ParameterCode.GetWeapon, ChangeDataType(roleweapon) );
                EquipmentSuccessS2C(roleid, EquipmentOpCode.GetEquipStatus, dict);
            }
        }
        #endregion

        void EquipmentSuccessS2C(int roleID, EquipmentOpCode oPcode, object data)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncEquipment;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.Success;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);

            Utility.Debug.LogInfo("角色副职业数据发送了" + Utility.Json.ToJson(data));
        }

        void EquipmentFailS2C(int roleID, EquipmentOpCode oPcode)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncSecondaryJob;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.ItemNotFound;
            opData.DataMessage = Utility.Json.ToJson(null);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }

        RoleWeaponDTO ChangeDataType(RoleWeapon roleWeapon)
        {
            RoleWeaponDTO weapon = new RoleWeaponDTO();
            weapon.RoleID = roleWeapon.RoleID;
            weapon.Weaponindex = Utility.Json.ToObject<Dictionary<int,int>>(roleWeapon.Weaponindex);
            weapon.WeaponStatusDict = Utility.Json.ToObject<Dictionary<int, WeaponDTO>>(roleWeapon.WeaponStatusDict);
            weapon.Magicindex = Utility.Json.ToObject<Dictionary<int, int>>(roleWeapon.Magicindex);
            weapon.MagicStatusDict = Utility.Json.ToObject<Dictionary<int, WeaponDTO>>(roleWeapon.MagicStatusDict);
            return weapon;
        }

        RoleEquipmentDTO ChangeDataType(RoleEquipment equipment)
        {
            RoleEquipmentDTO equipmentDTO = new RoleEquipmentDTO();
            equipmentDTO.RoleID = equipment.RoleID;
            equipmentDTO.MagicWeapon = Utility.Json.ToObject<Dictionary<int,int>>(equipment.MagicWeapon);
            equipmentDTO.Weapon = Utility.Json.ToObject<Dictionary<int, int>>(equipment.Weapon);
            return equipmentDTO;
        }
    }
}
