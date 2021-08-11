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
   public class GetAllianceMemberSubHandler : SyncAllianceMemberSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        List<NHCriteria> nHCriterias = new List<NHCriteria>();

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string allianceMemberJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AllianceMember));
            var allianceMemberObj = Utility.Json.ToObject<AllianceMemberDTO>(allianceMemberJson);
            NHCriteria nHCriteriallianceMember =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", allianceMemberObj.AllianceID);
            var allianceMemberTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceMember>(nHCriteriallianceMember).Result;
            Utility.Debug.LogInfo("发送的仙盟的所有成员" + allianceMemberTemp.Member);
            List<int> memberList = new List<int>();
            List<RoleAllianceDTO> allianceMembers = new List<RoleAllianceDTO>();
            nHCriterias.Clear();
            if (allianceMemberTemp != null)
            {
                if (!string.IsNullOrEmpty(allianceMemberTemp.Member))
                {
                    memberList = Utility.Json.ToObject<List<int>>(allianceMemberTemp.Member);
                    for (int i = 0; i < memberList.Count; i++)
                    {
                        NHCriteria nHCriteriMember =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", memberList[i]);
                        nHCriterias.Add(nHCriteriMember);
                       var RoleSchol= AlliancelogicManager.Instance.GetNHCriteria<RoleSchool>("RoleID", memberList[i]);
                        var Role = AlliancelogicManager.Instance.GetNHCriteria<Role>("RoleID", memberList[i]);
                        var School = AlliancelogicManager.Instance.GetNHCriteria<School>("ID", RoleSchol.RoleJoiningSchool);
                        var MemberTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriMember).Result;
                        RoleAllianceDTO roleAllianceDTO = new RoleAllianceDTO() { AllianceID = MemberTemp.AllianceID, AllianceJob = MemberTemp.AllianceJob, JoinTime = MemberTemp.JoinTime, ApplyForAlliance = Utility.Json.ToObject<List<int>>(MemberTemp.ApplyForAlliance), Offline = MemberTemp.Offline, Reputation = MemberTemp.Reputation, ReputationHistroy = MemberTemp.ReputationHistroy, ReputationMonth = MemberTemp.ReputationMonth, RoleID = MemberTemp.RoleID, RoleName = MemberTemp.RoleName,RoleLevel= Role .RoleLevel};
                        allianceMembers.Add(roleAllianceDTO);
                    }

                    SetResponseParamters(() =>
                    {
                        Utility.Debug.LogInfo("发送的仙盟的所有成员"+ Utility.Json.ToJson(allianceMembers));
                        subResponseParameters.Add((byte)ParameterCode.AllianceMember, Utility.Json.ToJson(allianceMembers));
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
            }
            ReferencePool.Release(nHCriterias.ToArray());
            return operationResponse;
        }
    }
}


