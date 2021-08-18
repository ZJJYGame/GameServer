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
    public class AddApplyForAllianceSubHandler : SyncApplyForAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleAllianceJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.ApplyForAlliance));
            var roleAllianceObj = Utility.Json.ToObject<RoleAllianceDTO>(roleAllianceJson);
            Utility.Debug.LogInfo("收到的加入仙盟的请求"+ roleAllianceJson);
            NHCriteria nHCriteriaroleAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleAllianceObj.RoleID);
            var roleAllianceTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriaroleAlliance).Result;
            List<int> applyList = new List<int>();
            List<NHCriteria> NHCriterias = new List<NHCriteria>();
            NHCriterias.Add(nHCriteriaroleAlliance);
            if (!string.IsNullOrEmpty(roleAllianceTemp.ApplyForAlliance))
            {
                applyList = Utility.Json.ToObject<List<int>>(roleAllianceTemp.ApplyForAlliance);
                if (!roleAllianceTemp.ApplyForAlliance.Equals(roleAllianceObj.RoleID.ToString()))
                {
                    for (int i = 0; i < roleAllianceObj.ApplyForAlliance.Count; i++)
                    {
                        applyList.Add(roleAllianceObj.ApplyForAlliance[i]);
                        roleAllianceTemp.ApplyForAlliance = Utility.Json.ToJson(applyList);
                     NHibernateQuerier.Update(roleAllianceTemp);
                        NHCriteria nHCriteriaroleAllianceMember =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", roleAllianceObj.ApplyForAlliance[i]);
                        NHCriterias.Add(nHCriteriaroleAllianceMember);
                        var allianceMemberTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceMember>(nHCriteriaroleAllianceMember).Result;
                        var applyer = Utility.Json.ToObject<List<int>>(allianceMemberTemp.ApplyforMember);
                        if (!applyer.Contains(roleAllianceObj.RoleID))
                        {
                            applyer.Add(roleAllianceObj.RoleID);
                        }
                        allianceMemberTemp.ApplyforMember = Utility.Json.ToJson(applyer);
                      NHibernateQuerier.Update(allianceMemberTemp);
                    }
                    SetResponseParamters(() =>
                    {
                        roleAllianceObj.ApplyForAlliance = applyList;
                        subResponseParameters.Add((byte)ParameterCode.ApplyForAlliance, Utility.Json.ToJson(roleAllianceObj));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
            }
            ReferencePool.Release(NHCriterias.ToArray());
            return operationResponse;
        }
    }
}


