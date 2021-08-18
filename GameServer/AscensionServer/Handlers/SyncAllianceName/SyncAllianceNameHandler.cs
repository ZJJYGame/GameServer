using AscensionProtocol;
using Photon.SocketServer;
using System;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using RedisDotNet;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace AscensionServer
{
    public class SyncAllianceNameHandler : Handler
    {
        public override byte OpCode { get { return (byte)OperationCode.SyncAllianceName; } }
        protected override OperationResponse OnOperationRequest(OperationRequest operationRequest)
        {
            responseParameters.Clear();
            var allianceJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.AllianceName));
            var roleAssetsJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.RoleAssets));

            var allianceStatusObj = Utility.Json.ToObject<AllianceStatusDTO>(allianceJson);
            var roleAssetsObj = Utility.Json.ToObject<RoleAssetsDTO>(roleAssetsJson);

            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", allianceStatusObj.AllianceName);
            var allianceNameTemp = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaAlliance);
            var allianceStatusTemp = AlliancelogicManager.Instance.GetNHCriteria<AllianceStatus>("ID", allianceStatusObj.ID);
            var roleAssetsTemp = AlliancelogicManager.Instance.GetNHCriteria<RoleAssets>("RoleID", roleAssetsObj.RoleID);
            operationResponse.OperationCode = operationRequest.OperationCode;
            if (roleAssetsTemp != null)
            {
                if (roleAssetsTemp.SpiritStonesLow >= roleAssetsObj.SpiritStonesLow)
                {

                    if (allianceStatusTemp != null && allianceNameTemp == null)
                    {
                        allianceStatusTemp.AllianceName = allianceStatusObj.AllianceName;
                        roleAssetsTemp.SpiritStonesLow -= roleAssetsObj.SpiritStonesLow;
                       NHibernateQuerier.Update(allianceStatusTemp);
                        NHibernateQuerier.Update(roleAssetsTemp);

                        RedisHelper.Hash.HashSet<RoleAssets>("RoleAssets", roleAssetsObj.RoleID.ToString(), new RoleAssets() { RoleID = roleAssetsObj.RoleID, SpiritStonesLow = roleAssetsTemp.SpiritStonesLow, XianYu = roleAssetsTemp.XianYu });
                        responseParameters.Add((byte)ParameterCode.AllianceName, Utility.Json.ToJson(allianceStatusTemp));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;//返回成功
                        operationResponse.Parameters = responseParameters;
                    }
                    else
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                }
                else
                    operationResponse.ReturnCode = (short)ReturnCode.Fail;
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;

            ReferencePool.Release(nHCriteriaAlliance);
            return operationResponse;
        }
    }
}


