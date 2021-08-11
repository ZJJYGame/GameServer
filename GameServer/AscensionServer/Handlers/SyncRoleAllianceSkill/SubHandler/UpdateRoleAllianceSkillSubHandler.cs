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
using StackExchange.Redis;

namespace AscensionServer
{
    public class UpdateRoleAllianceSkillSubHandler : SyncRoleAllianceSkillSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            //    var dict = operationRequest.Parameters;
            //    string roleallianceskillJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleAllianceSkill));

            //    var roleallianceskillObj = Utility.Json.ToObject<RoleAllianceSkillDTO>(roleallianceskillJson);
            //    NHCriteria nHCriteriroleallianceskill =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleallianceskillObj.RoleID);

            //    var roleallianceskillTemp= NHibernateQuerier.CriteriaSelectAsync<RoleAllianceSkill>(nHCriteriroleallianceskill).Result;

            //    var rolealliancesTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriroleallianceskill).Result;
            //    var roleAssetsTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAssets>(nHCriteriroleallianceskill).Result;

            //    if (roleallianceskillTemp!=null&& rolealliancesTemp != null && roleAssetsTemp != null)
            //    {
            //        if (roleAssetsTemp.SpiritStonesLow >= roleallianceskillObj.RoleAssets && rolealliancesTemp.Reputation >= roleallianceskillObj.Contribution)
            //        {
            //            roleallianceskillTemp.SkillInsight += roleallianceskillObj.SkillInsight;
            //            roleallianceskillTemp.SkillMeditation += roleallianceskillObj.SkillMeditation;
            //            roleallianceskillTemp.SkillRapid += roleallianceskillObj.SkillRapid;
            //            roleallianceskillTemp.SkillStrong += roleallianceskillObj.SkillStrong;
            //            roleAssetsTemp.SpiritStonesLow -= roleallianceskillObj.RoleAssets;
            //            rolealliancesTemp.Reputation -= roleallianceskillObj.Contribution;

            //          NHibernateQuerier.Update(roleallianceskillTemp);
            //          NHibernateQuerier.Update(rolealliancesTemp);
            //          NHibernateQuerier.Update(roleAssetsTemp);

            //            RedisHelper.Hash.HashSet<RoleAssets>("RoleAssets", roleAssetsTemp.RoleID.ToString(), roleAssetsTemp);
            //            SetResponseParamters(() =>
            //            {
            //                subResponseParameters.Add((byte)ParameterCode.RoleAllianceSkill, Utility.Json.ToJson(roleallianceskillTemp));
            //                subResponseParameters.Add((byte)ParameterCode.RoleAssets, Utility.Json.ToJson(roleAssetsTemp));
            //                operationResponse.ReturnCode = (short)ReturnCode.Success;
            //            });
            //        }
            //        else
            //        {
            //            SetResponseParamters(() =>
            //            {
            //                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            //            });
            //        }
            //    }
            //    else
            //    {
            //        SetResponseParamters(() =>
            //        {
            //            operationResponse.ReturnCode = (short)ReturnCode.Fail;
            //        });
            //    }
            //    ReferencePool.Release(nHCriteriroleallianceskill);
            return operationResponse;
        }
    }
}


