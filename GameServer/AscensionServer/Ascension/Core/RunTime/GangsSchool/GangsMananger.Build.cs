using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public partial class GangsMananger
    {
        #region Redis模块
        /// <summary>
        /// 获得宗门属性及建设
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="roleID"></param>
        async void GetAllianceConstructionS2C(int ID, int roleID)
        {
            var roleAllianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceConstructionPerfix, ID.ToString()).Result;
            var AllianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, ID.ToString()).Result;
            if (roleAllianceExist && AllianceExist)
            {
                var Construction = RedisHelper.Hash.HashGetAsync<AllianceConstructionDTO>(RedisKeyDefine._AllianceConstructionPerfix, ID.ToString()).Result;
                var Alliance = RedisHelper.Hash.HashGetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, ID.ToString()).Result;
                if (Construction != null && Alliance != null)
                {
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.AllianceStatus, Alliance);
                    dict.Add((byte)ParameterCode.AllianceConstruction, Construction);
                    var dissolveExist = RedisHelper.KeyExistsAsync(RedisKeyDefine._DissolveAlliancePerfix + roleID.ToString()).Result;
                    if (dissolveExist)
                    {
                        var dissolve = RedisHelper.String.StringGetAsync(RedisKeyDefine._DissolveAlliancePerfix + roleID.ToString()).Result;
                        if (string .IsNullOrEmpty(dissolve))
                        {
                            dict.Add((byte)ParameterCode.DissolveAlliance, dissolve);
                        }
                    }
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceStatus, dict);
                }
                else
                    GetRoleAliianceConstructionMySql(ID, roleID);
            }
            else
                GetRoleAliianceConstructionMySql(ID, roleID);
        }
        /// <summary>
        /// 宗门建筑升级
        /// </summary>
        async void BuildAllianceConstructionS2C(int ID, int roleID, AllianceConstructionDTO constructionDTO)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<byte, AllianceConstructionData>>(out var construction);
            var roleAllianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceConstructionPerfix, ID.ToString()).Result;
            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, ID.ToString()).Result;
            if (roleAllianceExist&& allianceExist)
            {
                Utility.Debug.LogInfo("YZQ升级宗门建设成员2"+Utility.Json.ToJson(construction));
                var Construction = RedisHelper.Hash.HashGetAsync<AllianceConstruction>(RedisKeyDefine._AllianceConstructionPerfix, ID.ToString()).Result;
                var alliance = RedisHelper.Hash.HashGetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, ID.ToString()).Result;
                if (Construction != null&& alliance!=null)
                {
                    if (constructionDTO.AllianceAlchemyStorage == 1)
                    {
                        var assets = construction[3].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceAlchemyStorage);
                        if (Construction.AllianceAssets>= assets.NeedAllianceSpiritStones)
                        {
                            Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                            Construction.AllianceAlchemyStorage++;                       
                        }
                    }
                    else if (constructionDTO.AllianceArmsDrillSite == 1)
                    {
                        var assets = construction[2].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceArmsDrillSite);
                        if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                        {
                            Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                            Construction.AllianceArmsDrillSite++;
                        }
                    }
                    else if (constructionDTO.AllianceChamber == 1)
                    {

                        if (Construction.AllianceScripturesPlatform == Construction.AllianceChamber && Construction.AllianceChamber == Construction.AllianceArmsDrillSite && Construction.AllianceChamber == Construction.AllianceAlchemyStorage)
                        {
                            var assets = construction[1].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceChamber);
                            if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                            {
                                Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                                Construction.AllianceChamber++;
                                alliance.AllianceLevel = Construction.AllianceChamber;
                                assets = construction[1].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceChamber);
                                alliance.AlliancePeopleMax = assets.MaxValue;
                            }
                        }
                    }
                    else if (constructionDTO.AllianceScripturesPlatform == 1)
                    {
                        var assets = construction[4].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceScripturesPlatform);
                        if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                        {
                            Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                            Construction.AllianceScripturesPlatform++;
                        }
                    }
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.AllianceStatus, alliance);
                    dict.Add((byte)ParameterCode.AllianceConstruction, Construction);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.BuildAlliance, dict);
                    await  RedisHelper.Hash.HashSetAsync<AllianceConstruction>(RedisKeyDefine._AllianceConstructionPerfix,ID.ToString(),Construction);
                    await NHibernateQuerier.UpdateAsync(Construction);

                    await RedisHelper.Hash.HashSetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, ID.ToString(), alliance);
                    await NHibernateQuerier.UpdateAsync(alliance);
                } else
                    BuildAllianceConstructionMySql(ID, roleID, constructionDTO);
            }
            else
                BuildAllianceConstructionMySql(ID, roleID, constructionDTO);
        }
        /// <summary>
        /// 升级宗门技能等级
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="skillDTO"></param>
        async void UpdateAllianceSkillS2C(int roleID,RoleAllianceSkillDTO skillDTO)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<string, AllianceSkillsData>>(out var SkillDict);
            var skillExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAllianceSkillPerfix,roleID.ToString()).Result;
            var assetsExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            if (skillExist && assetsExist&& allianceExist)
            {
                var skillObj = RedisHelper.Hash.HashGetAsync<RoleAllianceSkill>(RedisKeyDefine._RoleAllianceSkillPerfix, roleID.ToString()).Result;
                var assetsObj = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                var allianceObj = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                if (skillObj != null && assetsObj != null&& allianceObj!=null)
                {
                    if (skillDTO.SkillInsight == 1)
                    {
                        var obj = SkillDict["Insight"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillInsight);
                        if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillInsight < 150&& allianceObj.Reputation>= obj.AllianceContribution)
                        {
                            allianceObj.Reputation -= obj.AllianceContribution;
                            assetsObj.SpiritStonesLow -= obj.SpiritStones;
                            skillObj.SkillInsight++;
                        }
                        else
                        {
                            RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                            return;
                        }
                    }
                    else if (skillDTO.SkillMeditation == 1)
                    {
                        var obj = SkillDict["Meditation"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillMeditation);
                        if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillMeditation < 150 && allianceObj.Reputation >= obj.AllianceContribution)
                        {
                            allianceObj.Reputation -= obj.AllianceContribution;
                            assetsObj.SpiritStonesLow -= obj.SpiritStones;
                            skillObj.SkillMeditation++;
                        }
                        else
                        {
                            RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                            return;
                        }
                    }
                    else if (skillDTO.SkillRapid == 1)
                    {
                        var obj = SkillDict["Rapid"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillRapid);
                        if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillRapid < 150 && allianceObj.Reputation >= obj.AllianceContribution)
                        {
                            allianceObj.Reputation -= obj.AllianceContribution;
                            assetsObj.SpiritStonesLow -= obj.SpiritStones;
                            skillObj.SkillRapid++;
                        }
                        else
                        {
                            RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                            return;
                        }
                    }
                    else if (skillDTO.SkillStrong == 1)
                    {
                        var obj = SkillDict["StrongBody"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillStrong);
                        if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillStrong < 150 && allianceObj.Reputation >= obj.AllianceContribution)
                        {
                            allianceObj.Reputation -= obj.AllianceContribution;
                            assetsObj.SpiritStonesLow -= obj.SpiritStones;
                            skillObj.SkillStrong++;
                        }
                        else
                        {
                            RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                            return;
                        }
                    }
                    RedisHelper.Hash.HashSet<RoleAllianceSkill>(RedisKeyDefine._RoleAllianceSkillPerfix, roleID.ToString(), skillObj);
                    RedisHelper.Hash.HashSet<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), assetsObj);
                    RedisHelper.Hash.HashSet<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), allianceObj);
                    await NHibernateQuerier.UpdateAsync(skillObj);
                    await NHibernateQuerier.UpdateAsync(assetsObj);
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(allianceObj));

                    var status =GameEntry.practiceManager. RoleStatusAlgorithm(roleID, null, null, null, null, null, skillObj);
                    status.RoleID = assetsObj.RoleID;
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();

                    dict.Add((byte)ParameterCode.RoleAssets, assetsObj);
                    dict.Add((byte)ParameterCode.RoleAllianceSkill, skillObj);
                    dict.Add((byte)ParameterCode.RoleStatus, status);
                    dict.Add((byte)ParameterCode.RoleAlliance, allianceObj);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.UpdateAllianceSkill, dict);
                }
                else
                {
                    UpdateAllianceSkillMySql(roleID, skillDTO);
                }
            }
            else
            {
                UpdateAllianceSkillMySql(roleID, skillDTO);
            }

        }
        /// <summary>
        /// 获得宗门技能
        /// </summary>
         void GetAllianceSkillS2C(int roleID)
        {
            var skillExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAllianceSkillPerfix,roleID.ToString()).Result;
            if (skillExist)
            {
                var skillObj = RedisHelper.Hash.HashGetAsync<RoleAllianceSkillDTO>(RedisKeyDefine._RoleAllianceSkillPerfix, roleID.ToString()).Result;
                if (skillObj != null)
                {
                    Utility.Debug.LogInfo("获得角色宗門技能" + roleID);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceSkill, skillObj);
                }
                else
                    GetAllianceSkillMySql(roleID);
            }
            else
                GetAllianceSkillMySql(roleID);
        }
        /// <summary>
        /// 獲得領洞府信息
        /// </summary>
        void GetDongFuStatusS2C(int roleid,int id)
        {
            var dondfuExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceDongFuPostfix,id.ToString()).Result;
            if (dondfuExist)
            {
                var dondfuObj =RedisHelper.Hash.HashGetAsync<AllianceDongFuDTO>(RedisKeyDefine._AllianceDongFuPostfix, id.ToString()).Result;
                if (dondfuObj != null)
                {
                    RoleStatusSuccessS2C(roleid, AllianceOpCode.GetDongFuStatus, dondfuObj);
                }
                else
                    GetDongFuStatusMySql(roleid, id);
            }
            else
                GetDongFuStatusMySql(roleid, id);
        }
      /// <summary>
      /// 获得签到数据
      /// </summary>
      /// <param name="roleID"></param>
      /// <param name="id"></param>
        async void GetAllianceSigninS2C(int roleID,int id)
        {
            var signin = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceSigninPerfix,roleID.ToString()).Result;
            if (signin)
            {
                var signinObj = RedisHelper.Hash.HashGetAsync<AllianceSigninDTO>(RedisKeyDefine._AllianceSigninPerfix, roleID.ToString()).Result;
                if (signinObj == null)
                {
                    signinObj = new AllianceSigninDTO() { AllianceID = id, IsSignin = false,RoleID= roleID};
                   await  RedisHelper.Hash.HashSetAsync<AllianceSigninDTO>(RedisKeyDefine._AllianceSigninPerfix, roleID.ToString(), signinObj);
                }
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceSignin, signinObj);
            }
           else
            {
               var signinObj = new AllianceSigninDTO() { AllianceID = id, IsSignin = false, RoleID = roleID };
                await RedisHelper.Hash.HashSetAsync<AllianceSigninDTO>(RedisKeyDefine._AllianceSigninPerfix, roleID.ToString(), signinObj);
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceSignin, signinObj);
            }
        }

        async void UpdateAllianceSigninS2C(int roleID, int id)
        {
            GameEntry.DataManager.TryGetValue<List<AllianceSigninData>>(out var signinList);
            var signinExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceSigninPerfix, roleID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            var roleAllianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceConstructionPerfix, id.ToString()).Result;
            var assetsExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            var statusExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
            Utility.Debug.LogInfo("YZQ更新宗门签到1" + signinExist + roleExist + roleAllianceExist + allianceExist + assetsExist + statusExist);

            if (signinExist && roleExist && roleAllianceExist && allianceExist && assetsExist && statusExist)
            {
                var signinObj = RedisHelper.Hash.HashGetAsync<AllianceSigninDTO>(RedisKeyDefine._AllianceSigninPerfix, roleID.ToString()).Result;
                var roleObj = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
                var roleAllianceObj = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                var allianceObj = RedisHelper.Hash.HashGetAsync<AllianceConstruction>(RedisKeyDefine._AllianceConstructionPerfix, id.ToString()).Result;
                var statusObj = RedisHelper.Hash.HashGetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
                var assetsObj = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;

                Utility.Debug.LogInfo("YZQ更新宗门签到2" + (signinObj != null) +""+ (roleObj != null) +""+ (assetsObj != null) +""+ (statusObj != null) +""+ (roleAllianceObj != null) +""+ (allianceObj != null));


                if (signinObj != null && roleObj != null && assetsObj != null && statusObj != null && roleAllianceObj != null && allianceObj != null)
                {
                    if (!signinObj.IsSignin)
                    {
                        Utility.Debug.LogInfo("YZQ更新宗门签到3" + !signinObj.IsSignin);
                        for (int i = 0; i < signinList.Count; i++)
                        {
                            if (roleObj.RoleLevel <= signinList[i].Role_Level[1] && roleObj.RoleLevel >= signinList[i].Role_Level[0])
                            {
                                assetsObj.SpiritStonesLow += signinList[i].Role_Reward;
                                statusObj.Popularity += signinList[i].Alliance_Popularity;
                                roleAllianceObj.Reputation += signinList[i].Role_Contribution;
                                roleAllianceObj.ReputationHistroy += roleAllianceObj.Reputation;
                                roleAllianceObj.ReputationMonth += roleAllianceObj.Reputation;
                                allianceObj.AllianceAssets += signinList[i].Alliance_Spirit_Stone;

                                signinObj.IsSignin = true;
                                Dictionary<byte, object> dict = new Dictionary<byte, object>();

                                dict.Add((byte)ParameterCode.RoleAlliance, roleAllianceObj);
                                dict.Add((byte)ParameterCode.RoleAssets, assetsObj);
                                dict.Add((byte)ParameterCode.AllianceSignin, signinObj);
                                dict.Add((byte)ParameterCode.AllianceStatus, statusObj);
                                RoleStatusSuccessS2C(roleID, AllianceOpCode.UpdateAllianceSignin, dict);

                                await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), roleAllianceObj);
                                await RedisHelper.Hash.HashSetAsync<AllianceConstruction>(RedisKeyDefine._AllianceConstructionPerfix, id.ToString(), allianceObj);
                                await RedisHelper.Hash.HashSetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, id.ToString(), statusObj);
                                await RedisHelper.Hash.HashSetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), assetsObj);
                                await RedisHelper.Hash.HashSetAsync<AllianceSigninDTO>(RedisKeyDefine._AllianceSigninPerfix, roleID.ToString(), signinObj);

                                await NHibernateQuerier.UpdateAsync(assetsObj);
                                await NHibernateQuerier.UpdateAsync(statusObj);
                                await NHibernateQuerier.UpdateAsync(allianceObj);
                                await NHibernateQuerier.UpdateAsync(ChangeDataType(roleAllianceObj));
                            }
                        }
                    }
                    else
                        RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSignin);
                }
                else
                    RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSignin);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSignin);
        }
        #endregion

        #region MySql模块
        /// <summary>
        /// 获取仙盟建筑信息
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="roleID"></param>
        void GetRoleAliianceConstructionMySql(int ID, int roleID)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", ID);
            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ID);
            var Construction = NHibernateQuerier.CriteriaSelect<AllianceConstruction>(nHCriteria);
            var Alliance = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaAlliance);
            Utility.Debug.LogInfo("获得数据库宗門建设数据" + Utility.Json.ToJson(Construction));
            if (Alliance != null && Alliance != null)
            {
                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                var dissolveExist = RedisHelper.KeyExistsAsync(RedisKeyDefine._DissolveAlliancePerfix + roleID.ToString()).Result;
                if (dissolveExist)
                {
                    var dissolve = RedisHelper.String.StringGetAsync(RedisKeyDefine._DissolveAlliancePerfix + roleID.ToString()).Result;
                    if (string.IsNullOrEmpty(dissolve))
                    {
                        dict.Add((byte)ParameterCode.DissolveAlliance, dissolve);
                    }
                }
                dict.Add((byte)ParameterCode.AllianceStatus, Alliance);
                dict.Add((byte)ParameterCode.AllianceConstruction, Construction);
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceStatus, dict);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.GetAllianceStatus);
        }

        async void BuildAllianceConstructionMySql(int ID, int roleID, AllianceConstructionDTO constructionDTO)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<byte, AllianceConstructionData>>(out var construction);
            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", ID);
            NHCriteria nHCriteriaAlliancestatus =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ID);
            var Construction = NHibernateQuerier.CriteriaSelect<AllianceConstruction>(nHCriteriaAlliance);
            var alliance = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaAlliancestatus);
            if (Construction != null&& alliance!=null)
            {
                Utility.Debug.LogInfo("YZQ升级宗门建设成员3");
                if (constructionDTO.AllianceAlchemyStorage == 1)
                {
                    var assets = construction[3].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceAlchemyStorage);
                    if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                    {
                        Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                        Construction.AllianceAlchemyStorage++;
                    }
                }
                else if (constructionDTO.AllianceArmsDrillSite == 1)
                {
                    var assets = construction[2].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceArmsDrillSite);
                    if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                    {
                        Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                        Construction.AllianceArmsDrillSite++;
                    }
                }
                else if (constructionDTO.AllianceChamber == 1)
                {

                    if (Construction.AllianceScripturesPlatform == Construction.AllianceChamber && Construction.AllianceChamber == Construction.AllianceArmsDrillSite && Construction.AllianceChamber == Construction.AllianceAlchemyStorage)
                    {
                        var assets = construction[1].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceChamber);
                        if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                        {
                            Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                            Construction.AllianceChamber++;
                            alliance.AllianceLevel = Construction.AllianceChamber;
                             assets = construction[1].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceChamber);
                            alliance.AlliancePeopleMax = assets.MaxValue;
                        }
                    }
                }
                else if (constructionDTO.AllianceScripturesPlatform == 1)
                {
                    var assets = construction[4].AllianceBuildingData.Find((x) => x.BuildingLevel == Construction.AllianceScripturesPlatform);
                    if (Construction.AllianceAssets >= assets.NeedAllianceSpiritStones)
                    {
                        Construction.AllianceAssets -= assets.NeedAllianceSpiritStones;
                        Construction.AllianceScripturesPlatform++;
                    }
                }

                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.AllianceStatus, alliance);
                dict.Add((byte)ParameterCode.AllianceConstruction, Construction);
                RoleStatusSuccessS2C(roleID, AllianceOpCode.BuildAlliance, dict);

                await RedisHelper.Hash.HashSetAsync<AllianceConstruction>(RedisKeyDefine._AllianceConstructionPerfix, ID.ToString(), Construction);
                await NHibernateQuerier.UpdateAsync(Construction);

                await RedisHelper.Hash.HashSetAsync<AllianceStatus>(RedisKeyDefine._AllianceConstructionPerfix, ID.ToString(), alliance);
                await NHibernateQuerier.UpdateAsync(alliance);
            }
            else
                RoleStatusFailS2C(roleID,AllianceOpCode.BuildAlliance);
        }
        /// <summary>
        /// 获得角色宗门技能
        /// </summary>
        /// <param name="roleID"></param>
        void GetAllianceSkillMySql(int roleID)
        {
            Utility.Debug.LogInfo("获得角色宗門技能" + roleID);
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var skillObj = NHibernateQuerier.CriteriaSelect<RoleAllianceSkill>(nHCriteriarole);
            Utility.Debug.LogInfo("获得角色宗門技能" + Utility.Json.ToJson(skillObj));
            if (skillObj != null)
            {
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceSkill, skillObj);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.GetAllianceSkill);
        }

        async void UpdateAllianceSkillMySql(int roleID, RoleAllianceSkillDTO skillDTO)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<string, AllianceSkillsData>>(out var SkillDict);
            Utility.Debug.LogInfo("角色宗門技能升级2"+Utility.Json.ToJson(SkillDict));
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var skillObj = NHibernateQuerier.CriteriaSelect<RoleAllianceSkill>(nHCriteriarole);
            var assetsObj = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriarole);
            var allianceObj = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriarole);
            if (skillObj != null && assetsObj != null&& allianceObj!=null)
            {
                if (skillDTO.SkillInsight == 1)
                {
                    var obj = SkillDict["Insight"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillInsight);
                    if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillInsight < 150&& allianceObj.Reputation>=obj.AllianceContribution)
                    {
                        allianceObj.Reputation -= obj.AllianceContribution;
                        assetsObj.SpiritStonesLow -= obj.SpiritStones;
                        skillObj.SkillInsight++;
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                        return;
                    }
                }
                else if (skillDTO.SkillMeditation == 1)
                {
                    var obj = SkillDict["Meditation"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillMeditation);
                    if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillMeditation < 150 && allianceObj.Reputation >= obj.AllianceContribution)
                    {
                        allianceObj.Reputation -= obj.AllianceContribution;
                        assetsObj.SpiritStonesLow -= obj.SpiritStones;
                        skillObj.SkillMeditation++;
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                        return;
                    }
                }
                else if (skillDTO.SkillRapid == 1)
                {
                    var obj = SkillDict["Rapid"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillRapid);
                    if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillRapid < 150 && allianceObj.Reputation >= obj.AllianceContribution)
                    {
                        allianceObj.Reputation -= obj.AllianceContribution;
                        assetsObj.SpiritStonesLow -= obj.SpiritStones;
                        skillObj.SkillRapid++;
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                        return;
                    }
                }
                else if (skillDTO.SkillStrong == 1)
                {
                    var obj = SkillDict["StrongBody"].AllianceSkillData.Find((x) => x.SkillLevel == skillObj.SkillStrong);
                    if (obj.SpiritStones <= assetsObj.SpiritStonesLow && skillObj.SkillStrong < 150 && allianceObj.Reputation >= obj.AllianceContribution)
                    {
                        allianceObj.Reputation -= obj.AllianceContribution;
                        assetsObj.SpiritStonesLow -= obj.SpiritStones;
                        skillObj.SkillStrong++;
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
                        return;
                    }
                }
                Utility.Debug.LogInfo("角色宗門技能升级2");
                RedisHelper.Hash.HashSet<RoleAllianceSkill>(RedisKeyDefine._RoleAllianceSkillPerfix, roleID.ToString(), skillObj);
                RedisHelper.Hash.HashSet<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), assetsObj);
                RedisHelper.Hash.HashSet<RoleAlliance>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), allianceObj);

                await NHibernateQuerier.UpdateAsync(skillObj);
                await NHibernateQuerier.UpdateAsync(assetsObj);
                await NHibernateQuerier.UpdateAsync(ChangeDataType(allianceObj));

                var status = GameEntry.practiceManager.RoleStatusAlgorithm(roleID, null, null, null, null, null, skillObj);
                status.RoleID = assetsObj.RoleID;
                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.RoleAssets, assetsObj);
                dict.Add((byte)ParameterCode.RoleAllianceSkill, skillObj);
                dict.Add((byte)ParameterCode.RoleStatus, status);
                dict.Add((byte)ParameterCode.RoleAlliance, ChangeDataType(allianceObj));
                RoleStatusSuccessS2C(roleID,AllianceOpCode.UpdateAllianceSkill,dict);
            }
            else
            {
                RoleStatusFailS2C(roleID, AllianceOpCode.UpdateAllianceSkill);
            }
        }

        /// <summary>
        /// 獲得洞府
        /// </summary>
        async void GetDongFuStatusMySql(int roleid, int id)
        {
            NHCriteria nHCriteriAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", id);
            var dongfu = NHibernateQuerier.CriteriaSelect<AllianceDongFu>(nHCriteriAlliance);
            if (dongfu != null)
            {
                RoleStatusSuccessS2C(roleid, AllianceOpCode.GetDongFuStatus, ChangeDataType(dongfu));
            }
            else
                RoleStatusFailS2C(roleid,AllianceOpCode.GetDongFuStatus);

        }
        #endregion

        AllianceDongFuDTO ChangeDataType(AllianceDongFu dongFu)
        {
            AllianceDongFuDTO dongFuDTO = new AllianceDongFuDTO();
            dongFuDTO.AllianceID = dongFu.AllianceID;
            dongFuDTO.Occupant = dongFu.Occupant;
            dongFuDTO.SpiritRangeID = dongFu.SpiritRangeID;
            dongFuDTO.PreemptOne = Utility.Json.ToObject<List<PreemptInfo>>(dongFu.PreemptOne);
            dongFuDTO.PreemptTow = Utility.Json.ToObject<List<PreemptInfo>>(dongFu.PreemptTow);
            dongFuDTO.PreemptThree = Utility.Json.ToObject<List<PreemptInfo>>(dongFu.PreemptThree);
            dongFuDTO.PreemptFour = Utility.Json.ToObject<List<PreemptInfo>>(dongFu.PreemptFour);
            dongFuDTO.PreemptFive = Utility.Json.ToObject<List<PreemptInfo>>(dongFu.PreemptFive);
            return dongFuDTO;
        }


    }
}
