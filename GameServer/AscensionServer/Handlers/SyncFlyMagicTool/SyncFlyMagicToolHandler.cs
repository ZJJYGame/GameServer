using AscensionProtocol;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using RedisDotNet;
namespace AscensionServer
{
    public class SyncFlyMagicToolHandler:Handler
    {
        public override byte OpCode { get; }

        protected override OperationResponse OnOperationRequest(OperationRequest operationRequest)
        {
            Utility.Debug.LogInfo("yzqData获取角色飞行法器");
            var roleFlyMagicToolJson = Convert.ToString(Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.RoleFlyMagicTool));
            Utility.Debug.LogInfo("yzqData获取角色飞行法器" + roleFlyMagicToolJson);
            var roleFlyMagicToolObj = Utility.Json.ToObject<FlyMagicToolDTO>(roleFlyMagicToolJson);
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleFlyMagicToolObj.RoleID);
            var roleFlyMagicTool= NHibernateQuerier.CriteriaSelectAsync<FlyMagicTool>(nHCriteriaRole).Result;
            Utility.Debug.LogInfo("yzqData获取角色飞行法器"+ roleFlyMagicToolJson);
            FlyMagicToolDTO flyMagicToolRedisObj = new FlyMagicToolDTO();
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleFlyMagicToolPerfix, roleFlyMagicToolObj.RoleID.ToString()).Result)
            {
                flyMagicToolRedisObj = RedisHelper.Hash.HashGetAsync<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, roleFlyMagicToolObj.RoleID.ToString()).Result;
            }
            else
            {
                flyMagicToolRedisObj = null;
            }
            #region
            //switch (roleFlyMagicToolObj.OprateType)
            //{
            //    case FlyMagicToolDTO.FlyMagicToolType.Add:
            //        if (roleFlyMagicTool != null)
            //        {
            //            var tempList = Utility.Json.ToObject<List<int>>(roleFlyMagicTool.AllFlyMagicTool);
            //            if (!tempList.Contains(roleFlyMagicToolObj.FlyMagicToolID))
            //            {
            //                tempList.Add(roleFlyMagicToolObj.FlyMagicToolID);
            //                roleFlyMagicToolObj.AllFlyMagicTool = tempList;
            //                roleFlyMagicToolObj.RoleID = roleFlyMagicTool.RoleID;
            //                roleFlyMagicToolObj.FlyToolLayoutDict = Utility.Json.ToObject<Dictionary<string, int>>(roleFlyMagicTool.FlyToolLayoutDict);
            //                RedisHelper.Hash.HashSet<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, roleFlyMagicToolObj.RoleID.ToString(), roleFlyMagicToolObj);

            //                roleFlyMagicTool.AllFlyMagicTool = Utility.Json.ToJson(tempList);
            //                NHibernateQuerier.Update<FlyMagicTool>(roleFlyMagicTool);

            //                OperationData opData = new OperationData();
            //                opData.DataMessage = Utility.Json.ToJson(roleFlyMagicToolObj);
            //                opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            //                opData.ReturnCode = (byte)ReturnCode.Success;
            //                GameEntry.RoleManager.SendMessage(roleFlyMagicToolObj.RoleID, opData);
            //            }
            //            else
            //            {
            //                OperationData opData = new OperationData();
            //                opData.DataMessage = Utility.Json.ToJson(roleFlyMagicToolObj);
            //                opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            //                opData.ReturnCode = (byte)ReturnCode.Fail;
            //                GameEntry.RoleManager.SendMessage(flyMagicToolRedisObj.RoleID, opData);
            //            }
            //        }
            //        break;
            //    case FlyMagicToolDTO.FlyMagicToolType.Get:
            //        Utility.Debug.LogInfo("yzqData获取角色飞行法器1");
            //        if (flyMagicToolRedisObj == null)
            //        {
            //            Utility.Debug.LogInfo("yzqData获取角色飞行法器2");
            //            if (roleFlyMagicTool != null)
            //            {
            //                Utility.Debug.LogInfo("yzqData获取角色飞行法器3");
            //                roleFlyMagicToolObj.AllFlyMagicTool = Utility.Json.ToObject<List<int>>(roleFlyMagicTool.AllFlyMagicTool);
            //                OperationData opData = new OperationData();
            //                opData.DataMessage = Utility.Json.ToJson(roleFlyMagicToolObj);
            //                opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            //                opData.ReturnCode = (byte)ReturnCode.Success;
            //                GameEntry.RoleManager.SendMessage(roleFlyMagicToolObj.RoleID, opData);
            //            }
            //            else
            //            {
            //                Utility.Debug.LogInfo("yzqData获取角色飞行法器4");
            //                OperationData opData = new OperationData();
            //                opData.DataMessage = Utility.Json.ToJson(roleFlyMagicToolObj);
            //                opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            //                opData.ReturnCode = (byte)ReturnCode.Fail;
            //                GameEntry.RoleManager.SendMessage(roleFlyMagicToolObj.RoleID, opData);
            //            }
            //        }
            //        else
            //        {
            //            Utility.Debug.LogInfo("yzqData获取角色飞行法器5");
            //            OperationData opData = new OperationData();
            //            opData.DataMessage = Utility.Json.ToJson(flyMagicToolRedisObj);
            //            opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            //            opData.ReturnCode = (byte)ReturnCode.Success;
            //            GameEntry.RoleManager.SendMessage(flyMagicToolRedisObj.RoleID, opData);
            //        }
            //        break;
            //    case FlyMagicToolDTO.FlyMagicToolType.Update:
            //        Utility.Debug.LogInfo("yzqData获取角色飞行法器");
            //        if (roleFlyMagicTool != null)
            //        {
            //            roleFlyMagicTool.FlyToolLayoutDict = Utility.Json.ToJson(roleFlyMagicToolObj.FlyToolLayoutDict);
            //            NHibernateQuerier.Update<FlyMagicTool>(roleFlyMagicTool);
            //        }
            //        if (flyMagicToolRedisObj != null)
            //        {
            //            flyMagicToolRedisObj.FlyToolLayoutDict = roleFlyMagicToolObj.FlyToolLayoutDict;
            //            RedisHelper.Hash.HashSet<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, roleFlyMagicToolObj.RoleID.ToString(), flyMagicToolRedisObj);
            //        }
            //        break;
            //    default:
            //        break;
            //}
            #endregion
            return operationResponse;
        }
    }
}


