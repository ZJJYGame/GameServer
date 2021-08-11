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

namespace AscensionServer.Handlers
{
    public class GetHerbsFieldSubHandler : SyncHerbsFieldSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string herbsfieldJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobHerbsField));
            var hfObj = Utility.Json.ToObject<HerbsField>(herbsfieldJson);

            NHCriteria nHCriteriahf =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", hfObj.RoleID);
            Utility.Debug.LogInfo("接收到的霛田信息"+ herbsfieldJson);
            var hfTemp = NHibernateQuerier.CriteriaSelect<HerbsField>(nHCriteriahf);
            if (hfTemp!=null)
            {
                SetResponseParamters(() =>
                {
                    Utility.Debug.LogInfo("發送的霛田信息" + herbsfieldJson);
                    HerbsFieldDTO herbsFieldDTO = new HerbsFieldDTO() { RoleID= hfTemp .RoleID,jobLevel= hfTemp .jobLevel};
                    herbsFieldDTO.AllHerbs = Utility.Json.ToObject<List<HerbFieldStatus>>(hfTemp.AllHerbs);
                    subResponseParameters.Add((byte)ParameterCode.JobHerbsField, Utility.Json.ToJson(herbsFieldDTO));
                    operationResponse.ReturnCode = (byte)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (byte)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriahf);
            return operationResponse;
        }
    }
}


