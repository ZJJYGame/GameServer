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
  public   class UpdateShoppingMallSubHandler: SyncShoppingMallSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string rolepurchaseJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RolePurchase));
            var rolepurchaseObj = Utility.Json.ToObject<RolePurchaseRecordDTO>(rolepurchaseJson);
            NHCriteria nHCriteriarolepurchase =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolepurchaseObj.RoleID);
            Utility.Debug.LogInfo("传过来的购买数据" + rolepurchaseJson);
            var rolepurchasetemp = NHibernateQuerier.CriteriaSelect<RolePurchaseRecord>(nHCriteriarolepurchase);
            if (rolepurchasetemp != null)
            {
                if (rolepurchasetemp.GoodsPurchasedCount.Equals("{}"))
                {
                    rolepurchasetemp.GoodsPurchasedCount = Utility.Json.ToJson(rolepurchaseObj.GoodsPurchasedCount);
                    NHibernateQuerier.Update(rolepurchasetemp);
                }
                else
                {
                    var rolepurchaseDict = Utility.Json.ToObject<Dictionary<int, int>>(rolepurchasetemp.GoodsPurchasedCount);
                    foreach (var item in rolepurchaseObj.GoodsPurchasedCount)
                    {
                        if (rolepurchaseDict.ContainsKey(item.Key))
                        {
                            rolepurchaseDict[item.Key] += item.Value;
                        }
                        else
                            rolepurchaseDict.Add(item.Key, item.Value);
                    }                 
                    rolepurchasetemp.GoodsPurchasedCount = Utility.Json.ToJson(rolepurchaseDict);
                    NHibernateQuerier.Update(rolepurchasetemp);
                }
                RolePurchaseRecordDTO rolePurchaseRecordDTO = new RolePurchaseRecordDTO() { RoleID = rolepurchasetemp.RoleID, GoodsPurchasedCount = Utility.Json.ToObject<Dictionary<int, int>>(rolepurchasetemp.GoodsPurchasedCount) };
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.RolePurchase, Utility.Json.ToJson(rolePurchaseRecordDTO));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriarolepurchase);
            return operationResponse;
        }

    }
}


