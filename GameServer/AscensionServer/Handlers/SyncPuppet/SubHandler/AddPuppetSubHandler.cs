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
    public class AddPuppetSubHandler : SyncPuppetSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string puppetJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobPuppet));
            Utility.Debug.LogInfo("得到的傀儡为" + puppetJson);
            var puppetObj = Utility.Json.ToObject<PuppetDTO>(puppetJson);
            NHCriteria nHCriteriaPuppet =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", puppetObj.RoleID);
            var puppetTemp = NHibernateQuerier.CriteriaSelect<Puppet>(nHCriteriaPuppet);
            HashSet<int> puppetHash = new HashSet<int>();
            if (puppetTemp!=null)
            {
                if (string.IsNullOrEmpty(puppetTemp.Recipe_Array))
                {
                    puppetTemp.Recipe_Array = Utility.Json.ToJson(puppetObj.Recipe_Array);
                    NHibernateQuerier.Update(puppetTemp);
                }
                else
                {
                    puppetHash = Utility.Json.ToObject<HashSet<int>>(puppetTemp.Recipe_Array);
                    puppetHash.Add(puppetObj.Recipe_Array.First());
                    puppetTemp.Recipe_Array = Utility.Json.ToJson(puppetHash);
                    NHibernateQuerier.Update(puppetTemp);
                }
                SetResponseParamters(() =>
                {
                    puppetObj = new PuppetDTO() { RoleID = puppetTemp.RoleID, JobLevel = puppetTemp.JobLevel, JobLevelExp = puppetTemp.JobLevelExp, Recipe_Array = puppetHash };

                    subResponseParameters.Add((byte)ParameterCode.JobPuppet, puppetObj);
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriaPuppet);
            return operationResponse;
        }
    }
}


