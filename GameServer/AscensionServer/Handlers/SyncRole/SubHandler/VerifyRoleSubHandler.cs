using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
namespace AscensionServer
{
    public class VerifyRoleSubHandler : SyncRoleSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Verify;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string rolestatusJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleStatus));
            Utility.Debug.LogInfo(">>>>>>>>>>>>VerifyRoleStatusHandler\n传输过来更新的战斗数据:" + rolestatusJson + "VerifyRoleStatusHandler\n<<<<<<<<<<<");
            var rolestatusObj = Utility.Json.ToObject<RoleStatus>(rolestatusJson);
            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolestatusObj.RoleID);
            var result = NHibernateQuerier.Verify<RoleStatus>(nHCriteriaRoleStatue);
            if (result)
            {
                NHibernateQuerier.Update(rolestatusObj);
                SetResponseParamters(() => operationResponse.ReturnCode = (short)ReturnCode.Success);
            }
            else
            {
                SetResponseParamters(() => operationResponse.ReturnCode = (short)ReturnCode.Fail);
            }
            return operationResponse;
        }
    }
}


