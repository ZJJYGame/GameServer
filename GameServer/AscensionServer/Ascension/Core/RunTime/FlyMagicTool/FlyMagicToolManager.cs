using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionProtocol;
using RedisDotNet;
using AscensionServer.Model;

namespace AscensionServer
{
    [Module]
    public class FlyMagicToolManager: Cosmos.Module,IFlyMagicToolManager
    {
        protected override void OnPreparatory()
        {
            Utility.Debug.LogInfo("添加YZQ飞行法器");
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncRoleFlyMagicTool, ProcessHandlerC2S);
        }

        void ProcessHandlerC2S(int seeionid, OperationData packet)
        {
            Utility.Debug.LogInfo("YZQ飞行法器"+ Convert.ToString(packet.DataMessage));
            var flyObj = Utility.Json.ToObject<FlyMagicToolDTO > (Convert.ToString(packet.DataMessage));
            switch ((FlyMagicToolOpCode)packet.SubOperationCode)
            {
                case FlyMagicToolOpCode.AddTool:
                    Utility.Debug.LogInfo("添加YZQ飞行法器");
                    AddFlyMagicToolS2C(flyObj);
                    break;
                case FlyMagicToolOpCode.GetToolData:
                    GetFlyMagicToolS2C(flyObj);
                    break;
                case FlyMagicToolOpCode.UpdateStatus:
                    UpdateFlyMagicToolS2C(flyObj);
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 失败返回
        /// </summary>
        void ResultFailS2C(int roleID, FlyMagicToolOpCode opcode)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            opData.SubOperationCode = (byte)opcode;
            opData.DataMessage = null;
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        /// <summary>
        /// 结果成功返回
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="opcode"></param>
        /// <param name="data"></param>
        void ResultSuccseS2C(int roleID, FlyMagicToolOpCode opcode, object data)
        {
            Utility.Debug.LogInfo("YZQ飞行法器发送ID为"+ roleID);
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncRoleFlyMagicTool;
            opData.SubOperationCode = (byte)opcode;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }


        #region Redis模块
        /// <summary>
        /// 添加新的飞行法器
        /// </summary>
        async void AddFlyMagicToolS2C(FlyMagicToolDTO flyMagic)
        {
            GameEntry. DataManager.TryGetValue<Dictionary<int, FlyMagicToolData>>(out var flyDict);
            if (!flyDict.ContainsKey(flyMagic.FlyMagicToolID))
            {
                ResultFailS2C(flyMagic.RoleID,FlyMagicToolOpCode.AddTool);
            }
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", flyMagic.RoleID);
            var flyMagicObj = NHibernateQuerier.CriteriaSelectAsync<FlyMagicTool>(nHCriteria).Result;
            if (flyMagicObj!=null)
            {
                var flyList = Utility.Json.ToObject<List<int>>(flyMagicObj.AllFlyMagicTool);
                if (!flyList.Contains(flyMagic.FlyMagicToolID))
                {
                    flyList.Add(flyMagic.FlyMagicToolID);
                    flyMagicObj.AllFlyMagicTool = Utility.Json.ToJson(flyList);
                   await NHibernateQuerier.UpdateAsync(flyMagicObj);
                    var obj=flyMagicToolChange(flyMagicObj);
                    ResultSuccseS2C(flyMagic.RoleID,FlyMagicToolOpCode.AddTool, obj);
                    #region Reids
                    await RedisHelper.Hash.HashSetAsync<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString(), obj);
                    #endregion
                }
                else
                    ResultFailS2C(flyMagic.RoleID,FlyMagicToolOpCode.AddTool);
            }else
                ResultFailS2C(flyMagic.RoleID, FlyMagicToolOpCode.AddTool);
        }
        /// <summary>
        /// 获得任务所有飞行法器
        /// </summary>
        /// <param name="flyMagic"></param>
        void GetFlyMagicToolS2C(FlyMagicToolDTO flyMagic)
        {
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString()).Result;

            if (result)
            {
                var flyObj = RedisHelper.Hash.HashGetAsync<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString()).Result;
                if (flyObj != null)
                {
                    ResultSuccseS2C(flyObj.RoleID, FlyMagicToolOpCode.GetToolData, flyObj);
                }
                else
                {
                    //TODO数据库模块
                    GetFlyMagicToolMySql(flyMagic);
                }
            }else
                GetFlyMagicToolMySql(flyMagic);
        }
        /// <summary>
        /// 更新飞行法器状态更改人物属性数据
        /// </summary>
        /// <param name="flyMagic"></param>
         async void UpdateFlyMagicToolS2C(FlyMagicToolDTO flyMagic)
        {
            #region Redis逻辑
            var flyObjExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString()).Result;
            var rolestatusExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, flyMagic.RoleID.ToString()).Result;
            if (flyObjExist&& rolestatusExist)
            {
                var flyObj = RedisHelper.Hash.HashGetAsync<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString()).Result;
                var rolestatus = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, flyMagic.RoleID.ToString()).Result;
                var status = new RoleStatus();
                if (flyObj != null&& rolestatus!=null)
                {
                    flyObj.FlyToolLayoutDict = flyMagic.FlyToolLayoutDict;
                    foreach (var item in flyMagic.FlyToolLayoutDict)
                    {
                        if (flyObj.FlyToolLayoutDict.ContainsKey(item.Key))
                        {
                            status = await GameEntry.practiceManager.RoleSwitchFlyMagicTool(flyObj, rolestatus);
                        }
                        else
                        {
                            status = await GameEntry.practiceManager.RoleFlyMagicTool(flyObj, rolestatus);
                        }
                    }

                    await RedisHelper.Hash.HashSetAsync<FlyMagicToolDTO>(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString(), flyObj);
                    await NHibernateQuerier.UpdateAsync(flyMagicToolChange(flyObj));



                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.RoleStatus, status);
                    dict.Add((byte)ParameterCode.RoleFlyMagicTool, flyObj);
                    ResultSuccseS2C(flyMagic.RoleID, FlyMagicToolOpCode.GetToolData, dict);

                    await NHibernateQuerier.UpdateAsync(status);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, flyMagic.RoleID.ToString(), status);
                }
                else
                    UpdateFlyMagicTool(flyMagic);
            }
            else
                UpdateFlyMagicTool(flyMagic);
            #endregion

        }
        #endregion

        #region MySql模块
        /// <summary>
        /// 获取MySql数据
        /// </summary>
         void  GetFlyMagicToolMySql(FlyMagicToolDTO flyMagic)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", flyMagic.RoleID);
            var flyMagicObj = NHibernateQuerier.CriteriaSelectAsync<FlyMagicTool>(nHCriteria).Result;
            if (flyMagicObj != null)
            {
                var obj = flyMagicToolChange(flyMagicObj);
                ResultSuccseS2C(flyMagic.RoleID, FlyMagicToolOpCode.GetToolData, obj);
            }
            else
                ResultFailS2C(flyMagic.RoleID, FlyMagicToolOpCode.GetToolData);
        }
        /// <summary>
        /// 设置更新飞行法器
        /// </summary>
        /// <param name="flyMagic"></param>
        async void UpdateFlyMagicTool(FlyMagicToolDTO flyMagic)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", flyMagic.RoleID);
            var flyMagicObj = NHibernateQuerier.CriteriaSelectAsync<FlyMagicTool>(nHCriteria).Result;
            var roleStatusObj = NHibernateQuerier.CriteriaSelectAsync<RoleStatus>(nHCriteria).Result;
            if (flyMagicObj != null&& roleStatusObj!=null)
            {
                flyMagicObj.FlyToolLayoutDict = Utility.Json.ToJson(flyMagic.FlyToolLayoutDict);
                var obj = flyMagicToolChange(flyMagicObj);
                await NHibernateQuerier.UpdateAsync(flyMagicObj);
                await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleFlyMagicToolPerfix, flyMagic.RoleID.ToString(), obj);

                var status = await GameEntry.practiceManager.RoleFlyMagicTool(obj, roleStatusObj);

                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.RoleStatus, status);
                dict.Add((byte)ParameterCode.RoleFlyMagicTool, obj);
                ResultSuccseS2C(flyMagic.RoleID, FlyMagicToolOpCode.GetToolData, dict);

                await NHibernateQuerier.UpdateAsync(status);
                await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, flyMagic.RoleID.ToString(), status);
            }
            else
                ResultFailS2C(flyMagic.RoleID, FlyMagicToolOpCode.GetToolData);
        }
        #endregion
        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="flyMagic"></param>
        /// <returns></returns>
        FlyMagicToolDTO flyMagicToolChange(FlyMagicTool flyMagic)
        {
            FlyMagicToolDTO flyMagicTool = new FlyMagicToolDTO();
            flyMagicTool.AllFlyMagicTool = Utility.Json.ToObject<List<int>>(flyMagic.AllFlyMagicTool);
            flyMagicTool.FlyToolLayoutDict= Utility.Json.ToObject<Dictionary<string,int>>(flyMagic.FlyToolLayoutDict);
            flyMagicTool.RoleID = flyMagic.RoleID;

            return flyMagicTool;
        }

        FlyMagicTool flyMagicToolChange(FlyMagicToolDTO flyMagic)
        {
            FlyMagicTool flyMagicTool = new FlyMagicTool();
            flyMagicTool.AllFlyMagicTool = Utility.Json.ToJson(flyMagic.AllFlyMagicTool);
            flyMagicTool.FlyToolLayoutDict = Utility.Json.ToJson(flyMagic.FlyToolLayoutDict);
            flyMagicTool.RoleID = flyMagic.RoleID;

            return flyMagicTool;
        }
    }
}
