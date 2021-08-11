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
    public class RemoveAllianceMemberSubHandler : SyncAllianceMemberSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Remove;
        List<NHCriteria> nHCriterias = new List<NHCriteria>();

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string allianceMemberJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AllianceMember));
            var allianceMemberObj = Utility.Json.ToObject<AllianceMemberDTO>(allianceMemberJson);

            NHCriteria nHCriteriallianceMember =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", allianceMemberObj.AllianceID);
            var allianceMemberTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceMember>(nHCriteriallianceMember).Result;

            #region 待删
            nHCriterias.Clear();
            nHCriterias.Add(nHCriteriallianceMember);
            List<int> memberlist = new List<int>();
            if (allianceMemberTemp != null)
            {
                if (!string.IsNullOrEmpty(allianceMemberTemp.Member))
                {
                    for (int i = 0; i < allianceMemberObj.Member.Count; i++)
                    {
                        NHCriteria nHCriteriMember =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", allianceMemberObj.Member[i]);
                        nHCriterias.Add(nHCriteriMember);

                        var alliancestatus = AlliancelogicManager.Instance.GetNHCriteria<AllianceStatus>("ID", allianceMemberObj.AllianceID);
                        alliancestatus.AllianceNumberPeople -= 1;
                        NHibernateQuerier.Update(alliancestatus);
                        var MemberTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriMember).Result;
                        MemberTemp.AllianceID = 0;
                        MemberTemp.AllianceJob = 4;
                        NHibernateQuerier.Update(MemberTemp);
                        memberlist = Utility.Json.ToObject<List<int>>(allianceMemberTemp.Member);
                        memberlist.Remove(allianceMemberObj.Member[i]);
                        allianceMemberTemp.Member = Utility.Json.ToJson(memberlist);
                        NHibernateQuerier.Update(allianceMemberTemp);
                        SetResponseParamters(() =>
                        {
                            operationResponse.ReturnCode = (short)ReturnCode.Success;
                        });
                    }
                }
                else
                {
                    SetResponseParamters(() =>
                    {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                }
            }
            #endregion
            ReferencePool.Release(nHCriterias.ToArray());
            return operationResponse;
        }
    }
}


