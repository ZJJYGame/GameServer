//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Photon.SocketServer;
//using AscensionProtocol;
//using AscensionProtocol.DTO;
//using AscensionProtocol;
//using Photon.SocketServer;
//using AscensionServer.Model;
//using Cosmos;
//using Protocol;
//namespace AscensionServer
//{
//    public class GetRoleStatusSubHandler : SyncRoleStatusSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string rolestatusJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleStatus));
//            var rolestatusObj = Utility.Json.ToObject<RoleStatusDTO>(rolestatusJson);
//           // Utility.Debug.LogInfo("yzqData收到的角色数据"+ rolestatusJson);
//            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolestatusObj.RoleID);
//            var roleStatus = NHibernateQuerier.CriteriaSelect<RoleStatus>(nHCriteriaRoleStatue);
//            OperationData operationData = new OperationData();
//         //   Utility.Debug.LogInfo("yzqData数据库找到的角色数据" + Utility.Json.ToJson(roleStatus));
//            List<NHCriteria> nHCriteriaList = new List<NHCriteria>();
//            nHCriteriaList.Add(nHCriteriaRoleStatue);
//            switch (rolestatusObj.StatusChange)
//            {
//                case RoleStatusDTO.StatusChangeType.StatusGet:
//                    operationData.DataMessage = Utility.Json.ToJson(roleStatus);
//                    operationData.OperationCode = (byte)OperationCode.RoleStatusGet;
//                    GameEntry. RoleManager.SendMessage(roleStatus.RoleID, operationData);
//                    break;
//                case RoleStatusDTO.StatusChangeType.StatusReplyAll:
//                    #region 升级更新属性
//                    #region ServerJson
//                    GameEntry. DataManager.TryGetValue<Dictionary<int, List<MishuSkillData>>>(out var mishuDict);

//                    GameEntry. DataManager.TryGetValue<Dictionary<int, GongFa>>(out var gongfaDict);

//                    GameEntry. DataManager.TryGetValue<Dictionary<string, List<AllianceSkillsData>>>(out var allianceSkilldataDict);

//                    GameEntry. DataManager.TryGetValue<Dictionary<int, RoleStatusDatas>>(out var roleStatusDict);

//                    GameEntry. DataManager.TryGetValue<Dictionary<int, EquipmentData>>(out var equipmentDict);
//                    #endregion
//                    #region 获取数据库映射

//                    var roleObj = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriaRoleStatue);

//                    var rolegongfaObj = NHibernateQuerier.CriteriaSelect<RoleGongFa>(nHCriteriaRoleStatue);

//                    var rolemishuObj = NHibernateQuerier.CriteriaSelect<RoleMiShu>(nHCriteriaRoleStatue);

//                    var allianceObj = NHibernateQuerier.CriteriaSelect<RoleAllianceSkill>(nHCriteriaRoleStatue);

//                    //var roleringObj = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteriaRoleStatue);
//                    //NHCriteria nHCriteriaring =ReferencePool.Accquire<NHCriteria>().SetValue("ID", roleringObj.RingIdArray);
//                    //var ringObj = NHibernateQuerier.CriteriaSelect<Ring>(nHCriteriaring);
//                    //var equipDict = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringObj.RingAdorn);
//                    //var weaponObj = NHibernateQuerier.CriteriaSelect<Weapon>(nHCriteriaRoleStatue);

//                    //foreach (var equip in equipDict)
//                    //{

//                    //}
//                    #endregion
//                    #region 应用数据
//                    List<GongFa> gongfastatusList = new List<GongFa>();
//                    List<MishuSkillData> mishustatusList = new List<MishuSkillData>();
//                    RoleAllianceSkill roleAllianceSkill = new RoleAllianceSkill();
//                    RoleStatusDatas roleStatusDatas = new RoleStatusDatas();
//                    #endregion

