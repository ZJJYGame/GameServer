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
    public class AddHerbsFieldSubHandler : SyncHerbsFieldSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string herbsfieldJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobHerbsField));
            var hfObj = Utility.Json.ToObject<HerbsFieldDTO>(herbsfieldJson);
            NHCriteria nHCriteriahf =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", hfObj.RoleID);
            Utility.Debug.LogInfo("接收到添加的霛田信息" + herbsfieldJson);
            var hfTemp = NHibernateQuerier.CriteriaSelect<HerbsField>(nHCriteriahf);
            List<HerbFieldStatus> hfList = new List<HerbFieldStatus>();
            if (hfTemp!=null)
            {
                if (!string.IsNullOrEmpty(hfTemp.AllHerbs))
                {
                    hfList = Utility.Json.ToObject<List<HerbFieldStatus>>(hfTemp.AllHerbs);
                    if (!string.IsNullOrEmpty(Utility.Json.ToJson(hfObj.AllHerbs)))
                    {
                        for (int i = 0; i < hfObj.AllHerbs.Count; i++)
                        {
                            hfList.Add(hfObj.AllHerbs[i]);
                        }
                        hfTemp.AllHerbs = Utility.Json.ToJson(hfList);
                    }
                    else
                    {
                        hfList = hfObj.AllHerbs;
                        hfTemp.AllHerbs = Utility.Json.ToJson(hfList);
                    }
                    NHibernateQuerier.Update(hfTemp);
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.JobHerbsField, Utility.Json.ToJson(new HerbsFieldDTO() {AllHerbs= hfList,jobLevel= hfTemp.jobLevel,RoleID= hfTemp.RoleID }));
                        operationResponse.ReturnCode = (byte)ReturnCode.Success;
                    });
                }
            }
            ReferencePool.Release(nHCriteriahf);
            return operationResponse;
        }
    }
}


