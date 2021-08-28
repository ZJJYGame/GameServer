using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using AscensionProtocol;
using AscensionServer.Model;
using RedisDotNet;
using Cosmos;
namespace AscensionServer
{
    public partial class PetStatusManager
    {
        /// <summary>
        /// 宠物设置出战
        /// </summary>
        /// <param name="rolePetDTO"></param>
        /// <param name="rolePet"></param>
        public async void RolePetSetBattle(RolePetDTO rolePetDTO)
        {
            NHCriteria nHCriteriaRolePet =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolePetDTO.RoleID);
            NHCriteria nHCriteriaPet =ReferencePool.Accquire<NHCriteria>().SetValue("ID", rolePetDTO.PetIsBattle);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriaPet);
            var rolePet = NHibernateQuerier.CriteriaSelect<RolePet>(nHCriteriaRolePet);
            var role = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriaRolePet);
            if (pet!=null&& rolePet !=null&& role!=null)
            {
                if (role.RoleLevel >= pet.PetLevel)
                {
                    Utility.Debug.LogInfo("yzqData对角色宠物的操作" + rolePet.PetIsBattle + ".,.,.,.,.><<><><><><<<<>" + rolePetDTO.PetIsBattle);
                    if (rolePet.PetIsBattle == rolePetDTO.PetIsBattle)
                    {
                        rolePet.PetIsBattle = 0;
                        rolePetDTO.PetIsBattle = 0;
                    }
                    else
                        rolePet.PetIsBattle = rolePetDTO.PetIsBattle;
                    await NHibernateQuerier.UpdateAsync<RolePet>(rolePet);
                    rolePetDTO.PetIDDict = Utility.Json.ToObject<Dictionary<int, int>>(rolePet.PetIDDict);
                    rolePetDTO.RoleID = rolePet.RoleID;
                    await RedisHelper.Hash.HashSetAsync<RolePetDTO>(RedisKeyDefine._RolePetPerfix, rolePetDTO.RoleID.ToString(), rolePetDTO);
                    ResultSuccseS2C(rolePet.RoleID,RolePetOpCode.SetBattle, rolePetDTO);
                }
                else
                    ResultFailS2C(rolePet.RoleID, RolePetOpCode.SetBattle);
            }
            else
                ResultFailS2C(rolePet.RoleID, RolePetOpCode.SetBattle);

        }
        /// <summary>
        /// 获得角色所有宠物
        /// </summary>
        /// <param name="rolePet"></param>
        public async void GetRoleAllPetS2C( RolePetDTO rolePetDTO)
        {
            NHCriteria nHCriteriaRolePet =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolePetDTO.RoleID);

            var rolePet = NHibernateQuerier.CriteriaSelect<RolePet>(nHCriteriaRolePet);
            var petDict = Utility.Json.ToObject<Dictionary<int, int>>(rolePet.PetIDDict);
            List<NHCriteria> nHCriteriaList = new List<NHCriteria>();
            Dictionary<int, PetDTO> allPetDict = new Dictionary<int, PetDTO>();
            rolePetDTO.PetIsBattle = rolePet.PetIsBattle;
            rolePetDTO.PetIDDict = Utility.Json.ToObject<Dictionary<int, int>>(rolePet.PetIDDict);

            foreach (var item in petDict)
            {
                if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetPerfix, item.Key.ToString()).Result)
                {
                    var petDTOTemp =  RedisHelper.Hash.HashGetAsync<PetDTO>(RedisKeyDefine._PetPerfix, item.Key.ToString()).Result;
                    allPetDict.Add(petDTOTemp.ID, petDTOTemp);
                    Utility.Debug.LogInfo("YZQ發送角色所有寵物列表" + item.Key.ToString()+""+Utility.Json.ToJson(petDTOTemp));
                }
                else
                {
                    NHCriteria nHCriteriaPet =ReferencePool.Accquire<NHCriteria>().SetValue("ID", item.Key);
                    nHCriteriaList.Add(nHCriteriaPet);
                    var petTemp = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriaPet);
                    var petDtoTemp =ReferencePool.Accquire<PetDTO>();

                    petDtoTemp.ID = petTemp.ID;
                    petDtoTemp.PetExp = petTemp.PetExp;
                    petDtoTemp.DemonicSoul = Utility.Json.ToObject<Dictionary<int, List<int>>>(petTemp.DemonicSoul);
                    petDtoTemp.PetID = petTemp.PetID;
                    petDtoTemp.PetLevel = petTemp.PetLevel;
                    petDtoTemp.PetName = petTemp.PetName;
                    Utility.Debug.LogInfo("YZQ技能列表"+Utility.Json.ToJson(petTemp.PetSkillArray));
                    try
                    {
                        petDtoTemp.PetSkillArray = Utility.Json.ToObject<List<int>>(petTemp.PetSkillArray);
                    }
                    catch (Exception)
                    {
                        petDtoTemp.PetSkillArray=null;
                    }
                    allPetDict.Add(petDtoTemp.ID, petDtoTemp);
                }
            }
            Dictionary<byte, object> rolepetsDict = new Dictionary<byte, object>();
            rolepetsDict.Add((byte)ParameterCode.RolePet, rolePetDTO);
            rolepetsDict.Add((byte)ParameterCode.Pet, allPetDict);
            ResultSuccseS2C(rolePet.RoleID, RolePetOpCode.GetRolePet, rolepetsDict);
        }
        /// <summary>
        /// 移除角色宠物
        /// </summary>
        /// <param name="rolePet"></param>
        /// <param name="rolePetDTO"></param>
        public async void RemoveRolePet(RolePetDTO rolePetDTO)
        {
            NHCriteria nHCriteriaRolePet =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolePetDTO.RoleID);
            var rolePet = NHibernateQuerier.CriteriaSelect<RolePet>(nHCriteriaRolePet);

            NHCriteria nHCriteriaRomePet =ReferencePool.Accquire<NHCriteria>().SetValue("ID", rolePetDTO.AddRemovePetID);
            NHCriteria nHCriteriaRome =ReferencePool.Accquire<NHCriteria>().SetValue("PetID", rolePetDTO.AddRemovePetID);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriaRomePet);
            var petabitily = NHibernateQuerier.CriteriaSelect<PetAbilityPoint>(nHCriteriaRomePet);
            var petaptutide = NHibernateQuerier.CriteriaSelect<PetAptitude>(nHCriteriaRome);
            var petstatus = NHibernateQuerier.CriteriaSelect<PetStatus>(nHCriteriaRome);
            var rolePetDict = Utility.Json.ToObject<Dictionary<int, int>>(rolePet.PetIDDict);
            if (rolePetDict.ContainsKey(pet.ID))
            {
                #region Redis逻辑
                rolePetDict.Remove(pet.ID);
                rolePet.PetIDDict = Utility.Json.ToJson(rolePetDict);
                rolePetDTO.PetIDDict = rolePetDict;
                rolePetDTO.PetIsBattle = rolePet.PetIsBattle;
                var result = RedisHelper.KeyExistsAsync(RedisKeyDefine._PetPerfix).Result;
                if (result)
                {
                    await RedisHelper.Hash.HashDeleteAsync(RedisKeyDefine._PetPerfix, pet.ID.ToString());
                    await RedisHelper.Hash.HashDeleteAsync(RedisKeyDefine._PetAbilityPointPerfix, pet.ID.ToString());
                    await RedisHelper.Hash.HashDeleteAsync(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString());
                    await RedisHelper.Hash.HashDeleteAsync(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString());
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RolePetPerfix, rolePetDTO.RoleID.ToString(), rolePetDTO);
                }
                #endregion
                await NHibernateQuerier.DeleteAsync(pet);
                await NHibernateQuerier.DeleteAsync(petabitily);
                await NHibernateQuerier.DeleteAsync(petaptutide);
                await NHibernateQuerier.DeleteAsync(petstatus);
                Utility.Debug.LogInfo("yzqData已经放生的寵物" + Utility.Json.ToJson(rolePet));
                await NHibernateQuerier.UpdateAsync(rolePet);
                ResultSuccseS2C(rolePet.RoleID,RolePetOpCode.RemovePet, rolePetDTO);

            } else
                ResultFailS2C(rolePet.RoleID, RolePetOpCode.RemovePet);

        }
        /// <summary>
        /// 获得宠物所有属性
        /// </summary>
        public void GetPetAllCompeleteStatus(int petid,  int roleid)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petid);
            NHCriteria nHCriteriapet =ReferencePool.Accquire<NHCriteria>().SetValue("PetID", petid);


            Dictionary<byte, object> dict = new Dictionary<byte, object>();
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetPerfix ,petid.ToString()).Result)
            {
                var petObj = RedisHelper.Hash.HashGet<PetDTO>(RedisKeyDefine._PetPerfix, petid.ToString());
                if (petObj != null)
                {
                    dict.Add((byte)ParameterCode.Pet, petObj);
                }
                else
                {
                    Pet pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteria);
                    petObj =ReferencePool.Accquire<PetDTO>();
                    petObj.ID = pet.ID;
                    petObj.PetExp = pet.PetExp;
                    petObj.DemonicSoul = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);
                    petObj.PetLevel = pet.PetLevel;
                    petObj.PetName = pet.PetName;
                    petObj.PetSkillArray = Utility.Json.ToObject<List<int>>(pet.PetSkillArray);
                    dict.Add((byte)ParameterCode.Pet, petObj);
                }
            }
            else
            {
                Pet pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteria);
                var petObj =ReferencePool.Accquire<PetDTO>();
                petObj.ID = pet.ID;
                petObj.PetExp = pet.PetExp;
                petObj.DemonicSoul = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);
                petObj.PetLevel = pet.PetLevel;
                petObj.PetName = pet.PetName;
                petObj.PetSkillArray = Utility.Json.ToObject<List<int>>(pet.PetSkillArray);
                dict.Add((byte)ParameterCode.Pet, petObj);
            }
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetAbilityPointPerfix , petid.ToString()).Result)
            {
                var petAbilityPointObj = RedisHelper.Hash.HashGet<PetAbilityPointDTO>(RedisKeyDefine._PetAbilityPointPerfix, petid.ToString());

                if (petAbilityPointObj != null)
                {
                    dict.Add((byte)ParameterCode.PetAbility, petAbilityPointObj);
                }
                else
                {
                    PetAbilityPoint petAbilityPoint = NHibernateQuerier.CriteriaSelect<PetAbilityPoint>(nHCriteria);
                    petAbilityPointObj =ReferencePool.Accquire<PetAbilityPointDTO>();
                    petAbilityPointObj.ID = petAbilityPoint.ID;
                    petAbilityPointObj.IsUnlockSlnThree = petAbilityPoint.IsUnlockSlnThree;
                    petAbilityPointObj.SlnNow = petAbilityPoint.SlnNow;
                    if (!string.IsNullOrEmpty(petAbilityPoint.AbilityPointSln))
                    {
                        petAbilityPointObj.AbilityPointSln = new Dictionary<int, AbilityDTO>();
                        petAbilityPointObj.AbilityPointSln = Utility.Json.ToObject<Dictionary<int, AbilityDTO>>(petAbilityPoint.AbilityPointSln);
                    }
                    dict.Add((byte)ParameterCode.PetAbility, petAbilityPointObj);
                }
            }
            else
            {
                PetAbilityPoint petAbilityPoint = NHibernateQuerier.CriteriaSelect<PetAbilityPoint>(nHCriteria);
                var petAbilityPointObj =ReferencePool.Accquire<PetAbilityPointDTO>();
                petAbilityPointObj.ID = petAbilityPoint.ID;
                petAbilityPointObj.IsUnlockSlnThree = petAbilityPoint.IsUnlockSlnThree;
                petAbilityPointObj.SlnNow = petAbilityPoint.SlnNow;
                if (!string.IsNullOrEmpty(petAbilityPoint.AbilityPointSln))
                {
                    petAbilityPointObj.AbilityPointSln = new Dictionary<int, AbilityDTO>();
                    petAbilityPointObj.AbilityPointSln = Utility.Json.ToObject<Dictionary<int, AbilityDTO>>(petAbilityPoint.AbilityPointSln);
                }
                dict.Add((byte)ParameterCode.PetAbility, petAbilityPointObj);

            }
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetAptitudePerfix , petid.ToString()).Result)
            {
                var petAptitudeObj = RedisHelper.Hash.HashGet<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, petid.ToString());
                if (petAptitudeObj != null)
                {
                    dict.Add((byte)ParameterCode.PetAptitude, petAptitudeObj);
                }
                else
                {
                    PetAptitude petAptitude = NHibernateQuerier.CriteriaSelect<PetAptitude>(nHCriteriapet);
                    petAptitudeObj =ReferencePool.Accquire<PetAptitudeDTO>();
                    petAptitudeObj.PetID = petAptitude.PetID;
                    petAptitudeObj.AttackphysicalAptitude = petAptitude.AttackphysicalAptitude;
                    petAptitudeObj.AttackpowerAptitude = petAptitude.AttackpowerAptitude;
                    petAptitudeObj.AttackspeedAptitude = petAptitude.AttackspeedAptitude;
                    petAptitudeObj.DefendphysicalAptitude = petAptitude.DefendphysicalAptitude;
                    petAptitudeObj.DefendpowerAptitude = petAptitude.DefendpowerAptitude;
                    petAptitudeObj.HPAptitude = petAptitude.HPAptitude;
                    petAptitudeObj.Petaptitudecol = petAptitude.Petaptitudecol;
                    if (!string.IsNullOrEmpty(petAptitude.PetAptitudeDrug))
                    {
                        petAptitudeObj.PetAptitudeDrug = new Dictionary<int, int>();
                        petAptitudeObj.PetAptitudeDrug = Utility.Json.ToObject<Dictionary<int, int>>(petAptitude.PetAptitudeDrug);
                    }
                    petAptitudeObj.SoulAptitude = petAptitude.SoulAptitude;
                    dict.Add((byte)ParameterCode.PetAptitude, petAptitudeObj);
                }
            }
            else
            {
                PetAptitude petAptitude = NHibernateQuerier.CriteriaSelect<PetAptitude>(nHCriteriapet);
                var petAptitudeObj =ReferencePool.Accquire<PetAptitudeDTO>();
                petAptitudeObj.PetID = petAptitude.PetID;
                petAptitudeObj.AttackphysicalAptitude = petAptitude.AttackphysicalAptitude;
                petAptitudeObj.AttackpowerAptitude = petAptitude.AttackpowerAptitude;
                petAptitudeObj.AttackspeedAptitude = petAptitude.AttackspeedAptitude;
                petAptitudeObj.DefendphysicalAptitude = petAptitude.DefendphysicalAptitude;
                petAptitudeObj.DefendpowerAptitude = petAptitude.DefendpowerAptitude;
                petAptitudeObj.HPAptitude = petAptitude.HPAptitude;
                petAptitudeObj.Petaptitudecol = petAptitude.Petaptitudecol;
                if (!string.IsNullOrEmpty(petAptitude.PetAptitudeDrug))
                {
                    petAptitudeObj.PetAptitudeDrug = new Dictionary<int, int>();
                    petAptitudeObj.PetAptitudeDrug = Utility.Json.ToObject<Dictionary<int, int>>(petAptitude.PetAptitudeDrug);
                }
                petAptitudeObj.SoulAptitude = petAptitude.SoulAptitude;
                dict.Add((byte)ParameterCode.PetAptitude, petAptitudeObj);
            }

            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetStatusPerfix , petid.ToString()).Result)
            {
                var petStatusObj = RedisHelper.Hash.HashGet<PetStatusDTO>(RedisKeyDefine._PetStatusPerfix, petid.ToString());
                if (petStatusObj!=null)
                {
                    dict.Add((byte)ParameterCode.PetStatus, petStatusObj);
                }else
                {
                    PetStatus petAptitude = NHibernateQuerier.CriteriaSelect<PetStatus>(nHCriteriapet);
                     petStatusObj =ReferencePool.Accquire<PetStatusDTO>();
                    var petAptitudeJson = Utility.Json.ToJson(petAptitude);
                    petStatusObj = Utility.Json.ToObject<PetStatusDTO>(petAptitudeJson);
                    dict.Add((byte)ParameterCode.PetStatus, petStatusObj);
                }
            }
            else
            {
                PetStatus petAptitude = NHibernateQuerier.CriteriaSelect<PetStatus>(nHCriteriapet);
                var petStatusObj =ReferencePool.Accquire<PetStatusDTO>();
                var petAptitudeJson = Utility.Json.ToJson(petAptitude);
                petStatusObj = Utility.Json.ToObject<PetStatusDTO>(petAptitudeJson);
                dict.Add((byte)ParameterCode.PetStatus, petStatusObj);
            }

            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetDrugRefreshPostfix, petid.ToString()).Result)
            {
                var petdrugfreshObj = RedisHelper.Hash.HashGet<PetDrugRefreshDTO>(RedisKeyDefine._PetDrugRefreshPostfix, petid.ToString());
                if (petdrugfreshObj != null)
                {
                    dict.Add((byte)ParameterCode.PetDrugFresh, petdrugfreshObj);
                }
            }

            ResultSuccseS2C(roleid,RolePetOpCode.GetPetStatus,dict);

        }

        #region 使用丹药
        /// <summary>
        /// 宠物培养丹药区分
        /// </summary>
        /// <param name="drugID"></param>
        /// <param name="nHCriteria"></param>
        /// <param name="pet"></param>
        public void PetCultivate(int drugID, int petid, int roleid)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);

            NHCriteria nHCriteriapetStatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petid);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriapetStatus);

            var ringObj =ReferencePool.Accquire<RingDTO>();
            ringObj.RingItems = new Dictionary<int, RingItemsDTO>();
            ringObj.RingItems.Add(drugID, new RingItemsDTO());
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var nHCriteriaRingID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);
            Utility.Debug.LogError("收到宠物使用丹药1");
            if (InventoryManager.VerifyIsExist(drugID, nHCriteriaRingID))
            {
                Utility.Debug.LogError("收到宠物使用丹药2");
                if (VerifyDrugEffect(drugID, pet,roleid)) { }
                else
                    ResultFailS2C(roleid, RolePetOpCode.PetCultivate);
            }
            else
                ResultFailS2C(roleid, RolePetOpCode.PetCultivate);
        }
        /// <summary>
        /// 验证丹药作用
        /// </summary>
        public bool VerifyDrugEffect(int drugid, Pet pet,int roleid)
        {
            GameEntry. DataManager.TryGetValue<Dictionary<int, DrugData>>(out var DrugDataDict);
            if (DrugDataDict.TryGetValue(InventoryManager.ConvertInt32(drugid), out var drugDatatemp))
            {
                Utility.Debug.LogError("收到宠物使用丹药3");
                if (drugDatatemp.Drug_Type == DrugType.PetExp)
                {
                    Utility.Debug.LogError("收到宠物使用丹药4");
                    PetExpDrug(drugDatatemp, drugid, pet,roleid);
                    return true;
                }
                else if (drugDatatemp.Drug_Type == DrugType.AddPetTalent)
                {
                    PetAtitudeDrug(drugDatatemp, drugid, pet, roleid);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 增加经验
        /// </summary>
        /// <param name="drugData"></param>
        /// <param name="pet"></param>
        public async void PetExpDrug(DrugData drugData, int itemid, Pet pet,int roleid)
        {

            GameEntry. DataManager.TryGetValue<Dictionary<int, PetLevelData>>(out var petLevelDataDict);
            PetDrugRefreshDTO petDrugRefreshDTO = new PetDrugRefreshDTO();
            petDrugRefreshDTO.PetUsedDrug = new Dictionary<int, int>();
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix , roleid.ToString()).Result;
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetDrugRefreshPostfix , pet.ID.ToString()).Result)
            {
                Utility.Debug.LogError("收到宠物使用丹药5");
                var petDrugRefreshJson = RedisHelper.String.StringGetAsync(RedisKeyDefine._PetDrugRefreshPostfix + pet.ID.ToString()).Result;
                if (petDrugRefreshJson != null)
                {
                    Utility.Debug.LogError("收到宠物使用丹药6");
                    petDrugRefreshDTO = Utility.Json.ToObject<PetDrugRefreshDTO>(petDrugRefreshJson);
                    if (petDrugRefreshDTO.PetUsedDrug.TryGetValue(itemid, out int count))
                    {
                        if (count >= 100)
                        {
                            ResultFailS2C(roleid,RolePetOpCode.PetCultivate);
                            return;
                        }
                    }
                }
            }
            Utility.Debug.LogError("收到宠物使用丹药7");
            Utility.Debug.LogError(roleid+"收到宠物使用丹药" + roleExist+ drugData.Need_Level_ID+"///"+ pet.PetLevel);
            if (roleExist&&drugData.Need_Level_ID <= pet.PetLevel && drugData.Max_Level_ID >= pet.PetLevel)
            {
                Utility.Debug.LogError("收到宠物使用丹药8");
                var role = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix , roleid.ToString()).Result;
                var level = pet.PetLevel;
                if (pet.PetLevel <= role.RoleLevel)
                {
                    pet = PetUpdateLevel(petLevelDataDict, pet, role, drugData.Drug_Value);
                }

                else
                {
                    //增加升级判断提示，不能大于人物等级
                    //todo使用丹药失败
                    ResultFailS2C(roleid, RolePetOpCode.PetDrugFresh);
                    return;

                }
                await NHibernateQuerier.UpdateAsync(pet);
                var petAbility = await RedisHelper.Hash.HashGetAsync<PetAbilityPointDTO>(RedisKeyDefine._PetAbilityPointPerfix, pet.ID.ToString());

                var petAptitude = await RedisHelper.Hash.HashGetAsync<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString());

                var petStatus = await RedisHelper.Hash.HashGetAsync<PetStatusDTO>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString());

                var petObj = await RedisHelper.Hash.HashGetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString());
                petObj.PetLevel = pet.PetLevel;
                petObj.PetExp = pet.PetExp;
                await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString(), petObj);
                Utility.Debug.LogInfo("yzqData宠物加经验后1" + Utility.Json.ToJson(petStatus));
                PetStatus petStatuObj = new PetStatus();
                petStatuObj = Utility.Assembly.AssignSameFieldValue<PetStatusDTO, PetStatus>(petStatus, petStatuObj);
                Utility.Debug.LogInfo("yzqData宠物加经验后" + Utility.Json.ToJson(petStatuObj));
                PetAptitude petAptitudeObj = new PetAptitude();
                petAptitudeObj = AssignSameFieldValue(petAptitudeObj, petAptitude);

                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.Pet, petObj);
                dict.Add((byte)ParameterCode.PetAbility, petAbility);
                dict.Add((byte)ParameterCode.PetAptitude, petAptitude);

                var status = VerifyPetAllStatus(pet.ID, petAbility, petAptitudeObj, petStatuObj, pet);
                status.PetID = pet.ID;
                dict.Add((byte)ParameterCode.PetStatus, status);
                ResultSuccseS2C(roleid, RolePetOpCode.PetCultivate, dict);
                await RedisHelper.Hash.HashSetAsync<PetStatus>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString(), status);



                await NHibernateQuerier.UpdateAsync(status);

                if (petDrugRefreshDTO.PetUsedDrug.ContainsKey(drugData.Drug_ID))
                {
                    petDrugRefreshDTO.PetUsedDrug[drugData.Drug_ID]++;
                }
                else
                {
                    petDrugRefreshDTO.PetUsedDrug.Add(drugData.Drug_ID, 1);
                }
                petDrugRefreshDTO.PetID = pet.ID;


                await RedisHelper.Hash.HashSetAsync<PetDrugRefreshDTO>(RedisKeyDefine._PetDrugRefreshPostfix, pet.ID.ToString(), petDrugRefreshDTO);

                ResultSuccseS2C(roleid, RolePetOpCode.PetDrugFresh, petDrugRefreshDTO);

                InventoryManager.UpdateNewItem(roleid, itemid, 1);

            }
            else ResultFailS2C(roleid, RolePetOpCode.PetCultivate);

        }
        /// <summary>
        /// 增加资质
        /// </summary>
        public async void PetAtitudeDrug(DrugData drugData, int itemid, Pet pet, int roleid)
        {
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString()).Result)
            {
                var petAptitude =  RedisHelper.Hash.HashGetAsync<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString()).Result;
                if (petAptitude!=null)
                {
                    GameEntry.DataManager.TryGetValue<Dictionary<int, PetLevelData>>(out var petLevelDataDict);
                    if (petAptitude.PetAptitudeDrug.TryGetValue(drugData.Drug_ID, out var num))
                    {
                        if (num >= 10)
                        {
                            ResultFailS2C(roleid, RolePetOpCode.PetCultivate);
                            return;
                        }
                    }

                    if (drugData.Need_Level_ID <= pet.PetLevel && drugData.Max_Level_ID >= pet.PetLevel)
                    {
                        switch ((GrowUpDrugType)drugData.Drug_ID)
                        {
                            case GrowUpDrugType.HPGrowUp:
                                petAptitude.HPAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.SoulGrowUp:
                                petAptitude.SoulAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.SpeedGrowUp:
                                petAptitude.AttackspeedAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.AttackDamageGrowUp:
                                petAptitude.AttackphysicalAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.ResistanceAttackGrowUp:
                                petAptitude.DefendphysicalAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.PowerGrowUp:
                                petAptitude.AttackpowerAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.ResistancePowerGrowUp:
                                petAptitude.DefendpowerAptitude += drugData.Drug_Value;
                                break;
                            case GrowUpDrugType.GrowUp:
                                petAptitude.Petaptitudecol += drugData.Drug_Value;
                                break;
                            default:
                                break;
                        }
                        if (petAptitude.PetAptitudeDrug.ContainsKey(drugData.Drug_ID))
                        {
                            petAptitude.PetAptitudeDrug[drugData.Drug_ID]++;
                        }
                        else
                        {
                            petAptitude.PetAptitudeDrug.Add(drugData.Drug_ID, 1);
                        }
                        await RedisHelper.Hash.HashSetAsync<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString(), petAptitude);

                        var petAbility = await RedisHelper.Hash.HashGetAsync<PetAbilityPointDTO>(RedisKeyDefine._PetAbilityPointPerfix, pet.ID.ToString());

                        var petStatus = await RedisHelper.Hash.HashGetAsync<PetStatusDTO>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString());

                        var petObj = await RedisHelper.Hash.HashGetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString());



                        PetStatus petStatuObj = new PetStatus();
                        petStatuObj = Utility.Assembly.AssignSameFieldValue<PetStatusDTO, PetStatus>(petStatus, petStatuObj);
                        Utility.Debug.LogInfo("yzqData宠物加经验后" + Utility.Json.ToJson(petStatuObj));
                        PetAptitude petAptitudeObj = new PetAptitude();
                        petAptitudeObj = AssignSameFieldValue(petAptitudeObj, petAptitude);

                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.Pet, petObj);
                        dict.Add((byte)ParameterCode.PetAptitude, petAptitude);

                        var status = VerifyPetAllStatus(pet.ID, petAbility, petAptitudeObj, petStatuObj, pet);
                        status.PetID = pet.ID;
                        dict.Add((byte)ParameterCode.PetStatus, status);

                        await RedisHelper.Hash.HashSetAsync<PetStatus>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString(), status);

                        await NHibernateQuerier.UpdateAsync(status);
                        await NHibernateQuerier.UpdateAsync(petAptitudeObj);
                        ResultSuccseS2C(roleid, RolePetOpCode.PetCultivate, dict);

                        InventoryManager.UpdateNewItem(roleid, itemid, 1);

                    }
                }
            }

        }
        #endregion

        #region 宠物学习技能
        public async void PetStudySkill(int bookid, int petid,int roleid)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            GameEntry. DataManager.TryGetValue<Dictionary<int, PetSkillBookData>>(out var petSkillBookDataDict);
            NHCriteria nHCriteriapetStatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petid);
            var petObj = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriapetStatus);
            var ringObj =ReferencePool.Accquire<RingDTO>();
            ringObj.RingItems = new Dictionary<int, RingItemsDTO>();
            ringObj.RingItems.Add(bookid, new RingItemsDTO());
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var skillList = Utility.Json.ToObject<List<int>>(petObj.PetSkillArray);
            var nHCriteriaRingID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);
            Utility.Debug.LogInfo("yzqData学习技能技能书为"+ bookid +"角色id為"+ roleid+ InventoryManager.VerifyIsExist(bookid, nHCriteriaRingID));
            if (InventoryManager.VerifyIsExist(bookid, nHCriteriaRingID))
            {
                var bookID = InventoryManager.ConvertInt32(bookid);
                if (!skillList.Contains(petSkillBookDataDict[bookID].PetSkillID))
                {
                    Utility.Debug.LogInfo("yzqData学习技能存在");
                    skillList = RandomSkillRemoveAdd(skillList, petSkillBookDataDict[bookID].PetSkillID);
                    petObj.PetSkillArray = Utility.Json.ToJson(skillList);
                    await NHibernateQuerier.UpdateAsync<Pet>(petObj);
                    var petTemp = ChangeDataType(petObj);
                    var status = VerifyPetAllStatus(petTemp.ID, null, null, null, petObj);
                    status.PetID = petTemp.ID;
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.PetStatus, status);
                    dict.Add((byte)ParameterCode.Pet, petTemp);
                    ResultSuccseS2C(roleid, RolePetOpCode.PetStudySkill, dict);
                    await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, roleid.ToString(), petTemp);
                    await NHibernateQuerier.UpdateAsync(status);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PetStatusPerfix, petTemp.ID.ToString(), status);

                    InventoryManager.Remove(roleid, bookid);
                }
                else
                {
                    ResultFailS2C(roleid,RolePetOpCode.PetStudySkill);
                }
            }
            else
            {
                ResultFailS2C(roleid, RolePetOpCode.PetStudySkill);
            }
        }
        #endregion

        #region 妖灵精魄

        public async void UnEquipDemonicSoul(int soulid, int petid, int roleid)
        {
            NHCriteria nHCriteriapetStatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petid);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriapetStatus);



            var soulDict = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);
            Utility.Debug.LogInfo("yzqData卸载的妖灵精魄ID" + soulid);

            if (soulDict.ContainsKey(soulid))
            {
                InventoryManager.AddNewItem(roleid, soulid, 1);
                soulDict.Remove(soulid);
                pet.DemonicSoul = Utility.Json.ToJson(soulDict);
                var status = VerifyPetAllStatus(pet.ID, null, null, null, pet);
                status.PetID = petid;
                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.Pet, ChangeDataType(pet));
                dict.Add((byte)ParameterCode.PetStatus, status);
                ResultSuccseS2C(roleid, RolePetOpCode.RemoveDemonicSoul, dict);

                await NHibernateQuerier.UpdateAsync(pet);
                await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString(), ChangeDataType(pet));
                await NHibernateQuerier.UpdateAsync(status);
                await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString(), status);

            }
            else
                ResultFailS2C(roleid,RolePetOpCode.RemoveDemonicSoul);



        }

        public void AddDemonicSoul(int soulid, out List<int> getSkillList)
        {
            GameEntry. DataManager.TryGetValue<Dictionary<int, DemonicSoulData>>(out var DemonicSoulDict);

            GameEntry. DataManager.TryGetValue<Dictionary<int, DemonicSoulSkillPool>>(out var DemonicSoulSkillPoolDict);

            var skillPoolList = DemonicSoulDict[soulid].DemonicSoulSkill;
            Random random = new Random();
            int num = random.Next(0, skillPoolList.Count);
            var skillList = skillPoolList[num];
            getSkillList = new List<int>();
            for (int i = 0; i < skillList[0]; i++)
            {
                num = random.Next(0, DemonicSoulSkillPoolDict[0].PetSoulSkillPool.Count);
                if (!getSkillList.Contains(DemonicSoulSkillPoolDict[0].PetSoulSkillPool[num]))
                {
                    getSkillList.Add(DemonicSoulSkillPoolDict[0].PetSoulSkillPool[num]);
                }
            }
            for (int i = 0; i < skillList[1]; i++)
            {
                num = random.Next(0, DemonicSoulSkillPoolDict[1].PetSoulSkillPool.Count);
                if (!getSkillList.Contains(DemonicSoulSkillPoolDict[1].PetSoulSkillPool[num]))
                {
                    getSkillList.Add(DemonicSoulSkillPoolDict[1].PetSoulSkillPool[num]);
                }
            }
            for (int i = 0; i < skillList[2]; i++)
            {
                num = random.Next(0, DemonicSoulSkillPoolDict[2].PetSoulSkillPool.Count);
                if (!getSkillList.Contains(DemonicSoulSkillPoolDict[2].PetSoulSkillPool[num]))
                {
                    getSkillList.Add(DemonicSoulSkillPoolDict[2].PetSoulSkillPool[num]);
                }
            }
        }

        public async void EquipDemonicSoul(int roleid,int useitemid,int petid)
        {
            NHCriteria nHCriteriapetStatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petid);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriapetStatus);

            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var demonicSoul = NHibernateQuerier.CriteriaSelectAsync<DemonicSoul>(nHCriteriarole).Result;
            if (demonicSoul != null && pet != null)
            {
                var soulDict = Utility.Json.ToObject<Dictionary<int, DemonicSoulEntity>>(demonicSoul.DemonicSouls);

                var result = soulDict.TryGetValue(useitemid, out var demonicSoulEntity);


                if (result)
                {
                    var soulTemp = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);

                    Utility.Debug.LogInfo("yzqData" + Utility.Json.ToJson(pet));

                    soulTemp.Add(demonicSoulEntity.UniqueID, demonicSoulEntity.Skills);
                    pet.DemonicSoul = Utility.Json.ToJson(soulTemp);
                    await NHibernateQuerier.UpdateAsync(pet);

                    await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString(), ChangeDataType(pet));

                    InventoryManager.Remove(roleid, useitemid);

                    var status= VerifyPetAllStatus(pet.ID, null,null, null, pet);
                    status.PetID = pet.ID;
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.Pet,ChangeDataType(pet));
                    dict.Add((byte)ParameterCode.PetStatus, status);
                    ResultSuccseS2C(roleid, RolePetOpCode.EquipDemonicSoul, dict);

                    await NHibernateQuerier.UpdateAsync(status);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString(), status);
                }
                else
                    ResultFailS2C(roleid, RolePetOpCode.EquipDemonicSoul);
            }
            else
                ResultFailS2C(roleid,RolePetOpCode.EquipDemonicSoul);

        }


        #endregion

        #region 宠物改名
        public async void PetRename(int roleid, PetDTO petDTO)
        {
            NHCriteria nHCriteriapetStatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petDTO.ID);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriapetStatus);

            pet.PetName = petDTO.PetName;
            await NHibernateQuerier.UpdateAsync<Pet>(pet);

            await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString(), ChangeDataType(pet));

            ResultSuccseS2C(roleid, RolePetOpCode.RenamePet, ChangeDataType(pet));
        }
        #endregion

        #region 宠物进阶

        public async void PetEvolution(int roleid, int petid, int itemid)
        {
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);

            NHCriteria nHCriteriapetStatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", petid);
            var pet = NHibernateQuerier.CriteriaSelect<Pet>(nHCriteriapetStatus);

            GameEntry. DataManager.TryGetValue<Dictionary<int, PetLevelData>>(out var petLevelDataDict);
            var demonicSoul = NHibernateQuerier.CriteriaSelectAsync<DemonicSoul>(nHCriteriarole).Result;
            var roleDemonic = Utility.Json.ToObject<Dictionary<int, DemonicSoulEntity>>(demonicSoul.DemonicSouls);
            if (roleDemonic.ContainsKey(itemid))
            {
                if (petLevelDataDict[pet.PetLevel].IsFinalLevel)
                {
                    var demonicDict = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);
                    var skillList = Utility.Json.ToObject<List<int>>(pet.PetSkillArray);
                    if (demonicDict.TryGetValue(itemid, out var skills))
                    {
                        skillList.AddRange(skills);
                        demonicDict.Remove(itemid);
                        pet.PetLevel += 1;
                        pet.PetExp = 0;
                        pet.PetSkillArray = Utility.Json.ToJson(skillList);
                        pet.DemonicSoul = Utility.Json.ToJson(demonicDict);

                        await NHibernateQuerier.UpdateAsync(pet);
                        var petAbility = await RedisHelper.Hash.HashGetAsync<PetAbilityPointDTO>(RedisKeyDefine._PetAbilityPointPerfix, pet.ID.ToString());

                        var petAptitude = await RedisHelper.Hash.HashGetAsync<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString());

                        var petStatus = await RedisHelper.Hash.HashGetAsync<PetStatusDTO>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString());

                        var petObj = await RedisHelper.Hash.HashGetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString());
                        petObj.PetLevel = pet.PetLevel;
                        petObj.PetExp = pet.PetExp;
                        petObj.PetSkillArray = skillList;
                        petObj.DemonicSoul = demonicDict;
                        await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString(), petObj);
                        Utility.Debug.LogInfo("yzqData宠物加经验后1" + Utility.Json.ToJson(petStatus));
                        PetStatus petStatuObj = new PetStatus();
                        petStatuObj = Utility.Assembly.AssignSameFieldValue<PetStatusDTO, PetStatus>(petStatus, petStatuObj);
                        Utility.Debug.LogInfo("yzqData宠物加经验后" + Utility.Json.ToJson(petStatuObj));
                        PetAptitude petAptitudeObj = new PetAptitude();
                        petAptitudeObj = AssignSameFieldValue(petAptitudeObj, petAptitude);


                        roleDemonic.Remove(itemid);
                        demonicSoul.DemonicSouls = Utility.Json.ToJson(roleDemonic);
                        await NHibernateQuerier.UpdateAsync(demonicSoul);
                        DemonicSoulDTO demonicSoulDTO = new DemonicSoulDTO();
                        demonicSoulDTO.RoleID = demonicSoul.RoleID;
                        demonicSoulDTO.DemonicSouls = roleDemonic;

                        var status = VerifyPetAllStatus(pet.ID,petAbility, petAptitudeObj, petStatuObj, pet);
                        status.PetID = pet.ID;
                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.PetStatus, status);
                        dict.Add((byte)ParameterCode.Pet, petObj);
                        dict.Add((byte)ParameterCode.DemonicSoul, demonicSoulDTO);
                        ResultSuccseS2C(roleid,RolePetOpCode.PetEvolution,dict);

                        await RedisHelper.Hash.HashSetAsync<PetStatus>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString(), status);

                        await NHibernateQuerier.UpdateAsync(status);



                    }
                    else
                    {
                        ResultFailS2C(roleid, RolePetOpCode.PetEvolution);
                        return;
                    }
                }
            }
            else
            {
                ResultFailS2C(roleid, RolePetOpCode.PetEvolution);
                return;
            }
        }

        #endregion

        /// <summary>
        /// 新增新宠物
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="petName"></param>
        /// <param name="rolePet"></param>
        public async void InitPet(int petID, string petName, int roleid)
        {
            ///TODO增加可持有宠物数量判断
            NHCriteria nHCriteriaRolePet =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var rolePet = NHibernateQuerier.CriteriaSelect<RolePet>(nHCriteriaRolePet);

            GameEntry.DataManager.TryGetValue<Dictionary<int, PetAptitudeData>>(out var petLevelDataDict);
            #region Pet
            var pet = new Pet();
            pet.PetID = petID;
            pet.PetName = petName;
            pet.PetLevel = 1;
            pet.PetSkillArray = Utility.Json.ToJson(RestPetSkill(petLevelDataDict[petID].SkillArray));
            pet = NHibernateQuerier.Insert<Pet>(pet);
            var petObj =ReferencePool.Accquire<PetDTO>();
            petObj.ID = pet.ID;
            petObj.PetExp = pet.PetExp;
            petObj.DemonicSoul = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);
            petObj.PetID = petID;
            petObj.PetLevel = pet.PetLevel;
            petObj.PetName = pet.PetName;
            petObj.PetSkillArray = Utility.Json.ToObject<List<int>>(pet.PetSkillArray);
            await RedisHelper.Hash.HashSetAsync<PetDTO>(RedisKeyDefine._PetPerfix, pet.ID.ToString(), petObj);
            #endregion
            #region PetAbilityPointDTO
            var petAbilityPointObj = new PetAbilityPointDTO();
            var petAbilityPoint = new PetAbilityPoint();
            petAbilityPoint.ID = pet.ID;
            petAbilityPoint.AbilityPointSln = Utility.Json.ToJson(petAbilityPointObj.AbilityPointSln);
            Utility.Debug.LogInfo("yzqData宠物加点方案" + Utility.Json.ToJson(petAbilityPointObj.AbilityPointSln));
            await  NHibernateQuerier.SaveOrUpdateAsync<PetAbilityPoint>(petAbilityPoint);
            petAbilityPointObj.ID = petAbilityPoint.ID;
            await RedisHelper.Hash.HashSetAsync<PetAbilityPointDTO>(RedisKeyDefine._PetAbilityPointPerfix, pet.ID.ToString(), petAbilityPointObj);
            #endregion
            #region PetAptitudeDTO
            var petAptitude = new PetAptitude();
            ResetPetAptitude(petID, out petAptitude);
            petAptitude.PetID = pet.ID;
            var petAptitudeObj = new PetAptitudeDTO();
            petAptitudeObj.PetID = petAptitude.PetID;
            petAptitudeObj.AttackphysicalAptitude = petAptitude.AttackphysicalAptitude;
            petAptitudeObj.AttackpowerAptitude = petAptitude.AttackpowerAptitude;
            petAptitudeObj.AttackspeedAptitude = petAptitude.AttackspeedAptitude;
            petAptitudeObj.DefendphysicalAptitude = petAptitude.DefendphysicalAptitude;
            petAptitudeObj.DefendpowerAptitude = petAptitude.DefendpowerAptitude;
            petAptitudeObj.HPAptitude = petAptitude.HPAptitude;
            petAptitudeObj.Petaptitudecol = petAptitude.Petaptitudecol;
            petAptitudeObj.PetAptitudeDrug = Utility.Json.ToObject<Dictionary<int, int>>(petAptitude.PetAptitudeDrug);
            petAptitudeObj.SoulAptitude = petAptitude.SoulAptitude;

            await RedisHelper.Hash.HashSetAsync<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, pet.ID.ToString(), petAptitudeObj);
            await NHibernateQuerier.SaveOrUpdateAsync<PetAptitude>(petAptitude);
            #endregion
            #region PetStatusDTO
            var petStatus =new PetStatus();
            ResetPetStatus(pet, petAptitude, out petStatus);
            petStatus.PetID = pet.ID;
            await  RedisHelper.Hash.HashSetAsync<PetStatus>(RedisKeyDefine._PetStatusPerfix, pet.ID.ToString(), petStatus);

            await  NHibernateQuerier.SaveOrUpdateAsync<PetStatus>(petStatus);
            #endregion
            #region RolePetDTO
            var petDict = Utility.Json.ToObject<Dictionary<int, int>>(rolePet.PetIDDict);
            petDict.Add(pet.ID, pet.PetID);
            rolePet.PetIDDict = Utility.Json.ToJson(petDict);
            await NHibernateQuerier.SaveOrUpdateAsync<RolePet>(rolePet);
            var RolepetObj =ReferencePool.Accquire<RolePetDTO>();
            RolepetObj.RoleID = rolePet.RoleID;
            RolepetObj.PetIDDict = petDict;
            RolepetObj.PetIsBattle = rolePet.PetIsBattle;
            await  RedisHelper.Hash.HashSetAsync<RolePetDTO>(RedisKeyDefine._RolePetPerfix, rolePet.RoleID.ToString(), RolepetObj);
            #endregion

            ResultSuccseS2C(roleid,RolePetOpCode.AddPet,null);
        }

        /// <summary>
        /// 宠物升级判断处理
        /// </summary>
        Pet PetUpdateLevel(Dictionary<int, PetLevelData> petLevelDataDict, Pet pet, RoleDTO role, int exp)
        {

            if (petLevelDataDict[pet.PetLevel].IsFinalLevel)
            {
                if (petLevelDataDict[pet.PetLevel].ExpLevelUp <= (pet.PetExp + exp))
                {
                    pet.PetExp = petLevelDataDict[pet.PetLevel].ExpLevelUp;
                    Utility.Debug.LogInfo("yzqData使用加经验丹药即将进阶");
                }
                else
                    pet.PetExp += exp;
            }
            else
            {
                if ((pet.PetExp + exp) >= petLevelDataDict[pet.PetLevel].ExpLevelUp)
                {
                    var level = pet.PetLevel;
                    if (level < role.RoleLevel)
                    {
                        pet.PetExp = pet.PetExp + exp - petLevelDataDict[pet.PetLevel].ExpLevelUp;
                        pet.PetLevel = petLevelDataDict[pet.PetLevel].NextLevelID;
                        return PetUpdateLevel(petLevelDataDict, pet, role, 0);
                    }
                    else if (level == role.RoleLevel)
                    {
                        pet.PetExp = petLevelDataDict[pet.PetLevel].ExpLevelUp;
                    }
                    else
                        return pet;
                }
                else
                    pet.PetExp += exp;
            }
            return pet;
        }
    }
}


