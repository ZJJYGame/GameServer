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
    public class UpdateAlchemySubHandler : SyncAlchemySubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string alchemyJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobAlchemy));
            var alchemyObj = Utility.Json.ToObject<AlchemyDTO>(alchemyJson);
            Utility.Debug.LogInfo("传输回去的炼丹数据" + alchemyJson);
            NHCriteria nHCriteriaAlchemy =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", alchemyObj.RoleID);
            var alchemyTemp = NHibernateQuerier.CriteriaSelect<Alchemy>(nHCriteriaAlchemy);
            int Level = 0;
            int Exp = 0;

            if (alchemyTemp!=null)
            {
                if (alchemyObj.JobLevel!=0)
                {
                    Level = alchemyTemp.JobLevel + alchemyObj.JobLevel;
                    alchemyObj = new AlchemyDTO() { RoleID = alchemyTemp.RoleID, JobLevel = Level, JobLevelExp = alchemyObj.JobLevelExp, Recipe_Array = Utility.Json.ToObject<HashSet<int>>(alchemyTemp.Recipe_Array) };

                   NHibernateQuerier.Update(alchemyObj);
                }
                else
                {
                    Exp= alchemyTemp.JobLevelExp + alchemyObj.JobLevelExp;
                    alchemyObj = new AlchemyDTO() { RoleID = alchemyTemp.RoleID, JobLevel = alchemyTemp.JobLevel, JobLevelExp = Exp, Recipe_Array = Utility.Json.ToObject<HashSet<int>>(alchemyTemp.Recipe_Array) };

                    NHibernateQuerier.Update(new Alchemy() { RoleID = alchemyTemp.RoleID, JobLevel = alchemyTemp.JobLevel, JobLevelExp = Exp, Recipe_Array = alchemyTemp.Recipe_Array });
                }
                SetResponseParamters(() =>
                {            
                    subResponseParameters.Add((byte)ParameterCode.JobAlchemy, Utility.Json.ToJson(alchemyObj));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
          
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriaAlchemy);
            return operationResponse;
        }
    }
}


