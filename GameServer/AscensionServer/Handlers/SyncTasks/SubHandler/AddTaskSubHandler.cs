using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionServer.Model;
using Newtonsoft.Json;
using AscensionProtocol.DTO;
using Cosmos;
namespace AscensionServer
{
    public class AddTaskSubHandler : SyncTaskSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            string roletask = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.Task));
            Utility.Debug.LogInfo(">>>>>>>>>>>>>添加任务相关信息：" + roletask + ">>>>>>>>>>>>>>>>>>>>>>");
            var roletaskobj = Utility.Json.ToObject<RoleTaskProgressDTO>(roletask);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roletaskobj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleTaskProgress>(nHCriteriaRoleID);
            Dictionary<string, RoleTaskItemDTO> Dic;
            if (exist)
            {
                var roleTaskInfo = NHibernateQuerier.CriteriaSelect<RoleTaskProgress>(nHCriteriaRoleID);
                Dic = Utility.Json.ToObject<Dictionary<string, RoleTaskItemDTO>>(roleTaskInfo.RoleTaskInfoDic);
                if (roleTaskInfo.RoleTaskInfoDic !=null )
                {
                    foreach (var client_n in roletaskobj.RoleTaskInfoDic)
                    {
                        if (!Dic.ContainsKey(client_n.Key))
                        {
                            Dic.Add(client_n.Key, client_n.Value);
                            NHibernateQuerier.Update(new RoleTaskProgress() { RoleID = roletaskobj.RoleID, RoleTaskInfoDic = Utility.Json.ToJson(Dic) });
                        }else
                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    }
                    SetResponseParamters(() =>
                    {
                        operationResponse.Parameters = subResponseParameters;
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
            ReferencePool.Release(nHCriteriaRoleID);
            return operationResponse;
        }
    }
}


