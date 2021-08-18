//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AscensionProtocol;
//using AscensionProtocol.DTO;
//using Photon.SocketServer;
//using AscensionServer.Model;
//using Cosmos;
//using RedisDotNet;
//using StackExchange.Redis;

//namespace AscensionServer.Handlers
//{
//    public class UpdateAllianceConstructionSubHandler : SyncAllianceConstructionSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string allianceConstructionJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AllianceConstruction));

//           // Utility.Debug.LogError("yzq仙盟升级数据接收成功为" + allianceConstructionJson);
//            var allianceConstructionObj = Utility.Json.ToObject<AllianceConstructionDTO>(allianceConstructionJson);
//            NHCriteria nHCriteriallianceConstruction =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", allianceConstructionObj.AllianceID);

//            NHCriteria nHCriterialliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", allianceConstructionObj.AllianceID);

//            var allianceConstructionTemp= NHibernateQuerier.CriteriaSelectAsync<AllianceConstruction>(nHCriteriallianceConstruction).Result;
//            var allianceStatusTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceStatus>(nHCriterialliance).Result;

//            if (allianceConstructionTemp != null)
//            {
//                if (allianceConstructionObj.AllianceArmsDrillSite>0)
//                {
//                    if (allianceConstructionTemp .AllianceAssets> allianceConstructionObj.AllianceAssets)
//                    {
//                        allianceConstructionTemp.AllianceArmsDrillSite += allianceConstructionObj.AllianceArmsDrillSite;
//                        allianceConstructionTemp.AllianceAssets -= allianceConstructionObj.AllianceAssets;
//                        NHibernateQuerier.Update(allianceConstructionTemp);
//                        SetResponseParamters(() =>
//                        {
//                          //  Utility.Debug.LogError("yzq1发送的升级仙盟数据为" + Utility.Json.ToJson(allianceConstructionTemp));
//                            subResponseParameters.Add((byte)ParameterCode.AllianceConstruction, Utility.Json.ToJson(allianceConstructionTemp));
//                            operationResponse.ReturnCode = (short)ReturnCode.Success;
//                        });
//                    }
//                    else
//                    {
//                        SetResponseParamters(() =>
//                        {
//                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
//                        });
//                    }
//                }
//                if (allianceConstructionObj.AllianceAlchemyStorage > 0)
//                {
//                    if (allianceConstructionTemp.AllianceAssets > allianceConstructionObj.AllianceAssets)
//                    {
//                        allianceConstructionTemp.AllianceAlchemyStorage += allianceConstructionObj.AllianceAlchemyStorage;
//                        allianceConstructionTemp.AllianceAssets -= allianceConstructionObj.AllianceAssets;
//                        NHibernateQuerier.Update(allianceConstructionTemp);
//                        SetResponseParamters(() =>
//                        {
//                          //  Utility.Debug.LogError("yzq2发送的升级仙盟数据为" + Utility.Json.ToJson(allianceConstructionTemp));
//                            subResponseParameters.Add((byte)ParameterCode.AllianceConstruction, Utility.Json.ToJson(allianceConstructionTemp));
//                            operationResponse.ReturnCode = (short)ReturnCode.Success;
//                        });
//                    }
//                    else
//                    {
//                        SetResponseParamters(() =>
//                        {
//                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
//                        });
//                    }
//                }
//                if (allianceConstructionObj.AllianceScripturesPlatform > 0)
//                {
//                    if (allianceConstructionTemp.AllianceAssets > allianceConstructionObj.AllianceAssets)
//                    {
//                        allianceConstructionTemp.AllianceScripturesPlatform += allianceConstructionObj.AllianceScripturesPlatform;
//                        allianceConstructionTemp.AllianceAssets -= allianceConstructionObj.AllianceAssets;
//                        NHibernateQuerier.Update(allianceConstructionTemp);
//                        SetResponseParamters(() =>
//                        {
//                           // Utility.Debug.LogError("yzq3发送的升级仙盟数据为" + Utility.Json.ToJson(allianceConstructionTemp));
//                            subResponseParameters.Add((byte)ParameterCode.AllianceConstruction, Utility.Json.ToJson(allianceConstructionTemp));
//                            operationResponse.ReturnCode = (short)ReturnCode.Success;
//                        });
//                    }
//                    else
//                    {
//                        SetResponseParamters(() =>
//                        {
//                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
//                        });
//                    }
//                }
//                if (allianceConstructionObj.AllianceChamber > 0)
//                {
//                    if (allianceConstructionTemp.AllianceChamber == allianceConstructionTemp.AllianceArmsDrillSite&& allianceConstructionTemp.AllianceChamber == allianceConstructionTemp.AllianceAlchemyStorage&& allianceConstructionTemp.AllianceChamber == allianceConstructionTemp.AllianceScripturesPlatform&& allianceConstructionTemp.AllianceAssets >=allianceConstructionObj.AllianceAssets)
//                    {
//                        allianceConstructionTemp.AllianceChamber += allianceConstructionObj.AllianceChamber;
//                        allianceConstructionTemp.AllianceAssets -= allianceConstructionObj.AllianceAssets;
//                        NHibernateQuerier.Update(allianceConstructionTemp);
//                        allianceStatusTemp.AllianceLevel += 1;
//                        //GameEntry. DataManager.TryGetValue<Dictionary<int, AllianceLevleUpData>>(out var allianceDict);
//                        //allianceStatusTemp.AlliancePeopleMax = allianceDict[allianceStatusTemp.AllianceLevel].LevelUp_Describe;
//                        NHibernateQuerier.Update(allianceStatusTemp);

//                        SetResponseParamters(() =>
//                        {
//                          //  Utility.Debug.LogError("yzq4发送的升级仙盟数据为" + Utility.Json.ToJson(allianceConstructionTemp));
//                            subResponseParameters.Add((byte)ParameterCode.AllianceConstruction, Utility.Json.ToJson(allianceConstructionTemp));
//                            operationResponse.ReturnCode = (short)ReturnCode.Success;
//                        });
//                    }
//                    else
//                    {
//                        SetResponseParamters(() =>
//                        {
//                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
//                        });
//                    }
//                }
//            }

//            ReferencePool.Release(nHCriteriallianceConstruction, nHCriterialliance);
//            return operationResponse;
//        }
//    }
//}


