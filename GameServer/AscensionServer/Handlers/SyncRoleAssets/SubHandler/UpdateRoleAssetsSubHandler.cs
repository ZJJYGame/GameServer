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
    public class UpdateRoleAssetsSubHandler : SyncRoleAssetsSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            string roleAssetsJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.RoleAssets));

            Utility.Debug.LogInfo(">>>>>>>>>>>>>更新财产相关信息：" + roleAssetsJson + ">>>>>>>>>>>>>>>>>>>>>>");
            var roleAssetsObj = Utility.Json.ToObject<RoleAssets>(roleAssetsJson);

        //var result=    r.ReidsDataProcessing.GetRedisData(RedisKeyDefine._SkillLayoutPerfix, roleAssetsObj.RoleID);
        //    Utility.Debug.LogInfo("yzqData"+result);

            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleAssetsObj.RoleID);
            bool roleExist = NHibernateQuerier.Verify<Role>(nHCriteriaRoleID);
            bool roleAssetsExist = NHibernateQuerier.Verify<RoleAssets>(nHCriteriaRoleID);

            if (roleExist && roleAssetsExist)
            {
                var assetsServer = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriaRoleID);

                if (roleAssetsObj.XianYu != 0 && assetsServer.XianYu >= 0)
                {
                    Utility.Debug.LogInfo("变动的仙玉数量" + roleAssetsObj.XianYu);
                    Utility.Debug.LogInfo("当前的仙玉数量" + assetsServer.XianYu);
                    assetsServer.XianYu += roleAssetsObj.XianYu;
                    if ((assetsServer.XianYu < 0))
                        assetsServer.XianYu = 0;
                }
                if (roleAssetsObj.SpiritStonesLow != 0 && assetsServer.SpiritStonesLow >= 0)
                {
                    assetsServer.SpiritStonesLow += roleAssetsObj.SpiritStonesLow;
                    if ((assetsServer.SpiritStonesLow < 0))
                        assetsServer.SpiritStonesLow = 0;
                }

                NHibernateQuerier.Update<RoleAssets>(new RoleAssets() { RoleID = roleAssetsObj.RoleID,  SpiritStonesLow = assetsServer.SpiritStonesLow,XianYu = assetsServer.XianYu });

               RedisHelper.Hash.HashSet<RoleAssets>("RoleAssets", roleAssetsObj.RoleID.ToString(), new RoleAssets() { RoleID = roleAssetsObj.RoleID, SpiritStonesLow = assetsServer.SpiritStonesLow, XianYu = assetsServer.XianYu });
                SetResponseParamters(() => { operationResponse.ReturnCode = (byte)ReturnCode.Success; });
            }
            else
                SetResponseParamters(() => { operationResponse.ReturnCode = (byte)ReturnCode.Fail; });
            ReferencePool.Release(nHCriteriaRoleID);
            return operationResponse;
        }

        //void fun(string str)
        //{


        //    Utility.Debug.LogInfo("yzqData>>>>>>>>>>>>>>>>>>>>>>>>" + str);
        //}
    }
}


