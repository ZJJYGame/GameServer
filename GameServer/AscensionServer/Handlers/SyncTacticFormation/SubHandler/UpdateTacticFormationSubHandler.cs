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
    public class UpdateTacticFormationSubHandler : SyncTacticFormationSubHandler

    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string tacticformationJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobTacticFormation));
            var tacticformationObj = Utility.Json.ToObject<TacticFormationDTO>(tacticformationJson);
            NHCriteria  nHCriteriatacticformation =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", tacticformationObj.RoleID);
            var tacticformationTemp =NHibernateQuerier.CriteriaSelect<TacticFormation>(nHCriteriatacticformation);
            int Level = 0;
            int Exp = 0;
            //AscensionServer._Log.Info("传输回去的锻造数据" + forgeJson);

            if (tacticformationTemp != null)
            {
                if (tacticformationObj.JobLevel != 0)
                {
                    Level = tacticformationTemp.JobLevel + tacticformationObj.JobLevel;
                    tacticformationObj = new TacticFormationDTO() { RoleID = tacticformationTemp.RoleID, JobLevel = Level, JobLevelExp = tacticformationObj.JobLevelExp, Recipe_Array = Utility.Json.ToObject<HashSet<int>>(tacticformationTemp.Recipe_Array) };
                    NHibernateQuerier.Update(tacticformationObj);

                }
                else
                {
                    Exp = tacticformationTemp.JobLevelExp + tacticformationObj.JobLevelExp;
                    tacticformationObj = new TacticFormationDTO() { RoleID = tacticformationTemp.RoleID, JobLevel = tacticformationTemp.JobLevel, JobLevelExp = Exp, Recipe_Array =Utility.Json.ToObject<HashSet<int>>(tacticformationTemp.Recipe_Array) };
                    NHibernateQuerier.Update(tacticformationObj);
                }
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.JobTacticFormation, Utility.Json.ToJson(tacticformationObj));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriatacticformation);
            return operationResponse;
        }
    }
}


