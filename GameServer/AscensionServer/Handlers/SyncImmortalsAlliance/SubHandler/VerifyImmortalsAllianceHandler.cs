using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;
using StackExchange.Redis;
using NHibernate.Criterion;
namespace AscensionServer
{
    public class VerifyImmortalsAllianceHandler : SyncImmortalsAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Verify;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string alliancestatusJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Alliances));
            var alliancestatusObj = Utility.Json.ToObject<AllianceStatusDTO>
                (alliancestatusJson);
            List<AllianceStatusDTO> allianceStatusDTOs = new List<AllianceStatusDTO>();
            List<NHCriteria> nHCriterias = new List<NHCriteria>();
            if (alliancestatusObj.ID==0)
            {
                NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", alliancestatusObj.AllianceName);
                nHCriterias.Add(nHCriteriaAllianceName);
                var allianceNameObj = NHibernateQuerier.CriteriaLikeAsync<AllianceStatus>(nHCriteriaAllianceName, MatchMode.Anywhere).Result;
                Utility.Debug.LogError("1查询获得MySQL的数据" + allianceNameObj.Count);
                if (allianceNameObj.Count == 0)
                {
                    SetResponseParamters(() =>
                    {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                }
                else
                {
                    for (int i = 0; i < allianceNameObj.Count; i++)
                    {
                        AllianceStatusDTO allianceStatusDTO =ReferencePool.Accquire<AllianceStatusDTO>();
                        allianceStatusDTO.ID = allianceNameObj[i].ID;
                        allianceStatusDTO.AllianceLevel = allianceNameObj[i].AllianceLevel;
                        allianceStatusDTO.AllianceMaster = allianceNameObj[i].AllianceMaster;
                        allianceStatusDTO.AllianceName = allianceNameObj[i].AllianceName;
                        allianceStatusDTO.AlliancePeopleMax = allianceNameObj[i].AlliancePeopleMax;
                        allianceStatusDTO.AllianceNumberPeople = allianceNameObj[i].AllianceNumberPeople;
                        allianceStatusDTO.Manifesto = allianceNameObj[i].Manifesto;
                        allianceStatusDTO.Popularity = allianceNameObj[i].Popularity;
                        allianceStatusDTOs.Add(allianceStatusDTO);

                    }
                    SetResponseParamters(() =>
                    {
                        Utility.Debug.LogInfo("发送的所有仙盟列表" + Utility.Json.ToJson(allianceStatusDTOs));
                        subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(allianceStatusDTOs));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
            }
            else
            {

                NHCriteria nHCriteriaAllianceID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", alliancestatusObj.ID);
                nHCriterias.Add(nHCriteriaAllianceID);
                var allianceIDObj = NHibernateQuerier.CriteriaSelectAsync<AllianceStatus>(nHCriteriaAllianceID).Result;
                Utility.Debug.LogError("1搜索的是仙盟名字不是id");
                if (allianceIDObj == null)
                {
                    Utility.Debug.LogError("2搜索的是仙盟名字不是id");
                    NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", alliancestatusObj.AllianceName.ToString());
                    nHCriterias.Add(nHCriteriaAllianceName);
                    var allianceNameObj = NHibernateQuerier.CriteriaLikeAsync<AllianceStatus>(nHCriteriaAllianceName, MatchMode.Anywhere).Result;
                    //Utility.Debug.LogError("1查询获得MySQL的数据" + allianceNameObj[0].AllianceName + "%%" + allianceNameObj[2].AllianceName + "**" + "查询获得MySQL的数据长度" + allianceNameObj.Count);
                    if (allianceNameObj.Count == 0)
                    {
                        SetResponseParamters(() =>
                        {
                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
                        });
                    }
                    else
                    {
                        for (int i = 0; i < allianceNameObj.Count; i++)
                        {
                            AllianceStatusDTO allianceStatusDTO =ReferencePool.Accquire<AllianceStatusDTO>();
                            allianceStatusDTO.ID = allianceNameObj[i].ID;
                            allianceStatusDTO.AllianceLevel = allianceNameObj[i].AllianceLevel;
                            allianceStatusDTO.AllianceMaster = allianceNameObj[i].AllianceMaster;
                            allianceStatusDTO.AllianceName = allianceNameObj[i].AllianceName;
                            allianceStatusDTO.AllianceNumberPeople = allianceNameObj[i].AllianceNumberPeople;
                            allianceStatusDTO.AlliancePeopleMax = allianceNameObj[i].AlliancePeopleMax;
                            allianceStatusDTO.Manifesto = allianceNameObj[i].Manifesto;
                            allianceStatusDTO.Popularity = allianceNameObj[i].Popularity;
                            allianceStatusDTOs.Add(allianceStatusDTO);

                        }
                        SetResponseParamters(() =>
                        {
                            Utility.Debug.LogInfo("发送的所有仙盟列表" + Utility.Json.ToJson(allianceStatusDTOs));
                            subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(allianceStatusDTOs));
                            operationResponse.ReturnCode = (short)ReturnCode.Success;
                        });
                    }
                }
                else
                {

                    AllianceStatusDTO allianceStatusDTO =ReferencePool.Accquire<AllianceStatusDTO>();
                    allianceStatusDTO.ID = allianceIDObj.ID;
                    allianceStatusDTO.AllianceLevel = allianceIDObj.AllianceLevel;
                    allianceStatusDTO.AllianceMaster = allianceIDObj.AllianceMaster;
                    allianceStatusDTO.AllianceName = allianceIDObj.AllianceName;
                    allianceStatusDTO.AlliancePeopleMax = allianceIDObj.AlliancePeopleMax;
                    allianceStatusDTO.AllianceNumberPeople = allianceIDObj.AllianceNumberPeople;
                    allianceStatusDTO.Manifesto = allianceIDObj.Manifesto;
                    allianceStatusDTO.Popularity = allianceIDObj.Popularity;
                    allianceStatusDTOs.Add(allianceStatusDTO);

                    NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", alliancestatusObj.AllianceName);
                    nHCriterias.Add(nHCriteriaAllianceName);
                    var allianceNameObj = NHibernateQuerier.CriteriaLikeAsync<AllianceStatus>(nHCriteriaAllianceName, MatchMode.Anywhere).Result;
                    Utility.Debug.LogError("3搜索的是仙盟名字不是id");
                    if (allianceNameObj.Count == 0)
                    {
                        SetResponseParamters(() =>
                        {
                            Utility.Debug.LogInfo("发送的所有仙盟列表" + Utility.Json.ToJson(allianceStatusDTOs));
                            subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(allianceStatusDTOs));
                            operationResponse.ReturnCode = (short)ReturnCode.Success;
                        });
                    }
                    else
                    {
                        for (int i = 0; i < allianceNameObj.Count; i++)
                        {
                            if (allianceStatusDTO.ID!= allianceNameObj[i].ID)
                            {
                                AllianceStatusDTO allianceStatusNameDTO =ReferencePool.Accquire<AllianceStatusDTO>();
                                allianceStatusNameDTO.ID = allianceNameObj[i].ID;
                                allianceStatusNameDTO.AllianceLevel = allianceNameObj[i].AllianceLevel;
                                allianceStatusNameDTO.AllianceMaster = allianceNameObj[i].AllianceMaster;
                                allianceStatusNameDTO.AllianceName = allianceNameObj[i].AllianceName;
                                allianceStatusNameDTO.AlliancePeopleMax = allianceNameObj[i].AlliancePeopleMax;
                                allianceStatusNameDTO.AllianceNumberPeople = allianceNameObj[i].AllianceNumberPeople;
                                allianceStatusNameDTO.Manifesto = allianceNameObj[i].Manifesto;
                                allianceStatusNameDTO.Popularity = allianceNameObj[i].Popularity;
                                allianceStatusDTOs.Add(allianceStatusNameDTO);
                            }

                        }
                        SetResponseParamters(() =>
                        {
                            Utility.Debug.LogInfo("发送的所有仙盟列表" + Utility.Json.ToJson(allianceStatusDTOs));
                            subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(allianceStatusDTOs));
                            operationResponse.ReturnCode = (short)ReturnCode.Success;
                        });
                    }
                }
            }
           ReferencePool.Release(nHCriterias.ToArray());
            return operationResponse;
        }
    }
}



