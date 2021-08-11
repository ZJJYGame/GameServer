using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionServer.Model;
using Cosmos;
using AscensionProtocol.DTO;
namespace AscensionServer
{
    public class RemoveTemoraryRingSubHandler: SyncTemporarySubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Remove;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var TemRingRoleData = Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.RoleTemInventory) as string;
            Utility.Debug.LogInfo(">>>>>Remove 临时背包" + TemRingRoleData + ">>>>>>>>>>>>>");
            var TemRingRoleObj = Utility.Json.ToObject<TemporaryRingDTO>(TemRingRoleData);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", TemRingRoleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleRing>(nHCriteriaRoleID);
            if (exist)
            {
                var ringServer = NHibernateQuerier.CriteriaSelect<TemporaryRing>(nHCriteriaRoleID);
                var tempRing = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServer.RingItems);

                foreach (var temp in TemRingRoleObj.RingItems)
                {
                    if (!tempRing.ContainsKey(temp.Key))
                    {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                        continue;
                    }
                    else
                        tempRing.Remove(temp.Key);
                    NHibernateQuerier.Update(new TemporaryRing() { RoleID = TemRingRoleObj.RoleID, RingItems = Utility.Json.ToJson(tempRing) });
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
            ReferencePool.Release(nHCriteriaRoleID);
            return operationResponse;
        }
    }
}


