using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;
namespace AscensionServer
{
    public class GetRoleAssetsSubHandler : SyncRoleAssetsSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Role));
            Utility.Debug.LogInfo(">>>>>>>>>>>>>GetRoleAssetsSubHandler\n" + roleJson + "\n GetRoleAssetsSubHandler >>>>>>>>>>>>>>>>>>>>>>");
            var roleObj = Utility.Json.ToObject<Role>(roleJson);
            if (RedisHelper.Hash.HashExistAsync("RoleAssets", roleObj.RoleID.ToString()).Result)
            {
                #region Redis模块
                var roleAssetsObj = RedisHelper.Hash.HashGetAsync<RoleAssets>("RoleAssets", roleObj.RoleID.ToString()).Result;
                Utility.Debug.LogError("获取人物资源的书序1");
                string roleAssetsJson = Utility.Json.ToJson(roleAssetsObj);
                SetResponseParamters(() =>
                {
                    Utility.Debug.LogError("获取人物资源的书序4"+ roleAssetsJson);
                    subResponseParameters.Add((byte)ParameterCode.RoleAssets, roleAssetsJson);
                    operationResponse.ReturnCode = (byte)ReturnCode.Success;
                });
                #endregion

            }
            else
            {
                #region MySql
                NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleObj.RoleID);
                var obj = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriaRoleID);
                if (obj != null)
                {
                    var result = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriaRoleID);
                    if (result == null)
                    {
                        Utility.Debug.LogError("获取人物资源的书序2");
                        Utility.Debug.LogInfo(">>>>>>>>>>>>>\n GetRoleAssetsSubHandler  " + roleObj.RoleID + "  GetRoleAssetsSubHandler  \n >>>>>>>>>>>>>>>>>>>>>>");
                        result = NHibernateQuerier.Insert(new RoleAssets() { RoleID = roleObj.RoleID });
                    }
                    string roleAssetsJson = Utility.Json.ToJson(result);
                    Utility.Debug.LogInfo(">>>>>>>>>>>>>出納過去的數據為  " + Utility.Json.ToJson(result) + "   >>>>>>>>>>>>>>>>>>>>>>");
                    SetResponseParamters(() =>
                    {
                        Utility.Debug.LogError("获取人物资源的书序3");
                        subResponseParameters.Add((byte)ParameterCode.RoleAssets, roleAssetsJson);
                        operationResponse.ReturnCode = (byte)ReturnCode.Success;
                    });
                }
                else
                {
                    Utility.Debug.LogError("获取人物资源的书序5");
                    operationResponse.ReturnCode = (byte)ReturnCode.Fail;
                }
                ReferencePool.Release(nHCriteriaRoleID);
                #endregion
            }
            return operationResponse;
        }
    }
}


