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
namespace AscensionServer
{
    public class UpdateVareityShopSubHandler : SyncVareityShopSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string rolepurchaseJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.VareityPurchase));
            var rolepurchaseObj = Utility.Json.ToObject<VareityPurchaseRecordDTO>(rolepurchaseJson);
            #region Redis模块
            var vareityname = RedisData.ReidsDataProcessing.InsertName("VAREITY_BUY_COUNT", rolepurchaseObj.RoleID);
            var vareitycontent = RedisData.ReidsDataProcessing.GetData(vareityname);

            #endregion
            NHCriteria nHCriteriarolepurchase =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", rolepurchaseObj.RoleID);
            Utility.Debug.LogInfo("传过来的杂货铺购买数据" + rolepurchaseJson);
            var rolepurchasetemp = NHibernateQuerier.CriteriaSelect<VareityPurchaseRecord>(nHCriteriarolepurchase);
            if (rolepurchasetemp != null)
            {
                if (rolepurchasetemp.VareityPurchasedCount.Equals("{}"))
                {
                    rolepurchasetemp.VareityPurchasedCount = Utility.Json.ToJson(rolepurchaseObj.VareityPurchasedCount);
                    NHibernateQuerier.Update(rolepurchasetemp);
                }
                else
                {
                    var rolepurchaseDict = Utility.Json.ToObject<Dictionary<int, int>>(rolepurchasetemp.VareityPurchasedCount);
                    foreach (var item in rolepurchaseObj.VareityPurchasedCount)
                    {
                        if (rolepurchaseDict.ContainsKey(item.Key))
                        {
                            rolepurchaseDict[item.Key] += item.Value;
                        }
                        else
                            rolepurchaseDict.Add(item.Key, item.Value);
                    }
                    rolepurchasetemp.VareityPurchasedCount = Utility.Json.ToJson(rolepurchaseDict);
                    NHibernateQuerier.Update(rolepurchasetemp);
                }

                VareityPurchaseRecordDTO vareityPurchaseRecordDTO = new VareityPurchaseRecordDTO() { RoleID = rolepurchasetemp.RoleID, VareityPurchasedCount = Utility.Json.ToObject<Dictionary<int, int>>(rolepurchasetemp.VareityPurchasedCount) };
                if (!string.IsNullOrEmpty(vareitycontent))
                {
                    RedisHelper.String.StringGetSet(vareityname, Utility.Json.ToJson(vareityPurchaseRecordDTO));
                }
                else
                {
                    RedisHelper.String.StringGet(vareityname, Utility.Json.ToJson(vareityPurchaseRecordDTO));
                }
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.VareityPurchase, Utility.Json.ToJson(vareityPurchaseRecordDTO));
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


