using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisDotNet;
namespace AscensionServer
{
    [Implementer]
    public class DefaultCreateRole : ICreateRoleHelper
    {
        Dictionary<int, int> RoleGFDict = new Dictionary<int, int>();
        //Dictionary<int, int> RoleMiShuDict = new Dictionary<int, int>();
        Dictionary<int, int> RolePetDict = new Dictionary<int, int>();
        Dictionary<string, RoleTaskItemDTO> roleTaskDic = new Dictionary<string, RoleTaskItemDTO>();
        Dictionary<int, RingItemsDTO> ringDict = new Dictionary<int, RingItemsDTO>();
        Dictionary<int, int> magicRingDict = new Dictionary<int, int>();
        public OperationData CreateRole(Dictionary<byte, object> dataMessage)
        {
            var opData = new OperationData();
            //{
            //    OperationCode = (byte)OperationCode.LoginArea,
            //    SubOperationCode = (short)LoginAreaOpCode.CreateRole,
            //};

            var messageDict = new Dictionary<byte, object>();

            var dict = dataMessage;
            string roleJsonTmp = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Role));
            Role roleTmp = Utility.Json.ToObject<Role>(roleJsonTmp);
            NHCriteria nHCriteriaRoleName =ReferencePool.Accquire<NHCriteria>().SetValue("RoleName", roleTmp.RoleName);
            var isExisted = NHibernateQuerier.Verify<Role>(nHCriteriaRoleName);
            if (isExisted)
                Utility.Debug.LogInfo("----------------------------  Role >>Role name:+" + roleTmp.RoleName + " already exist !!!  ---------------------------------");
            Role role = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriaRoleName);//根据username查询数据
                                                                                   //TODO AddRoleSubHandler查询uuid未处理
            string userJsonTmp = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.User));
            User userTmp = Utility.Json.ToObject<User>(userJsonTmp);
            string str_uuid = userTmp.UUID;
            NHCriteria nHCriteriaUUID =ReferencePool.Accquire<NHCriteria>().SetValue("UUID", str_uuid);
            var userRole = NHibernateQuerier.CriteriaSelect<UserRole>(nHCriteriaUUID);
            string roleJson = userRole.RoleIDArray;
            string roleStatusJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleStatus));

            //Dictionary<int, int> idRing = new Dictionary<int, int>();
            Dictionary<int, int> initialSchool = new Dictionary<int, int>();
            GameEntry.DataManager.TryGetValue<Dictionary<int, RoleStatusDatas>>(out var roleStatusDict);
            Ring ring = null;
            //如果没有查询到代表角色没被注册过可用
            if (role == null)
            {
                List<string> roleList = new List<string>();
                if (!string.IsNullOrEmpty(roleJson))
                    roleList = Utility.Json.ToObject<List<string>>(roleJson);
                //添加输入的用户进数据库
                role = roleTmp;
                RoleStatus rolestatus = new RoleStatus()
                {
                    FreeAttributes = roleStatusDict[0].AttackPhysical,
                    RoleHP = roleStatusDict[0].RoleHP,
                    RoleMaxHP = roleStatusDict[0].RoleHP,
                    RoleMP = roleStatusDict[0].RoleMP,
                    RoleMaxMP = roleStatusDict[0].RoleMP,
                    RoleSoul = roleStatusDict[0].RoleSoul,
                    RoleMaxSoul = roleStatusDict[0].RoleSoul,
                    BestBlood = (short)roleStatusDict[0].BestBlood,
                    BestBloodMax = (short)roleStatusDict[0].BestBlood,
                    AttackSpeed = roleStatusDict[0].AttackSpeed,
                    AttackPhysical = roleStatusDict[0].AttackPhysical,
                    DefendPhysical = roleStatusDict[0].DefendPhysical,
                    AttackPower = roleStatusDict[0].AttackPower,
                    DefendPower = roleStatusDict[0].DefendPower,
                    PhysicalCritProb = roleStatusDict[0].PhysicalCritProb,
                    MagicCritProb = roleStatusDict[0].MagicCritProb,
                    ReduceCritProb = roleStatusDict[0].ReduceCritProb,
                    PhysicalCritDamage = roleStatusDict[0].PhysicalCritDamage,
                    MagicCritDamage = roleStatusDict[0].MagicCritDamage,
                    ReduceCritDamage = roleStatusDict[0].ReduceCritDamage,
                    MoveSpeed = roleStatusDict[0].MoveSpeed,
                    RolePopularity = roleStatusDict[0].RolePopularity,
                    RoleMaxPopularity = roleStatusDict[0].RolePopularity,
                    ValueHide = roleStatusDict[0].ValueHide,
                    GongfaLearnSpeed = roleStatusDict[0].GongfaLearnSpeed,
                    MishuLearnSpeed = roleStatusDict[0].MishuLearnSpeed,
                    Vitality = roleStatusDict[0].Vitality,
                    MaxVitality = roleStatusDict[0].Vitality
                };
                role = NHibernateQuerier.Insert<Role>(role);
                string roleId = role.RoleID.ToString();
                if (!string.IsNullOrEmpty(roleJson))
                    roleList.Add(roleId);
                else
                    roleList.Add(roleId);
                rolestatus.RoleID = int.Parse(roleId);
                 RedisHelper.Hash.HashSet(RedisKeyDefine._RolePostfix,roleId.ToString(), role);
                NHibernateQuerier.Insert(rolestatus);
                RedisHelper.Hash.HashSet(RedisKeyDefine._RoleStatsuPerfix, rolestatus.RoleID.ToString(), rolestatus);
                NHibernateQuerier.Insert(new RoleAssets() { RoleID = rolestatus.RoleID });
                RedisHelper.Hash.HashSet(RedisKeyDefine._RoleAssetsPerfix, rolestatus.RoleID.ToString(), new RoleAssetsDTO() { RoleID = rolestatus.RoleID });
                NHibernateQuerier.Insert(new OnOffLine() { RoleID = rolestatus.RoleID });
                NHibernateQuerier.Insert(new Bottleneck() { RoleID = rolestatus.RoleID });
                #region 任务
                roleTaskDic.Clear();
                roleTaskDic.Add("1001001", new RoleTaskItemDTO() { RoleTaskType = "DialogSystem", RoleTaskAchieveState = "NoAchieveTask", RoleTaskAcceptState = "NoAcceptAbleTask", RoleTaskAbandonState = "NoAbandonTask", RoleTaskKind = "MainTask" });
                NHibernateQuerier.Insert(new RoleTaskProgress() { RoleID = rolestatus.RoleID, RoleTaskInfoDic = Utility.Json.ToJson(roleTaskDic) });
                #endregion
                Dictionary<string, string> DOdict = new Dictionary<string, string>();
                #region 测试待修改
                RoleGFDict.Clear();
                //CultivationMethod gongFa = new CultivationMethod();
                //gongFa = ConcurrentSingleton<NHManager>.Instance.Insert(gongFa);
                //RoleGFDict.Add(gongFa.ID, gongFa.CultivationMethodID);
                NHibernateQuerier.Insert(new RoleGongFa() { RoleID = rolestatus.RoleID, GongFaIDDict = Utility.Json.ToJson(new Dictionary<string, string>()) });

                RoleGongFaDTO roleGongFaDTO = new RoleGongFaDTO();
                roleGongFaDTO.RoleID = rolestatus.RoleID;
                RedisHelper.Hash.HashSet(RedisKeyDefine._RoleGongfaPerfix, rolestatus.RoleID.ToString(), roleGongFaDTO);


                NHibernateQuerier.Insert(new RoleMiShu() { RoleID = rolestatus.RoleID });


                RoleMiShuDTO roleMiShu = new RoleMiShuDTO();
                roleMiShu.RoleID = rolestatus.RoleID;
                RedisHelper.Hash.HashSet(RedisKeyDefine._RoleMiShuPerfix, rolestatus.RoleID.ToString(), roleMiShu);

                RolePetDict.Clear();
                //Pet pet = new Pet() {};
                //pet = ConcurrentSingleton<NHManager>.Instance.Insert(pet);
                //PetStatus petStatus = new PetStatus() { PetID = pet.ID };
                //ConcurrentSingleton<NHManager>.Instance.Insert(petStatus);
                //PetaPtitude petaPtitude=new PetaPtitude() { PetID = pet.ID,PetaptitudeDrug=Utility.Json.ToJson(new Dictionary<int, int>()) };
                //petaPtitude = ConcurrentSingleton<NHManager>.Instance.Insert(petaPtitude);
                //RolePetDict.Add(pet.ID, pet.PetID);
                NHibernateQuerier.Insert(new RolePet() { RoleID = rolestatus.RoleID, PetIDDict = "{}" });
                RolePurchaseRecord rolePurchaseRecord = new RolePurchaseRecord() { RoleID = rolestatus.RoleID, GoodsPurchasedCount = Utility.Json.ToJson(new Dictionary<int, int>()) };
                NHibernateQuerier.Insert(rolePurchaseRecord);
                RoleWeapon weapon = new RoleWeapon() { RoleID = rolestatus.RoleID, Weaponindex = Utility.Json.ToJson(new Dictionary<int, int>()), WeaponStatusDict = Utility.Json.ToJson(new Dictionary<int, WeaponDTO>()), Magicindex = Utility.Json.ToJson(new Dictionary<int, int>()), MagicStatusDict = Utility.Json.ToJson(new Dictionary<int, WeaponDTO>()) };
                NHibernateQuerier.Insert(weapon);
                #endregion
                #region 背包
                NHibernateQuerier.Insert(new RoleRing() { RoleID = rolestatus.RoleID });
                NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolestatus.RoleID);
                var ringArray = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteriaRoleID);
                if (ringArray.RingIdArray == 0)
                {
                    ringDict.Clear();
                    magicRingDict.Clear();
                    ringDict.Add(17701, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 1 });
                    ringDict.Add(17711, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 2 });
                    ringDict.Add(17716, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 3 });
                    ringDict.Add(17721, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 4 });
                    ringDict.Add(17952, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15001, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15002, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15003, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15004, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15005, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15006, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15007, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15008, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15009, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(15010, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(17001, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(17016, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    ringDict.Add(17006, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 1, RingItemType = 0 });
                    //ringDict.Add(14301, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =20 });
                    //ringDict.Add(14302, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =20 });
                    //ringDict.Add(14303, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =20 });
                    //ringDict.Add(14304, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =20 });
                    //ringDict.Add(14305, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =20 });
                    //ringDict.Add(16001, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =50 });
                    //ringDict.Add(16002, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16003, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16004, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16005, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16006, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16007, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16008, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16009, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(16010, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 50 });
                    //ringDict.Add(17955, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(17985, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(13021, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(13022, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(13023, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(13024, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(13025, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =1 });
                    //ringDict.Add(14101, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax =10 });
                    //ringDict.Add(14102, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14103, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14104, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14105, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14106, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14107, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14108, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 10 });
                    //ringDict.Add(14001, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 99 });
                    //ringDict.Add(14002, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 99 });
                    //ringDict.Add(14003, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 99 });
                    //ringDict.Add(14004, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 99 });
                    //ringDict.Add(14005, new RingItemsDTO() { RingItemAdorn = "0", RingItemCount = 1, RingItemTime = DateTime.Now.ToString("yyyyMMddHHmmss"), RingItemMax = 99 });
                    for (int i = 0; i < 6; i++)
                    { magicRingDict.Add(i, -1); }
                    ring = NHibernateQuerier.Insert<Ring>(new Ring() { RingId = 11110, RingItems = Utility.Json.ToJson(ringDict), RingMagicDictServer = Utility.Json.ToJson(magicRingDict), RingAdorn = Utility.Json.ToJson(new Dictionary<int, RingItemsDTO>()) });
                    //idRing.Add(ring.ID, 0);
                    NHibernateQuerier.Update<RoleRing>(new RoleRing() { RoleID = rolestatus.RoleID, RingIdArray = ring.ID });
                }

                #endregion
                #region 临时背包
                NHibernateQuerier.Insert(new TemporaryRing() { RoleID = rolestatus.RoleID, RingItems = Utility.Json.ToJson(new Dictionary<int, RingItemsDTO>()) });
                #endregion
                #region 副职业
                NHibernateQuerier.Insert<Alchemy>(new Alchemy() { RoleID = rolestatus.RoleID, Recipe_Array = Utility.Json.ToJson(new List<int>()) });
                NHibernateQuerier.Insert<HerbsField>(new HerbsField() { RoleID = rolestatus.RoleID, jobLevel = 0, AllHerbs = Utility.Json.ToJson(new List<HerbFieldStatus>()) });
                NHibernateQuerier.Insert<Forge>(new Forge() { RoleID = rolestatus.RoleID, Recipe_Array = Utility.Json.ToJson(new List<int>()) });
                NHibernateQuerier.Insert<SpiritualRunes>(new SpiritualRunes() { RoleID = rolestatus.RoleID, Recipe_Array = Utility.Json.ToJson(new List<int>()) });
                NHibernateQuerier.Insert<Puppet>(new Puppet() { RoleID = rolestatus.RoleID, Recipe_Array = Utility.Json.ToJson(new List<int>()) });
                RedisHelper.Hash.HashSet(RedisKeyDefine._PuppetPerfix, rolestatus.RoleID.ToString(), new PuppetDTO() { RoleID = rolestatus.RoleID });
                NHibernateQuerier.Insert<TacticFormation>(new TacticFormation() { RoleID = rolestatus.RoleID, Recipe_Array = Utility.Json.ToJson(new List<int>()) });
                NHibernateQuerier.SaveOrUpdate(new RoleWeapon() { RoleID = rolestatus.RoleID});
                RedisHelper.Hash.HashSet(RedisKeyDefine._RoleWeaponPostfix, rolestatus.RoleID.ToString(),new RoleWeaponDTO());
                NHibernateQuerier.SaveOrUpdate(new PuppetUnit() { RoleID = rolestatus.RoleID });
                RedisHelper.Hash.HashSet(RedisKeyDefine._PuppetUnitPerfix, rolestatus.RoleID.ToString(), new PuppetUnitDTO() { RoleID = rolestatus.RoleID });
                Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>添加副职业成功");
                #endregion
                #region 仙盟
                RoleAlliance roleAlliance = new RoleAlliance() { RoleID = rolestatus.RoleID, RoleName = role.RoleName, ApplyForAlliance = Utility.Json.ToJson(new List<int>()), RoleSchool = 900 };
                NHibernateQuerier.Insert(roleAlliance);
                RoleAllianceSkill roleAllianceSkill = new RoleAllianceSkill() { };
                roleAllianceSkill.RoleID = rolestatus.RoleID;
                NHibernateQuerier.Insert(roleAllianceSkill);
                #endregion
                NHibernateQuerier.Insert(new RolePuppet() { RoleID = role.RoleID });
                RedisHelper.Hash.HashSet(RedisKeyDefine._RolePuppetPerfix, rolestatus.RoleID.ToString(), new RolePuppetDTO() { RoleID = role.RoleID });

                NHibernateQuerier.Insert(new RoleEquipment() { RoleID = role.RoleID });
                RedisHelper.Hash.HashSet(RedisKeyDefine._RoleEquipmentPerfix, rolestatus.RoleID.ToString(), new RoleEquipmentDTO() { RoleID = role.RoleID });

                NHibernateQuerier.Insert(new FlyMagicTool() { RoleID = role.RoleID, AllFlyMagicTool = Utility.Json.ToJson(new List<int>() { 23401, 23402 }) });
                RoleStatusPointDTO roleStatusPointDTO = new RoleStatusPointDTO();
                NHibernateQuerier.Insert(new RoleStatusPoint() { RoleID = role.RoleID,AbilityPointSln= Utility.Json.ToJson(roleStatusPointDTO.AbilityPointSln) });
                Utility.Debug.LogInfo("yzqData添加新角色" + role.RoleID);
                NHibernateQuerier.Insert(new DemonicSoul() { RoleID = role.RoleID });
                Utility.Debug.LogInfo("yzqData添加新角色" + userRole.UUID);
                var userRoleJson = Utility.Json.ToJson(roleList);
                NHibernateQuerier.Update(new UserRole() { RoleIDArray = userRoleJson, UUID = str_uuid });
                opData.ReturnCode = (short)ReturnCode.Success;
                RoleAllianceDTO roleAllianceDTO = new RoleAllianceDTO() { RoleID = rolestatus.RoleID, RoleName = role.RoleName, ApplyForAlliance = new List<int>() };
                DOdict.Add("Role", Utility.Json.ToJson(role));
                DOdict.Add("RoleStatus", Utility.Json.ToJson(rolestatus));
                //DOdict.Add("GongFa", Utility.Json.ToJson(gongFa));
                //MiShuDTO miShuDTO = new MiShuDTO() { ID = miShu.ID, MiShuID = miShu.MiShuID, MiShuSkillArry = Utility.Json.ToObject<List<int>>(miShu.MiShuSkillArry) };
                //DOdict.Add("MiShu", Utility.Json.ToJson(miShuDTO));
                DOdict.Add("RoleAlliance", Utility.Json.ToJson(roleAllianceDTO));
                messageDict.Add((byte)ParameterCode.Role, Utility.Json.ToJson(DOdict));
                opData.DataMessage = Utility.Json.ToJson(messageDict);
            }
            else
            {
                opData.ReturnCode = (short)ReturnCode.Fail;
            }
            //把上面的回应给客户端
            ReferencePool.Release(nHCriteriaUUID, nHCriteriaRoleName);
            return opData;
        }
        public void Release()
        {
            RoleGFDict.Clear();
           // RoleMiShuDict.Clear();
            RolePetDict.Clear();
            roleTaskDic.Clear();
            ringDict.Clear();
            magicRingDict.Clear();
        }
    }
}
