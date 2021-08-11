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
    public class UpdateForgeSubHandler : SyncForgeSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string forgeJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobForge));
            var forgeObj = Utility.Json.ToObject<ForgeDTO>(forgeJson);
            NHCriteria nHCriteriaforge =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", forgeObj.RoleID);
            var forgeTemp = NHibernateQuerier.CriteriaSelect<Forge>(nHCriteriaforge);
            int Level = 0;
            int Exp = 0;
            //AscensionServer._Log.Info("传输回去的锻造数据" + forgeJson);
            if (forgeTemp!=null)
            {
                if (forgeObj.JobLevel != 0)
                {
                    Level = forgeTemp.JobLevel + forgeObj.JobLevel;
                    forgeObj = new ForgeDTO() { RoleID = forgeTemp.RoleID, JobLevel = Level, JobLevelExp = forgeObj.JobLevelExp, Recipe_Array = Utility.Json.ToObject<HashSet<int>>(forgeTemp.Recipe_Array) };
                    NHibernateQuerier.Update(forgeObj);
                }
                else
                {
                    Exp = forgeTemp.JobLevelExp + forgeObj.JobLevelExp;
                    forgeObj = new ForgeDTO() { RoleID = forgeTemp.RoleID, JobLevel = forgeTemp.JobLevel, JobLevelExp = Exp, Recipe_Array = Utility.Json.ToObject<HashSet<int>>(forgeTemp.Recipe_Array) };

                    NHibernateQuerier.Update(forgeObj);
                }
                SetResponseParamters(() =>
                {

                    subResponseParameters.Add((byte)ParameterCode.JobForge, Utility.Json.ToJson(forgeObj));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriaforge);
            return operationResponse;
        }
    }
}


