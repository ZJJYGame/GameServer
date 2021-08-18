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
    public class GetAlchemySubHandler : SyncAlchemySubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string alchemyJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobAlchemy));
            var alchemyObj = Utility.Json.ToObject<AlchemyDTO>(alchemyJson);
            NHCriteria nHCriteriaalchemy =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", alchemyObj.RoleID);
            var alchemytemp = NHibernateQuerier.CriteriaSelect<Alchemy>(nHCriteriaalchemy);
            if (alchemytemp != null)
            {
                if (!string.IsNullOrEmpty(alchemytemp.Recipe_Array))
                {
                    SetResponseParamters(() =>
                    {
                        alchemyObj.JobLevel = alchemytemp.JobLevel;
                        alchemyObj.JobLevelExp = alchemytemp.JobLevelExp;
                        alchemyObj.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(alchemytemp.Recipe_Array);
                        alchemyObj.RoleID = alchemytemp.RoleID;

                        subResponseParameters.Add((byte)ParameterCode.JobAlchemy, Utility.Json.ToJson(alchemyObj));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
            }
            else
            {
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
               subResponseParameters.Add((byte)ParameterCode.JobAlchemy, Utility.Json.ToJson(new List<string>()));
            }
            ReferencePool.Release(nHCriteriaalchemy);
            return operationResponse;
        }


    }
}


