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
    public class UpdateTaskSubHandler : SyncTaskSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            string roletask = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.Task));
            Utility.Debug.LogInfo(">>>>>>>>>>>>>更新任务相关信息：" + roletask + ">>>>>>>>>>>>>>>>>>>>>>");
            var roletaskobj = Utility.Json.ToObject<RoleTaskProgressDTO>(roletask);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roletaskobj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleTaskProgress>(nHCriteriaRoleID);
            if (exist)
            {
                var roleTaskInfo = NHibernateQuerier.CriteriaSelect<RoleTaskProgress>(nHCriteriaRoleID);
                var serverDict = Utility.Json.ToObject<Dictionary<string, RoleTaskItemDTO>>(roleTaskInfo.RoleTaskInfoDic);
                if (roleTaskInfo.RoleTaskInfoDic != null)
                {
                    foreach (var client_p in roletaskobj.RoleTaskInfoDic)
                    {
                        if (serverDict.ContainsKey(client_p.Key))
                        {
                            var serverDictValue = serverDict[client_p.Key];
                            if (serverDictValue.RoleTaskAcceptState != client_p.Value.RoleTaskAcceptState)
                                serverDictValue.RoleTaskAcceptState = client_p.Value.RoleTaskAcceptState;
                            if (serverDictValue.RoleTaskAbandonState != client_p.Value.RoleTaskAbandonState)
                                serverDictValue.RoleTaskAbandonState = client_p.Value.RoleTaskAbandonState;
                            if (serverDictValue.RoleTaskAchieveState != client_p.Value.RoleTaskAchieveState)
                                serverDictValue.RoleTaskAchieveState = client_p.Value.RoleTaskAchieveState;
                            NHibernateQuerier.Update(new RoleTaskProgress() { RoleID = roletaskobj.RoleID, RoleTaskInfoDic = Utility.Json.ToJson(serverDict) });
                        }else
                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    }
                    SetResponseParamters(() =>
                    {
                        operationResponse.Parameters = subResponseParameters;
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
            ReferencePool.Release(nHCriteriaRoleID);
            return operationResponse;
        }
    }
}


