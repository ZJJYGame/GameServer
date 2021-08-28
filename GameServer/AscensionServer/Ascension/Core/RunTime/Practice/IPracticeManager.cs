using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionProtocol;
using AscensionServer.Model;
namespace AscensionServer
{
    public interface IPracticeManager: IModuleManager
    {
        /// <summary>
        /// 计算人物属性
        /// </summary>
        /// <param name="pointDTO"></param>
        /// <param name="roleStatus"></param>
        /// <returns></returns>
        Task<RoleStatus> RoleAblility(RoleStatusPointDTO pointDTO, RoleStatus roleStatus);
        /// <summary>
        /// 计算人物装备
        /// </summary>
        /// <param name="statusObj"></param>
        /// <param name="roleWeapon"></param>
        /// <param name="roleEquipment"></param>
        /// <returns></returns>
        Task<RoleStatus> RoleEquip(RoleStatus statusObj, RoleWeaponDTO roleWeapon, RoleEquipmentDTO roleEquipment);
        /// <summary>
        /// 计算飞行法器
        /// </summary>
        /// <param name="flyMagic"></param>
        /// <param name="roleStatus"></param>
        /// <returns></returns>
        Task<RoleStatus> RoleFlyMagicTool(FlyMagicToolDTO flyMagic, RoleStatus roleStatus);
        /// <summary>
        /// 切换加点方案
        /// </summary>
        /// <param name="pointDTO"></param>
        /// <param name="roleStatus"></param>
        /// <returns></returns>
        Task<RoleStatus> RoleSwitchAblility(RoleStatusPointDTO pointDTO, RoleStatus roleStatus);
        /// <summary>
        /// 切换飞行法器
        /// </summary>
        /// <param name="flyMagic"></param>
        /// <param name="roleStatus"></param>
        /// <returns></returns>
        Task<RoleStatus> RoleSwitchFlyMagicTool(FlyMagicToolDTO flyMagic, RoleStatus roleStatus);

        RoleStatusDTO RoleStatusAlgorithm(int roleid, RoleStatus statusFly = null, RoleStatusAdditionDTO statusGF = null, RoleStatusAdditionDTO statusMS = null, RoleStatus statusPoint = null, RoleStatusAdditionDTO statusEquip = null, RoleAllianceSkill roleAllianceSkill = null, int rolelevel = 0);
    }
}
