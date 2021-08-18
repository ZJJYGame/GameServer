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
    public class GetTaskSubHandler : SyncTaskSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {

            string roletask = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.Task));
            Utility.Debug.LogInfo(">>>>>>>>>>>>>获得任务相关信息：" + roletask + ">>>>>>>>>>>>>>>>>>>>>>");
            var roletaskobj = Utility.Json.ToObject<RoleTaskProgressDTO>(roletask);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roletaskobj.RoleID);
            bool exist = NHibernateQuerier.Verify<Role>(nHCriteriaRoleID);
            if (exist)
            {
                var roleTaskInfo = NHibernateQuerier.CriteriaSelect<RoleTaskProgress>(nHCriteriaRoleID);
                SetResponseParamters(() => {
                    if (roleTaskInfo.RoleTaskInfoDic != null)
                        subResponseParameters.Add((byte)ParameterCode.Task, roleTaskInfo.RoleTaskInfoDic);
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
            ReferencePool.Release(nHCriteriaRoleID);
            return operationResponse;
        }
    }
}

