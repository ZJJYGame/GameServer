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
namespace AscensionServer
{
    public class GetAllianceConstructionSubHandler : SyncAllianceConstructionSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            {
                var dict = operationRequest.Parameters;
                string allianceConstructionJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AllianceConstruction));

                var allianceConstructionObj = Utility.Json.ToObject<AllianceConstructionDTO>(allianceConstructionJson);
                NHCriteria nHCriteriallianceConstruction =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", allianceConstructionObj.AllianceID);

                Utility.Debug.LogError("yzqData獲得的得到的仙盟建設" + allianceConstructionObj.AllianceID);
                var allianceConstructionTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceConstruction>(nHCriteriallianceConstruction).Result;
                Utility.Debug.LogError("yzqData2獲得的得到的仙盟建設" + Utility.Json.ToJson(allianceConstructionTemp));


                var allianceTemp = AlliancelogicManager.Instance.GetNHCriteria<AllianceStatus>("ID", allianceConstructionObj.AllianceID);

                if (allianceConstructionTemp != null)
                {
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.AllianceConstruction, Utility.Json.ToJson(allianceConstructionTemp));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });             
                }
                else
                {
                    SetResponseParamters(() =>
                    {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                }
                ReferencePool.Release(nHCriteriallianceConstruction);
                return operationResponse;
            }
        }
    }
}


