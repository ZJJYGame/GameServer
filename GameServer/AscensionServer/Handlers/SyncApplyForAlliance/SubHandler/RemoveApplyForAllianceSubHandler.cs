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
    public class RemoveApplyForAllianceSubHandler : SyncApplyForAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Remove;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;

            string allianceJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.ApplyForAlliance));
            var allianceObj = Utility.Json.ToObject<AllianceMemberDTO>(allianceJson);
            List<int> roleidList = new List<int>();
            roleidList = allianceObj.ApplyforMember;
            NHCriteria nHCriteriallianceMember =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", allianceObj.AllianceID);
            var alliancememberTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceMember>(nHCriteriallianceMember).Result;

            List<NHCriteria> NHCriterias = new List<NHCriteria>();
            NHCriterias.Add(nHCriteriallianceMember);


            List<int> applyList = new List<int>();
            List<int> memberList = new List<int>();
            applyList = Utility.Json.ToObject<List<int>>(alliancememberTemp.ApplyforMember);
            Utility.Debug.LogError("数据库储存的成员的数据 " + Utility.Json.ToJson(applyList));
            for (int i = 0; i < roleidList.Count; i++)
            {

                NHCriteria nHCriteriRoleAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleidList[i]);

                NHCriterias.Add(nHCriteriRoleAlliance);
                var roleAllianceTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriRoleAlliance).Result;
                if (roleAllianceTemp.AllianceID == 0)
                {

                    var roleApplyList = Utility.Json.ToObject<List<int>>(roleAllianceTemp.ApplyForAlliance);
                    if (roleApplyList.Contains(allianceObj.AllianceID))
                    {
                        roleApplyList.Remove(allianceObj.AllianceID);
                    }
                    roleAllianceTemp.ApplyForAlliance = Utility.Json.ToJson(roleApplyList);
                    NHibernateQuerier.Update(roleAllianceTemp);


                    Utility.Debug.LogError("进来的查找所有成员的数据 " + roleidList[i]);
                    applyList.Remove(roleidList[i]);
                }
            }
            alliancememberTemp.ApplyforMember = Utility.Json.ToJson(applyList);
            NHibernateQuerier.Update(alliancememberTemp);
            SetResponseParamters(() =>
            {
                operationResponse.ReturnCode = (short)ReturnCode.Success;
            });
            //TODO写法有问题；
            ReferencePool.Release(NHCriterias.ToArray());
            return operationResponse;
        }
    }
}


