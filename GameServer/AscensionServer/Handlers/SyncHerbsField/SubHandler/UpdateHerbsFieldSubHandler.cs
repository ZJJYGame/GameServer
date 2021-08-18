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
    public class UpdateHerbsFieldSubHandler : SyncHerbsFieldSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string herbsfieldJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobHerbsField));
            var hfObj = Utility.Json.ToObject<HerbsFieldDTO>(herbsfieldJson);
            NHCriteria nHCriteriahf =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", hfObj.RoleID);
            Utility.Debug.LogInfo("接收到的霛田信息" + herbsfieldJson);
            var hfTemp = NHibernateQuerier.CriteriaSelect<HerbsField>(nHCriteriahf);
            List<HerbFieldStatus> hfList = new List<HerbFieldStatus>();
            if (hfTemp != null)
            {
                hfList = Utility.Json.ToObject<List<HerbFieldStatus>>(hfTemp.AllHerbs);
                if (hfList.Count<hfObj.AllHerbs[0].ArrayID)
                {
                    SetResponseParamters(() =>
                    {
                        operationResponse.ReturnCode = (byte)ReturnCode.Fail;
                        ReferencePool.Release(nHCriteriahf);
                    });                  
                }else
                {
                    for (int i = 0; i < hfObj.AllHerbs.Count; i++)
                    {
                        hfList[hfObj.AllHerbs[i].ArrayID].FieldLevel+= hfObj.AllHerbs[i].FieldLevel;
                        hfList[hfObj.AllHerbs[i].ArrayID].HerbsID= hfObj.AllHerbs[i].HerbsID;
                        hfList[hfObj.AllHerbs[i].ArrayID].HerbsGrowthValue = hfObj.AllHerbs[i].HerbsGrowthValue;
                        if (hfObj.AllHerbs[i].IsStratPlant)
                        {
                            hfList[hfObj.AllHerbs[i].ArrayID].plantingTime = hfObj.AllHerbs[i].plantingTime;
                        }
                       NHibernateQuerier.Update<HerbsField>(new HerbsField() {RoleID= hfTemp.RoleID,jobLevel= hfTemp.jobLevel,AllHerbs= Utility.Json.ToJson(hfList) });
                        for (int j = 0; j < hfObj.AllHerbs.Count; j++)
                        {
                            hfObj.AllHerbs[j].IsStratPlant = false;
                        }
                        SetResponseParamters(() =>
                        {                     
                            subResponseParameters.Add((byte)ParameterCode.RoleSchool, Utility.Json.ToJson(hfObj));
                            Utility.Debug.LogInfo("的霛田信息" + Utility.Json.ToJson(hfObj));
                            operationResponse.ReturnCode = (byte)ReturnCode.Success;
                        });
                    }            
                }
            }
            ReferencePool.Release(nHCriteriahf);
            return operationResponse;
        }
    }
}


