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
    public class GetApplyForAllianceSubHandler : SyncApplyForAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string alliancememberJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.ApplyForAlliance));
            var alliancememberObj = Utility.Json.ToObject<AllianceMemberDTO>(alliancememberJson);

            List<ApplyForAllianceDTO> applyForAllianceList = new List<ApplyForAllianceDTO>();
            List<int> applyForList = new List<int>();
            NHCriteria nHCriteriaalliancemember =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", alliancememberObj.AllianceID);
            var alliancememberTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceMember>(nHCriteriaalliancemember).Result;
            applyForList = Utility.Json.ToObject<List<int>>(alliancememberTemp.ApplyforMember);
            for (int i = 0; i < applyForList.Count; i++)
            {
             var roleObj= AlliancelogicManager.Instance.GetNHCriteria<Role>("RoleID", applyForList[i]);
                var roleschoolObj = AlliancelogicManager.Instance.GetNHCriteria<RoleSchool>("RoleID", applyForList[i]);
                var schoolObj = AlliancelogicManager.Instance.GetNHCriteria<School>("ID", roleschoolObj.RoleJoiningSchool);
                var gongfaObj = AlliancelogicManager.Instance.GetNHCriteria<RoleGongFa>("RoleID", applyForList[i]);

                applyForAllianceList.Add(AlliancelogicManager.Instance.JointDate(roleObj, schoolObj));
            }
            SetResponseParamters(() =>
            {
                Utility.Debug.LogInfo("获得申请加仙盟的列表" + Utility.Json.ToJson(applyForAllianceList));
                subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(applyForAllianceList));
                operationResponse.ReturnCode = (short)ReturnCode.Success;
            });
            ReferencePool.Release(nHCriteriaalliancemember);
            return operationResponse;
        }
    }
}


