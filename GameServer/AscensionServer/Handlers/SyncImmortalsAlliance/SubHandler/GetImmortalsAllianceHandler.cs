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

namespace AscensionServer
{
    public class GetImmortalsAllianceHandler : SyncImmortalsAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string immortalsAllianceJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Alliances));
            var immortalsAllianceObj = Utility.Json.ToObject<AlliancesDTO>
                (immortalsAllianceJson);
            var name = RedisData.ReidsDataProcessing.InsertName("(Alliance", immortalsAllianceObj.ID);
            var content = RedisData.ReidsDataProcessing.GetData(name);


            List<int> alliances = new List<int>();
            List<NHCriteria> nhcriteriaList = new List<NHCriteria>();
            List<AllianceStatusDTO> ImmortalsAllianceList = new List<AllianceStatusDTO>();
            //if (string.IsNullOrEmpty(content))
            if (true)
            {
                #region  MySql模块
                NHCriteria nHCriteriaimmortalsAllianceslist =ReferencePool.Accquire<NHCriteria>().SetValue("ID", 1);
                nhcriteriaList.Add(nHCriteriaimmortalsAllianceslist);
                var immortalsAllianceslistTemp = NHibernateQuerier.CriteriaSelect<Alliances>(nHCriteriaimmortalsAllianceslist);

                alliances = Utility.Json.ToObject<List<int>>(immortalsAllianceslistTemp.AllianceList);
                Utility.Debug.LogError("获取到的仙盟数量为" + alliances.Count);
                if (immortalsAllianceObj.Index <= alliances.Count)
                {
                    if (immortalsAllianceObj.AllIndex < alliances.Count)
                    {

                        for (int i = immortalsAllianceObj.Index; i < immortalsAllianceObj.AllIndex; i++)
                        {
                            NHCriteria nHCriteriaimmortalsAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", alliances[i]);
                            var alliancestatusTemp = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaimmortalsAlliance);
                            AllianceStatusDTO allianceStatusDTO = new AllianceStatusDTO() { ID = alliancestatusTemp.ID, AllianceLevel = alliancestatusTemp.AllianceLevel, AllianceMaster = alliancestatusTemp.AllianceMaster, AllianceName = alliancestatusTemp.AllianceName, AllianceNumberPeople = alliancestatusTemp.AllianceNumberPeople, AlliancePeopleMax = alliancestatusTemp.AlliancePeopleMax, Manifesto = alliancestatusTemp.Manifesto, Popularity = alliancestatusTemp.Popularity };
                            ImmortalsAllianceList.Add(allianceStatusDTO);
                            nhcriteriaList.Add(nHCriteriaimmortalsAlliance);
                        }
                        SetResponseParamters(() =>
                        {
                            Utility.Debug.LogError("发送的所有仙盟列表" + Utility.Json.ToJson(ImmortalsAllianceList));
                            subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(ImmortalsAllianceList));
                            operationResponse.ReturnCode = (short)ReturnCode.Success;
                        });
                    }
                    else
                    {
                        Utility.Debug.LogInfo("2开始的下标" + immortalsAllianceObj.Index + "获得的仙盟列表数据" + immortalsAllianceObj.AllIndex + "数据库的总数" + alliances.Count);
                        for (int i = immortalsAllianceObj.Index; i <alliances.Count ; i++)
                        {
                            NHCriteria nHCriteriaimmortalsAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", alliances[i]);
                            var alliancestatusTemp = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaimmortalsAlliance);
                            AllianceStatusDTO allianceStatusDTO = new AllianceStatusDTO() { ID = alliancestatusTemp.ID, AllianceLevel = alliancestatusTemp.AllianceLevel, AllianceMaster = alliancestatusTemp.AllianceMaster, AllianceName = alliancestatusTemp.AllianceName, AllianceNumberPeople = alliancestatusTemp.AllianceNumberPeople, AlliancePeopleMax = alliancestatusTemp.AlliancePeopleMax, Manifesto = alliancestatusTemp.Manifesto, Popularity = alliancestatusTemp.Popularity };
                            ImmortalsAllianceList.Add(allianceStatusDTO);
                            nhcriteriaList.Add(nHCriteriaimmortalsAlliance);

                        }
                        SetResponseParamters(() =>
                        {
                            Utility.Debug.LogError("发送的所有仙盟列表" + Utility.Json.ToJson(ImmortalsAllianceList));
                            subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(ImmortalsAllianceList));
                            operationResponse.ReturnCode = (short)ReturnCode.ItemAlreadyExists;
                        });
                    }
                }

                ReferencePool.Release(nhcriteriaList.ToArray());
                #endregion
            }
            else
            {
                alliances = Utility.Json.ToObject<List<int>>(content);
                Utility.Debug.LogError("获取到的Redis仙盟数量为" + alliances.Count);
            }
            return operationResponse;
        }
    }
}


