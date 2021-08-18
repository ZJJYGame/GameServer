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
namespace AscensionServer
{
    public class UpdateRoleSubHandler : SyncRoleSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Role));


            var roleObj = Utility.Json.ToObject<RoleDTO>(roleJson);
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleObj.RoleID);
            var roleTemp = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriaRole);

            if (roleTemp != null)
            {
                roleTemp.RoleLevel = roleObj.RoleLevel;
              NHibernateQuerier.Update(roleTemp);
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.Role, Utility.Json.ToJson(roleTemp));
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
            return operationResponse;
        }
    }
}


