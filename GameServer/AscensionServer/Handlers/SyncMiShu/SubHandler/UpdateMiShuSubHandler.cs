//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Photon.SocketServer;
//using AscensionProtocol;
//using AscensionProtocol.DTO;
//using AscensionServer.Model;
//using Cosmos;
//using RedisDotNet;
//namespace AscensionServer
//{
//    public class UpdateMiShuSubHandler : SyncMiShuSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var rolemishuJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.Role));
//            var mishuJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.MiShu));
//            var rolemishuObj = Utility.Json.ToObject<RoleMiShuDTO>(rolemishuJson);
//            var mishuObj = Utility.Json.ToObject<MiShuDTO>(mishuJson);
//            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolemishuObj.RoleID);
//            Utility.Debug.LogInfo("yzqData收到的角色秘术数据" + mishuJson);
//            GameEntry. DataManager.TryGetValue<Dictionary<int, List<MishuSkillData>>>(out var MiShuDataDict);
//            if (RedisHelper.KeyExistsAsync(RedisKeyDefine._MiShuPerfix).Result)
//            {
//                Utility.Debug.LogInfo("yzqData进入Redis判断");
//                var rolemishuRedisObj = RedisHelper.Hash.HashGetAsync<RoleMiShuDTO>(RedisKeyDefine._RoleMiShuPerfix + rolemishuObj.RoleID, rolemishuObj.RoleID.ToString()).Result;
//                if (rolemishuRedisObj.MiShuIDDict.ContainsKey(mishuObj.MiShuID))
//                {
//                    var mishuRedisObj = RedisHelper.Hash.HashGetAsync<MiShuDTO>(RedisKeyDefine._MiShuPerfix + rolemishuObj.RoleID, rolemishuObj.RoleID.ToString()).Result;
//                    if (mishuObj.MiShuLevel > 0)
//                    {
//                        if (mishuObj.MiShuExp >= 0)
//                        {
//                            mishuRedisObj.MiShuExp += mishuObj.MiShuExp;
//                        }
//                        if (mishuObj.MiShuLevel >= 0)
//                        {
//                            mishuRedisObj.MiShuLevel += mishuObj.MiShuLevel;
//                        }
//                        if (MiShuDataDict.TryGetValue(mishuObj.MiShuID, out List<MishuSkillData> mishuSkillDatas))
//                        {
//                            var mishuSkillData = mishuSkillDatas.Find(t => t.NeedLevelID == mishuRedisObj.MiShuLevel);
//                            if (mishuSkillData != null)
//                                mishuRedisObj.MiShuSkillArry.AddRange(mishuSkillData.SkillArrayOne);
//                        }
//                    }
//                    else
//                    {
//                        if (mishuObj.MiShuExp >= 0)
//                        {
//                            mishuRedisObj.MiShuExp += mishuObj.MiShuExp;
//                        }
//                    }
//                    RedisHelper.Hash.HashSet<MiShuDTO>(RedisKeyDefine._MiShuPerfix + rolemishuObj.RoleID, rolemishuObj.RoleID.ToString(), mishuObj);
//                    NHCriteria nHCriteriaMiShuID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", mishuObj.MiShuID);
//                    var mishuMySQL = NHibernateQuerier.CriteriaSelect<MiShu>(nHCriteriaMiShuID);
//                    mishuMySQL.MiShuSkillArry = Utility.Json.ToJson(mishuRedisObj.MiShuSkillArry);
//                    NHibernateQuerier.Update<MiShu>(mishuMySQL);
//                    if (mishuObj.MiShuLevel > 0)
//                    { 
//                        //升级发送更新的数据
//                        SetResponseParamters(() =>
//                        {
//                            subResponseParameters.Add((byte)ParameterCode.MiShu, Utility.Json.ToJson(mishuRedisObj));
//                            operationResponse.ReturnCode = (short)ReturnCode.Success;
//                        });
//                    }
//                    ReferencePool.Release(nHCriteriaMiShuID);
//                }else
//                    SetResponseParamters(() => operationResponse.ReturnCode = (short)ReturnCode.Fail);
//            }
//            else
//            {
//                Utility.Debug.LogInfo("yzqData进入mYsql判断");
//                RoleMiShu roleMishu = NHibernateQuerier.CriteriaSelect<RoleMiShu>(nHCriteriaRoleID);
//                var mishuDict = Utility.Json.ToObject<Dictionary<int, int>>(roleMishu.MiShuIDArray);
//                if (mishuDict.ContainsKey(mishuObj.MiShuID))
//                {
//                    NHCriteria nHCriteriaMiShuID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", mishuObj.MiShuID);
//                    var mishuMySQLObj = NHibernateQuerier.CriteriaSelect<MiShu>(nHCriteriaMiShuID);
//                    var mishuSkillList = Utility.Json.ToObject<List<int>>(mishuMySQLObj.MiShuSkillArry);
//                    if (mishuObj.MiShuLevel>0)
//                    {
//                        if (mishuObj.MiShuExp >= 0)
//                        {
//                            mishuMySQLObj.MiShuExp += mishuObj.MiShuExp;
//                        }
//                        if (mishuObj.MiShuLevel >= 0)
//                        {
//                            mishuMySQLObj.MiShuLevel += mishuObj.MiShuLevel;
//                        }
//                        if (MiShuDataDict.TryGetValue(mishuObj.MiShuID, out List<MishuSkillData> mishuSkillDatas))
//                        {
//                            var mishuSkillData = mishuSkillDatas.Find(t => t.NeedLevelID == mishuMySQLObj.MiShuLevel);
//                            if (mishuSkillData != null)
//                                mishuSkillList.AddRange(mishuSkillData.SkillArrayOne);
//                        }
//                    }
//                    else
//                    {
//                        if (mishuObj.MiShuExp >= 0)
//                        {
//                            mishuMySQLObj.MiShuExp += mishuObj.MiShuExp;
//                        }
//                    }
//                    #region Redis更新部分
//                    var redismishuObj =ReferencePool.Accquire<MiShuDTO>();
//                    redismishuObj.MiShuLevel = mishuMySQLObj.MiShuLevel;
//                    redismishuObj.MiShuExp = mishuMySQLObj.MiShuExp;
//                    redismishuObj.MiShuID = mishuMySQLObj.MiShuID;
//                    redismishuObj.MiShuAdventureSkill = mishuSkillList;
//                    redismishuObj.MiShuSkillArry = mishuSkillList;
//                    RedisHelper.Hash.HashSet<MiShuDTO>(RedisKeyDefine._MiShuPerfix + rolemishuObj.RoleID, rolemishuObj.RoleID.ToString(), redismishuObj);
//                    #endregion
//                    mishuMySQLObj.MiShuSkillArry = Utility.Json.ToJson(mishuSkillList);
//                    NHibernateQuerier.Update<MiShu>(mishuMySQLObj);
//                    if (mishuObj.MiShuLevel > 0)
//                    {
//                        //升级发送更新的数据
//                        SetResponseParamters(() =>
//                        {
//                            subResponseParameters.Add((byte)ParameterCode.MiShu, Utility.Json.ToJson(redismishuObj));
//                            operationResponse.ReturnCode = (short)ReturnCode.Success;
//                        });
//                    }
//                    ReferencePool.Release(nHCriteriaMiShuID, redismishuObj);
//                }
//                else
//                    SetResponseParamters(() => operationResponse.ReturnCode = (short)ReturnCode.Fail);
//            }
//            ReferencePool.Release(nHCriteriaRoleID);
//            return operationResponse;
//        }
//    }
//}



