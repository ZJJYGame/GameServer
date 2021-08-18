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
    public class GetForgeSubHandler : SyncForgeSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string forgeJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobForge));
            var forgeObj = Utility.Json.ToObject<ForgeDTO>(forgeJson);
            NHCriteria nHCriteriaFroge =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", forgeObj.RoleID);
            //AscensionServer._Log.Info("得到的锻造配方" );
            var Frogetemp= NHibernateQuerier.CriteriaSelect<Forge>(nHCriteriaFroge);
            if (Frogetemp != null)
            {
                if (!string.IsNullOrEmpty(Frogetemp.Recipe_Array))
                {
                    forgeObj.JobLevel = Frogetemp.JobLevel;
                    forgeObj.JobLevelExp = Frogetemp.JobLevelExp;
                    forgeObj.RoleID = Frogetemp.RoleID;
                    forgeObj.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(Frogetemp.Recipe_Array);
                    SetResponseParamters(() =>
                    {   
                        subResponseParameters.Add((byte)ParameterCode.JobForge, Utility.Json.ToJson(forgeObj));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                    //AscensionServer._Log.Info("得到的锻造配方"+ Utility.Json.ToJson(Frogetemp));
                }
            }
            else
            {
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
                subResponseParameters.Add((byte)ParameterCode.JobForge, Utility.Json.ToJson(new List<string>()));
            }
            ReferencePool.Release(nHCriteriaFroge);
            return operationResponse;
        }
    }
}


