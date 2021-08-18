using AscensionProtocol;
using Cosmos;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using RedisDotNet;
namespace AscensionServer
{
    public class SyncRoleGongfaHandler : Handler
    {
        public override byte OpCode { get { return (byte)OperationCode.SyncRoleGongfa; } }
        List<NHCriteria> nHCriteriaList = new List<NHCriteria>();

        protected override OperationResponse OnOperationRequest(OperationRequest operationRequest)
        {
            responseParameters.Clear();
            var roleJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.Role));
            Utility.Debug.LogInfo("yzqData收到的功法数据"+ roleJson);
            var roleObj = Utility.Json.ToObject<RoleGongFaDTO>(roleJson);
            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleObj.RoleID);
            var rolegongfaObj = NHibernateQuerier.CriteriaSelect<RoleGongFa>(nHCriteriaRoleStatue);
            List<CultivationMethodDTO> gongfaList = new List<CultivationMethodDTO>();
            nHCriteriaList.Clear();
            if (rolegongfaObj!=null)
            {
                var gongfaDict = Utility.Json.ToObject<Dictionary<int, int>>(rolegongfaObj.GongFaIDDict);
                if (gongfaDict.Count>0)
                {
                    foreach (var item in gongfaDict)
                    {
                        NHCriteria nHCriteriaGongFaId =ReferencePool.Accquire<NHCriteria>().SetValue("ID", item.Key);
                        var gongFaIdArray = NHibernateQuerier.CriteriaSelect<CultivationMethod>(nHCriteriaGongFaId);
                        nHCriteriaList.Add(nHCriteriaGongFaId);
                        var gongfaTemp =ReferencePool.Accquire<CultivationMethodDTO>();
                        gongfaTemp.CultivationMethodExp = gongFaIdArray.CultivationMethodExp;
                        gongfaTemp.CultivationMethodLevel = gongFaIdArray.CultivationMethodLevel;
                        gongfaTemp.CultivationMethodLevelSkillArray = Utility.Json.ToObject<List<int>>(gongFaIdArray.CultivationMethodLevelSkillArray);
                        gongfaTemp.CultivationMethodID = gongFaIdArray.CultivationMethodID;
                        gongfaList.Add(gongfaTemp);
                    }
                    OperationData operationData = new OperationData();
                    operationData.DataMessage = Utility.Json.ToJson(gongfaList);
                    operationData.OperationCode = (byte)OperationCode.SyncRoleGongfa;
                    GameEntry. RoleManager.SendMessage(roleObj.RoleID, operationData);
                }
            }
            ReferencePool.Release(nHCriteriaList.ToArray());
            return operationResponse;
        }

        }
}


