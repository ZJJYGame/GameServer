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
    public class GetSpiritualRunesSubHandler : SyncSpiritualRuneSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string spiritualrunesJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobSpiritualRunes));
            var spiritualrunesObj = Utility.Json.ToObject<SpiritualRunesDTO>(spiritualrunesJson);
            NHCriteria nHCriteriaspiritualrunes =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", spiritualrunesObj.RoleID);
            Utility.Debug.LogInfo("得到的制符配方");
            var spiritualrunestemp =NHibernateQuerier.CriteriaSelect<SpiritualRunes>(nHCriteriaspiritualrunes);
            if (spiritualrunestemp != null)
            {
                if (!string.IsNullOrEmpty(spiritualrunestemp.Recipe_Array))
                {
                    spiritualrunesObj.JobLevel = spiritualrunestemp.JobLevel;
                    spiritualrunesObj.JobLevelExp = spiritualrunestemp.JobLevelExp;
                    spiritualrunesObj.RoleID = spiritualrunestemp.RoleID;
                    spiritualrunesObj.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(spiritualrunestemp.Recipe_Array);

                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.JobSpiritualRunes, Utility.Json.ToJson(spiritualrunesObj));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                    //AscensionServer._Log.Info("得到的锻造配方"+ Utility.Json.ToJson(Frogetemp));
                }
            }
            else
            {
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
                subResponseParameters.Add((byte)ParameterCode.JobSpiritualRunes, Utility.Json.ToJson(new List<string>()));
            }
            ReferencePool.Release(nHCriteriaspiritualrunes);
            return operationResponse;
        }
    }
}