//                    if (roleObj != null)
//                    {
//                        if (rolegongfaObj != null)
//                        {
//                            var gongfaidDict = Utility.Json.ToObject<Dictionary<int, int>>(rolegongfaObj.GongFaIDArray);
//                            if (gongfaidDict.Count > 0)
//                            {
//                                foreach (var item in gongfaidDict)
//                                {
//                                    var temp = gongfaDict[item.Value];
//                                    if (temp != null)
//                                        gongfastatusList.Add(temp);
//                                }
//                            }
//                        }

//                        if (rolemishuObj != null)
//                        {
//                            var mishuidDict = Utility.Json.ToObject<Dictionary<int, int>>(rolemishuObj.MiShuIDArray);
//                            if (mishuidDict.Count > 0)
//                            {
//                                foreach (var item in mishuidDict)
//                                {
//                                    NHCriteria nHCriteriamishu =ReferencePool.Accquire<NHCriteria>().SetValue("ID", item.Key);
//                                    var mishuObj = NHibernateQuerier.CriteriaSelect<MiShu>(nHCriteriamishu);

//                                    var temp = mishuDict[item.Value].Find(t => t.Mishu_Floor == mishuObj.MiShuLevel);

//                                    if (temp != null)
//                                        mishustatusList.Add(temp);
//                                }
//                            }
//                        }

//                        if (allianceObj != null)
//                        {
//                            var skillStrongObj = allianceSkilldataDict["SkillStrong"].Find(t => t.Skill_Level == allianceObj.SkillStrong);
//                            if (skillStrongObj != null)
//                            {
//                                roleAllianceSkill.SkillStrong = skillStrongObj.Skill_Level * skillStrongObj.AddCoefficient;
//                            }

//                            var skillInsightObj = allianceSkilldataDict["SkillInsight"].Find(t => t.Skill_Level == allianceObj.SkillInsight);
//                            if (skillInsightObj != null)
//                            {
//                                roleAllianceSkill.SkillInsight = skillInsightObj.Skill_Level * skillInsightObj.AddCoefficient;
//                            }

//                            var skillMeditationObj = allianceSkilldataDict["SkillMeditation"].Find(t => t.Skill_Level == allianceObj.SkillMeditation);
//                            if (skillMeditationObj != null)
//                            {
//                                roleAllianceSkill.SkillMeditation = skillMeditationObj.Skill_Level * skillMeditationObj.AddCoefficient;
//                            }

//                            var skillRapidObj = allianceSkilldataDict["SkillRapid"].Find(t => t.Skill_Level == allianceObj.SkillRapid);
//                            if (skillRapidObj != null)
//                            {
//                                roleAllianceSkill.SkillRapid = skillRapidObj.Skill_Level * skillRapidObj.AddCoefficient;
//                            }
//                            Utility.Debug.LogInfo("yzqData数据是否为空4");
//                        }

//                        roleStatusDict.TryGetValue(roleObj.RoleLevel, out roleStatusDatas);
//                    }
//                    StatusCalculateHelper.GetRoleStatus(gongfastatusList, mishustatusList, roleStatusDatas, out RoleStatus rolestatusTemp);
//                    if (rolestatusTemp != null)
//                    {
//                        rolestatusTemp.RoleID = roleStatus.RoleID;
//                        Utility.Debug.LogInfo("yzqData角色属性最大值为" + Utility.Json.ToJson(rolestatusTemp));
//                        operationData.DataMessage = Utility.Json.ToJson(rolestatusTemp);
//                        operationData.OperationCode = (byte)OperationCode.RoleStatusFullRecovery;
//                       GameEntry. RoleManager.SendMessage(roleObj.RoleID, operationData);
//                        NHibernateQuerier.Update<RoleStatus>(rolestatusTemp);
//                    }
//                    else
//                    {
//                        SetResponseParamters(() => operationResponse.ReturnCode = (short)ReturnCode.Fail);
//                    }
//                    #endregion
//                    break;
//                default:
//                    break;
//            }
//            ReferencePool.Release(nHCriteriaList);
//            return operationResponse;
//        }
//    }
//}

