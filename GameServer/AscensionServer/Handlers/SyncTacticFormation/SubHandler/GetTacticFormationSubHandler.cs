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
    public class GetTacticFormationSubHandler : SyncTacticFormationSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string tacticformationJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobTacticFormation));
            var tacticformationObj = Utility.Json.ToObject<TacticFormationDTO>(tacticformationJson);
            NHCriteria nHCriteriatacticformation = ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", tacticformationObj.RoleID);
            Utility.Debug.LogInfo("得到的阵法配方" + tacticformationJson);
            var tacticformationtemp = NHibernateQuerier.CriteriaSelect<TacticFormation>(nHCriteriatacticformation);
            if (tacticformationtemp != null)
            {
                if (!string.IsNullOrEmpty(tacticformationtemp.Recipe_Array))
                {
                    tacticformationObj.JobLevel = tacticformationtemp.JobLevel;
                    tacticformationObj.JobLevelExp = tacticformationtemp.JobLevelExp;
                    tacticformationObj.RoleID = tacticformationtemp.RoleID;
                    tacticformationObj.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(tacticformationtemp.Recipe_Array);

                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.JobTacticFormation, Utility.Json.ToJson(tacticformationObj));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                    Utility.Debug.LogInfo("得到的阵法配方" + Utility.Json.ToJson(tacticformationtemp));
                }
            }
            else
            {
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
                subResponseParameters.Add((byte)ParameterCode.JobTacticFormation, Utility.Json.ToJson(new List<string>()));
            }
            ReferencePool.Release(nHCriteriatacticformation);
            return operationResponse;
        }
    }
}


