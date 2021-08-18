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
//    public class AddMiShuSubHandler : SyncMiShuSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string msJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.MiShu));
//            string roleJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Role));

//            var roleObj = Utility.Json.ToObject<RoleDTO>(roleJson);
//            var mishuObj = Utility.Json.ToObject<MiShu>(msJson);
//            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleObj.RoleID);
//            var roleMiShuObj = NHibernateQuerier.CriteriaSelect<RoleMiShu>(nHCriteriaRoleID);
//            //if (roleMiShuObj==null)
//            //{
//            //    roleMiShuObj =ReferencePool.Accquire<RoleMiShu>();
//            //    roleMiShuObj = NHibernateQuerier.Insert<RoleMiShu>(roleMiShuObj);
//            //}
//            GameEntry. DataManager.TryGetValue<Dictionary<int, MiShuData>>(out var mishuDataDict);

//            #region 背包验证逻辑
//            var ringObj =ReferencePool.Accquire<RingDTO>();
//            ringObj.RingItems = new Dictionary<int, RingItemsDTO>();
//            ringObj.RingItems.Add(mishuObj.MiShuID, new RingItemsDTO());
//            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteriaRoleID);
//            var nHCriteriaRingID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);
//            #endregion

//            //if (InventoryManager.VerifyIsExist(mishuObj.MiShuID, nHCriteriaRingID))
//            //{
//            //    var mishuTemp =ReferencePool.Accquire<MiShuDTO>();

//            //    var result = MishuStudyHelper.AddMishuJuge(mishuObj.MiShuID, roleObj, out mishuTemp);
//            //    if (!result)
//            //    {
//            //        Utility.Debug.LogInfo("1添加的新的秘术为yzqData" + msJson);
//            //        SetResponseParamters(() =>
//            //        {
//            //            operationResponse.ReturnCode = (short)ReturnCode.Fail;
//            //        });
//            //    }
//            //    else
//            //    {
//            //        if (roleMiShuObj != null)
//            //        {
//            //            var roleMishuDict = Utility.Json.ToObject<Dictionary<int, int>>(roleMiShuObj.MiShuIDArray);



//            //        }
//            //        else
//            //        {
//            //            SetResponseParamters(() =>
//            //            {
//            //                operationResponse.ReturnCode = (short)ReturnCode.Fail;
//            //            });
//            //        }
//            //    }
//            //}


//            #region 待删
//            var rolemishuredisObj = RedisHelper.Hash.HashGet<RoleMiShuDTO>(RedisKeyDefine._MiShuPerfix, roleObj.RoleID.ToString());
//            var roleMishuMySQL = Utility.Json.ToObject<List<int>>(roleMiShuObj.MiShuIDDict);
//            if (!roleMishuMySQL.Contains(mishuObj.MiShuID))
//            {
//                #region 生成新的秘术
//                var mishuRedisObj =ReferencePool.Accquire<MiShuDTO>();
//                var mishuMysqlObj =ReferencePool.Accquire<MiShu>();
//                mishuMysqlObj.MiShuID = mishuObj.MiShuID;
//                mishuMysqlObj = NHibernateQuerier.InsertAsync<MiShu>(mishuMysqlObj).Result;

//                mishuRedisObj.MiShuID = mishuMysqlObj.MiShuID;

//                if (mishuDataDict[mishuObj.MiShuID].mishuSkillDatas[0].SkillArrayOne.Count > 0)
//                {
//                    mishuRedisObj.MiShuSkillArry = new List<int>() { mishuDataDict[mishuObj.MiShuID].mishuSkillDatas[0].SkillArrayOne[0] };
//                }
//                if (mishuDataDict[mishuObj.MiShuID].mishuSkillDatas[0].SkillArrayTwo.Count > 0)
//                {
//                    mishuRedisObj.MiShuAdventureSkill = new List<int>() { mishuDataDict[mishuObj.MiShuID].mishuSkillDatas[0].SkillArrayTwo[0] };
//                }
//                mishuMysqlObj.MiShuAdventtureSkill = Utility.Json.ToJson(mishuRedisObj.MiShuAdventureSkill);
//                mishuMysqlObj.MiShuSkillArry = Utility.Json.ToJson(mishuRedisObj.MiShuSkillArry);

//                #endregion
//                //Redis存在先存
//                if (rolemishuredisObj != null)
//                {
//                    if (!rolemishuredisObj.MiShuIDArray.ContainsKey(mishuObj.MiShuID))
//                    {
//                        NHibernateQuerier.UpdateAsync(mishuMysqlObj);
//                        RedisHelper.Hash.HashSetAsync<MiShuDTO>(RedisKeyDefine._MiShuPerfix + roleObj.RoleID, roleObj.RoleID.ToString(), mishuRedisObj);

//                        rolemishuredisObj.MiShuIDArray.Add(mishuRedisObj.MiShuID, mishuRedisObj.MiShuID);
//                        RedisHelper.Hash.HashSetAsync<RoleMiShuDTO>(RedisKeyDefine._RoleMiShuPerfix + roleObj.RoleID, roleObj.RoleID.ToString(), rolemishuredisObj);

//                        roleMiShuObj.MiShuIDArray = Utility.Json.ToJson(rolemishuredisObj);
//                        NHibernateQuerier.UpdateAsync(roleMiShuObj);
//                    }
//                }
//                //Redis没有先存MySQL
//                else
//                {
//                    NHibernateQuerier.UpdateAsync(mishuMysqlObj);
//                    RedisHelper.Hash.HashSetAsync<MiShuDTO>(RedisKeyDefine._MiShuPerfix + roleObj.RoleID, roleObj.RoleID.ToString(), mishuRedisObj);

//                    rolemishuredisObj =ReferencePool.Accquire<RoleMiShuDTO>();
//                    rolemishuredisObj.MiShuIDArray = new Dictionary<int, int>() { };
//                    rolemishuredisObj.MiShuIDArray.Add(mishuRedisObj.MiShuID, mishuRedisObj.MiShuID);
//                    RedisHelper.Hash.HashSetAsync<RoleMiShuDTO>(RedisKeyDefine._RoleMiShuPerfix + roleObj.RoleID, roleObj.RoleID.ToString(), rolemishuredisObj);

//                    roleMiShuObj.MiShuIDArray = Utility.Json.ToJson(rolemishuredisObj);
//                    NHibernateQuerier.UpdateAsync(roleMiShuObj);
//                }
//                SetResponseParamters(() =>
//                {
//                    subResponseParameters.Add((byte)ParameterCode.MiShu, Utility.Json.ToJson(mishuRedisObj));
//                    subResponseParameters.Add((byte)ParameterCode.RoleMiShu, Utility.Json.ToJson(rolemishuredisObj));
//                    operationResponse.ReturnCode = (byte)ReturnCode.Success;
//                });
//                ReferencePool.Release(mishuRedisObj, mishuMysqlObj, nHCriteriaRoleID);
//            }
//            else
//            {
//                SetResponseParamters(() =>
//                {
//                    operationResponse.ReturnCode = (byte)ReturnCode.Fail;
//                });
//            }
//            #endregion
//            return operationResponse;
//        }

//    }
//}


