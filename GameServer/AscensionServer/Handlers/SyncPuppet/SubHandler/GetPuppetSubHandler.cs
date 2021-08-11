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

namespace AscensionServer.Handlers.SyncPuppet.SubHandler
{
    public class GetPuppetSubHandler : SyncPuppetSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string puppetJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobPuppet));
            var puppetObj = Utility.Json.ToObject<PuppetDTO>(puppetJson);
            NHCriteria nHCriteriapuppetObj =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", puppetObj.RoleID);
            //AscensionServer._Log.Info("得到的锻造配方" );
            var puppettemp =NHibernateQuerier.CriteriaSelect<Puppet>(nHCriteriapuppetObj);
            if (puppettemp != null)
            {
                if (!string.IsNullOrEmpty(puppettemp.Recipe_Array))
                {
                    puppetObj.JobLevel = puppettemp.JobLevel;
                    puppetObj.JobLevelExp = puppettemp.JobLevelExp;
                    puppetObj.RoleID = puppettemp.RoleID;
                    puppetObj.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(puppettemp.Recipe_Array);

                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.JobPuppet, Utility.Json.ToJson(puppetObj));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                    //AscensionServer._Log.Info("得到的锻造配方"+ Utility.Json.ToJson(Frogetemp));
                }
            }
            else
            {
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
                subResponseParameters.Add((byte)ParameterCode.JobPuppet, Utility.Json.ToJson(new List<string>()));
            }
            ReferencePool.Release(nHCriteriapuppetObj);
            return operationResponse;
        }
    }
}


