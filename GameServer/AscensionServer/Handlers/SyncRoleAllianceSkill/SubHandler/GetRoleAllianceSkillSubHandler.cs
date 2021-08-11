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
using RedisDotNet;

namespace AscensionServer
{
    public class GetRoleAllianceSkillSubHandler : SyncRoleAllianceSkillSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleallianceskillJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleAllianceSkill));

            var roleallianceskillObj = Utility.Json.ToObject<RoleAllianceSkillDTO>(roleallianceskillJson);
            NHCriteria nHCriteriroleallianceskill =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleallianceskillObj.RoleID);
            Utility.Debug.LogError("收到的获得技能仙盟数据" + roleallianceskillJson);

            var roleallianceskillTemp= NHibernateQuerier.CriteriaSelectAsync<RoleAllianceSkill>(nHCriteriroleallianceskill).Result;


            if (roleallianceskillTemp!=null)
            {
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.RoleAllianceSkill, Utility.Json.ToJson(roleallianceskillTemp));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
            {
                SetResponseParamters(() =>
                {
                    operationResponse.ReturnCode = (short)ReturnCode.Fail;
                });
            }
            ReferencePool.Release(nHCriteriroleallianceskill);
            return operationResponse;
        }
    }
}


