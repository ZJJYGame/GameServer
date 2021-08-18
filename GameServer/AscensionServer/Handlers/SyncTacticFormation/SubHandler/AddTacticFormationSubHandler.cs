using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class AddTacticFormationSubHandler : SyncTacticFormationSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string tacticFormationJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobTacticFormation));
            Utility.Debug.LogInfo("得到的阵法为"+tacticFormationJson);
            var tacticFormationObj = Utility.Json.ToObject<TacticFormationDTO>(tacticFormationJson);
            NHCriteria nHCriteriatacticFormation =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", tacticFormationObj.RoleID);
            var tacticFormatioTemp =NHibernateQuerier.CriteriaSelect<TacticFormation>(nHCriteriatacticFormation);
            HashSet<int> tacticFormationHash = new HashSet<int>();
            if (tacticFormatioTemp!=null)
            {
                if (string .IsNullOrEmpty(tacticFormatioTemp.Recipe_Array))
                {
                    tacticFormatioTemp.Recipe_Array = Utility.Json.ToJson(tacticFormationObj.Recipe_Array);
                    NHibernateQuerier.Update(tacticFormatioTemp);
                }
                else
                {
                    tacticFormationHash = Utility.Json.ToObject<HashSet<int>>(tacticFormatioTemp.Recipe_Array);
                    tacticFormationHash.Add(tacticFormationObj.Recipe_Array.First());
                    tacticFormatioTemp.Recipe_Array = Utility.Json.ToJson(tacticFormationHash);
                    NHibernateQuerier.Update(tacticFormatioTemp);
                }
                SetResponseParamters(() =>
                {
                    tacticFormationObj = new TacticFormationDTO() { RoleID = tacticFormatioTemp.RoleID, JobLevel = tacticFormatioTemp.JobLevel, JobLevelExp = tacticFormatioTemp.JobLevelExp, Recipe_Array = tacticFormationHash };
                    subResponseParameters.Add((byte)ParameterCode.JobTacticFormation, tacticFormationObj);
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriatacticFormation);
            return operationResponse;
        }
    }
}


