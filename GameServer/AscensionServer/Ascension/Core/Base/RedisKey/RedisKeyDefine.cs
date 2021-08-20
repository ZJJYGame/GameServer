using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    /// <summary>
    /// Postfix(后缀);
    /// Perfix(前缀)； 
    /// </summary>
    public class RedisKeyDefine
    {
        /// <summary>
        /// 类历练技能槽按钮持久化key前缀；
        /// </summary>
        public static readonly string _SkillLayoutPerfix = "SkillLayout";
        /// <summary>
        /// 功法的key前缀
        /// </summary>
        public static readonly string _CultivatiionMethodPerfix = "Gongfa";
        /// <summary>
        /// 角色的Ke前缀
        /// </summary>
        public static readonly string _RolePostfix = "Role";
        /// <summary>
        /// 秘术的Key前缀
        /// </summary>
        public static readonly string _MiShuPerfix = "MiShu";
        /// <summary>
        /// 角色秘术的Key前缀
        /// </summary>
        public static readonly string _RoleMiShuPerfix = "RoleMiShu";
        /// <summary>
        /// 宠物的Key前缀
        /// </summary>
        public static readonly string _PetPerfix = "Pet";
        /// <summary>
        /// 宠物属性的Key前缀
        /// </summary>
        public static readonly string _PetStatusPerfix = "PetStatus";
        /// <summary>
        /// 宠物资质的Key前缀
        /// </summary>
        public static readonly string _PetAptitudePerfix = "PetAptitude";
        /// <summary>
        /// 宠物加点的Key前缀
        /// </summary>
        public static readonly string _PetAbilityPointPerfix = "PetAbilityPoint";
        /// <summary>
        /// 傀儡的Key前缀
        /// </summary>
        public static readonly string _PuppetPerfix = "Puppet";
        /// <summary>
        /// 角色拥有的傀儡的Key前缀
        /// </summary>
        public static readonly string _RolePuppetPerfix = "RolePuppet";
        /// <summary>
        /// 傀儡个体拥有的属性值的Key前缀
        /// </summary>
        public static readonly string _PuppetIndividualPerfix = "PuppetIndividual";
        /// <summary>
        /// 炼丹的Key前缀
        /// </summary>
        public static readonly string _AlchemyPerfix = "Alchemy";
        /// <summary>
        ///锻造的Key前缀
        /// </summary>
        public static readonly string _ForgePerfix = "Forge";
        /// <summary>
        ///阵法的Key前缀
        /// </summary>
        public static readonly string _TacticFormationPerfix = "TacticFormation";
        /// <summary>
        ///制符的Key前缀
        /// </summary>
        public static readonly string _RunesPerfix = "Runes";
        /// <summary>
        ///灵田的Key前缀
        /// </summary>
        public static readonly string _FieldPerfix = "Field";
        /// <summary>
        ///商店的Key前缀
        /// </summary>
        public static readonly string _ShopPerfix = "Shop";
        /// <summary>
        ///杂货铺的Key前缀
        /// </summary>
        public static readonly string _VareityShopPerfix = "VareityShop";
        /// <summary>
        ///仙盟列表的Key前缀
        /// </summary>
        public static readonly string _AllianceListPerfix = "AllianceList";
        /// <summary>
        ///仙盟签到的Key前缀
        /// </summary>
        public static readonly string _AllianceSigninPerfix = "AllianceSignin";
        /// <summary>
        ///仙盟签到的Key前缀
        /// </summary>
        public static readonly string _RefreshAllianceSigninPerfix = "RefreshAllianceSignin";
        /// <summary>
        ///仙盟的Key前缀
        /// </summary>
        public static readonly string _AlliancePerfix = "Alliance";
        /// <summary>
        ///个人仙盟数据的Key前缀
        /// </summary>
        public static readonly string _RoleAlliancePerfix = "RoleAlliance";
        /// <summary>
        ///仙盟建设的Key前缀
        /// </summary>
        public static readonly string _AllianceConstructionPerfix = "AllianceConstruction";
        /// <summary>
        ///仙盟设置的兑换Key前缀
        /// </summary>
        public static readonly string _AllianceExchangeGoodsPerfix = "AllianceExchangeGoods";
        /// <summary>
        ///角色仙盟技能的Key前缀
        /// </summary>
        public static readonly string _RoleAllianceSkillPerfix = "RoleAllianceSkill";
        /// <summary>
        ///仙盟成员的Key前缀
        /// </summary>
        public static readonly string _AllianceMemberPerfix = "AllianceMember";
        /// <summary>
        ///仙盟内广播的消息内容的Key前缀
        /// </summary>
        public static readonly string _DailyMessagePerfix = "DailyMessage";
        /// <summary>
        /// 阵法记录删除回调的key前缀
        /// </summary>
        public static readonly string _DeldteTacticalPerfix = "Tactical";
        /// <summary>
        /// 角色属性值的key前缀
        /// </summary>
        public static readonly string _RoleStatsuPerfix = "RoleStatus";
        /// <summary>
        /// 角色功法的key前缀
        /// </summary>
        public static readonly string _RoleGongfaPerfix = "RoleGongfa";
        /// <summary>
        /// 功法的key前缀
        /// </summary>
        public static readonly string _GongfaPerfix = "Gongfa";
        /// <summary>
        /// 角色所有宠物的key前缀
        /// </summary>
        public static readonly string _RolePetPerfix = "RolePet";
        /// <summary>
        /// 角色所有宠物的key前缀
        /// </summary>
        public static readonly string _RoleAssetsPerfix = "RoleAssets";
        /// <summary>
        /// 傀儡部件的属性的key前缀
        /// </summary>
        public static readonly string _PuppetUnitPerfix = "PuppetUnit";
        /// <summary>
        /// 角色所有飞行法器的key前缀
        /// </summary>
        public static readonly string _RoleFlyMagicToolPerfix = "RoleFlyMagicTool";
        /// <summary>
        /// 历练地图资源后缀；
        /// </summary>
        public static readonly string _WildMapResPerfix = "WildMapResource";
        /// <summary>
        /// 秘境地图资源前缀
        /// </summary>
        public static readonly string _SecretMapResPerfix = "SecretMapReseource";
        /// <summary>
        /// 宠物使用丹药每日刷新
        /// </summary>
        public static readonly string _PetDrugRefreshPostfix = "PetDrugRefresh";
        /// <summary>
        /// 副职业炼丹
        /// </summary>
        public static readonly string _AlchemyPostfix = "Alchemy";
        /// <summary>
        /// 角色修炼功法秘书记录
        /// </summary>
        public static readonly string _RoleOnOffLinePostfix = "RoleOnOffLine";
        /// <summary>
        /// 角色瓶颈触发
        /// </summary>
        public static readonly string _RoleBottleneckPostfix = "RoleBottleneck";
        /// <summary>
        /// 角色属性加点
        /// </summary>
        public static readonly string _RoleAbilityPointPostfix = "RoleAbilityPoint";
        /// <summary>
        /// 宗門所有洞府信息
        /// </summary>
        public static readonly string _AllianceDongFuPostfix = "AllianceDongFu";
        /// <summary>
        /// 获取人物装备法宝信息
        /// </summary>
        public static readonly string _RoleWeaponPostfix = "RoleWeapon";
        /// <summary>
        /// 角色装备的装备的Key前缀
        /// </summary>
        public static readonly string _RoleEquipmentPerfix = "RoleEquipment";
        /// <summary>
        /// 宗门解散的Key前缀
        /// </summary>
        public static readonly string _DissolveAlliancePerfix = "DissolveAlliance";

        #region 角色属性高速存储用
        /// <summary>
        /// 角色功法部分属性记录
        /// </summary>
        public static readonly string _RoleStatusGFPerfix = "RoleStatusGF";
        /// <summary>
        /// 角色秘术部分属性记录
        /// </summary>
        public static readonly string _RoleStatusMSPerfix = "RoleStatusMS";
        /// <summary>
        /// 角色加点部分属性记录
        /// </summary>
        public static readonly string _RoleStatusAddPointPerfix = "RoleStatusAddPoint";
        /// <summary>
        /// 角色装备部分属性记录
        /// </summary>
        public static readonly string _RoleStatusEquipPerfix = "RoleStatusEquip";
        /// <summary>
        /// 角色飛行法器部分属性记录
        /// </summary>
        public static readonly string _RoleStatusFlyPerfix = "RoleStatusFly";
        #endregion
    }
}


