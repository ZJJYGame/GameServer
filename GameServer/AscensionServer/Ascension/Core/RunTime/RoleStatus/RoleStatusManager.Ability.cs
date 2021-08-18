using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using RedisDotNet;
using Cosmos;
namespace AscensionServer
{
    public partial class  RoleStatusManager
    {
        /// <summary>
        /// 获取人物加点方案
        /// </summary>
        /// <param name="pointDTO"></param>
         void GetRolePointAbilityS2C(RoleStatusPointDTO pointDTO)
        {
            Utility.Debug.LogInfo("YZQ获得角色属性0" + Utility.Json.ToJson(pointDTO));
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString()).Result;
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString()).Result&& result)
            {
                var AbilityPoint = RedisHelper.Hash.HashGetAsync<RoleStatusPointDTO>(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString()).Result;
                var rolestatus = RedisHelper.Hash.HashGetAsync<RoleStatusDTO>(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString()).Result;
                if (AbilityPoint != null)
                {
                    Dictionary<byte, object> dataDict = new Dictionary<byte, object>();
                    dataDict.Add((byte)ParameterCode.RoleStatus, rolestatus);
                    dataDict.Add((byte)ParameterCode.RoleStatusPoint, AbilityPoint);
                    RoleStatusSuccessS2C(pointDTO.RoleID, RoleStatusOpCode.GetStatus, dataDict);
                }
                else
                {
                    GetRoleStatusMySql(pointDTO);
                }
            }else
                GetRoleStatusMySql(pointDTO);
        }
        /// <summary>
        ///设置人物加点
        /// </summary>
        async void SetRolePointS2C(RoleStatusPointDTO pointDTO)
        {
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString()).Result;
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString()).Result&& result)
            {
                var pointObj = RedisHelper.Hash.HashGetAsync<RoleStatusPointDTO>(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString()).Result;
                var roleStatusObj = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString()).Result;

                var obj = RolePointCalculate(pointObj, pointDTO);
                if (obj != null&& roleStatusObj!=null)
                {
                 var status= await  GameEntry.practiceManager.RoleAblility(obj, roleStatusObj);
                    Dictionary<byte, object> dataDict = new Dictionary<byte, object>();
                    dataDict.Add((byte)ParameterCode.RoleStatus, status);
                    dataDict.Add((byte)ParameterCode.RoleStatusPoint, pointObj);
                    RoleStatusSuccessS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint, dataDict);
                    Utility.Debug.LogError(Utility.Json.ToJson(status));
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString(), obj);
                    await NHibernateQuerier.UpdateAsync(ChangeRoleStatusPointType(obj));

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString(), status);
                    await NHibernateQuerier.UpdateAsync(status);
                }
                else
                    SetRolePointMySql(pointDTO);
            }else
                SetRolePointMySql(pointDTO);
        }
        /// <summary>
        /// 设置加点方案名称
        /// </summary>
       async  void SetRoleSlnNameS2C(RoleStatusPointDTO pointDTO)
        {
            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", pointDTO.RoleID);
            var rolePoint = NHibernateQuerier.CriteriaSelect<RoleStatusPoint>(nHCriteriaRoleStatue);
            if (rolePoint!=null)
            {
                //TODO记得补充名称合法验证
                var obj = ChangeRoleStatusPointType(rolePoint);
                var result=  obj.AbilityPointSln.TryGetValue(pointDTO.SlnNow,out var ability);
                var data = pointDTO.AbilityPointSln.TryGetValue(pointDTO.SlnNow, out var abilitydto);
                if (result&& data)
                {
                    ability.SlnName = abilitydto.SlnName;
                    obj.AbilityPointSln[pointDTO.SlnNow] = ability;
                    RoleStatusSuccessS2C(pointDTO.RoleID,RoleStatusOpCode.Rename, obj);

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString(), obj);
                    await NHibernateQuerier.UpdateAsync(ChangeRoleStatusPointType(obj));
                }
                else
                    RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint);
            }
            else
                RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint);
        }
        /// <summary>
        /// 重置加点
        /// </summary>
       async void RestartPointS2C(RoleStatusPointDTO pointDTO)
        {
            var exist =GameEntry. DataManager.TryGetValue<Dictionary<int, RoleStatusDatas>>(out var roleStatudict);
            Utility.Debug.LogInfo("YZQ加点数据进来了");
            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", pointDTO.RoleID);
            int point = 0;
            var rolePoint = NHibernateQuerier.CriteriaSelect<RoleStatusPoint>(nHCriteriaRoleStatue);
            var roleStatus= NHibernateQuerier.CriteriaSelect<RoleStatus>(nHCriteriaRoleStatue);
            var role = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriaRoleStatue);
            if (rolePoint != null && role != null)
            {
                Utility.Debug.LogInfo("YZQ加点数据进来了1");
                //TODO记得补充名称合法验证
                var obj = ChangeRoleStatusPointType(rolePoint);
                var result = obj.AbilityPointSln.TryGetValue(pointDTO.SlnNow, out var ability);
                var data = pointDTO.AbilityPointSln.TryGetValue(pointDTO.SlnNow, out var abilitydto);
                if (result && data)
                {
                    if (exist)
                    {
                        foreach (var item in roleStatudict)
                        {
                            if (item.Value.LevelID<= role.RoleLevel)
                            {
                                point += item.Value.FreeAttributes;
                            }
                        }

                    }
                    Utility.Debug.LogInfo("YZQ重置加点为"+ point);
                    ability.SurplusAptitudePoint = point;
                    ability.Agility = 0;
                    ability.Corporeity = 0;
                    ability.Power = 0;
                    ability.Soul = 0;
                    ability.Stamina = 0;
                    ability.Strength = 0;
                    ability.Agility = 0;
                    obj.AbilityPointSln[pointDTO.SlnNow] = ability;
                    Utility.Debug.LogInfo("YZQ重置加点为" + Utility.Json.ToJson(obj));

                    var status = await GameEntry.practiceManager.RoleAblility(obj, roleStatus);
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.RoleStatus, status);
                    dict.Add((byte)ParameterCode.RoleStatusPoint, obj);
                    RoleStatusSuccessS2C(pointDTO.RoleID, RoleStatusOpCode.RestartAddPoint, dict);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString(), obj);
                    await NHibernateQuerier.UpdateAsync(ChangeRoleStatusPointType(obj));

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString(), status);
                    await NHibernateQuerier.UpdateAsync(status);
                }
                else
                    RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint);
            }
            else
            RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint);
        }
        /// <summary>
        /// 切换解决方案
        /// </summary>
        async void SwitchAddPointSlnS2C(RoleStatusPointDTO pointDTO)
        {
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString()).Result;
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString()).Result && result)
            {
                var pointObj = RedisHelper.Hash.HashGetAsync<RoleStatusPointDTO>(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString()).Result;
                var roleStatusObj = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString()).Result;

                var obj = RolePointCalculate(pointObj, pointDTO);
                if (obj != null && roleStatusObj != null)
                {
                    var status = await GameEntry.practiceManager.RoleSwitchAblility(obj, roleStatusObj);
                    Dictionary<byte, object> dataDict = new Dictionary<byte, object>();
                    dataDict.Add((byte)ParameterCode.RoleStatus, status);
                    dataDict.Add((byte)ParameterCode.RoleStatusPoint, pointObj);
                    RoleStatusSuccessS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint, dataDict);
                    Utility.Debug.LogError(Utility.Json.ToJson(status));
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString(), obj);
                    await NHibernateQuerier.UpdateAsync(ChangeRoleStatusPointType(obj));

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString(), status);
                    await NHibernateQuerier.UpdateAsync(status);
                }
                else
                    SetRolePointMySql(pointDTO);
            }
            else
                SetRolePointMySql(pointDTO);
        }
        #region MySql模块
        /// <summary>
        /// 获取MySql中的角色数据
        /// </summary>
        void GetRoleStatusMySql(RoleStatusPointDTO pointDTO)
        {
            Utility.Debug.LogInfo("YZQ获得角色属性1" + Utility.Json.ToJson(pointDTO));
            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", pointDTO.RoleID);
            var rolePoint = NHibernateQuerier.CriteriaSelect<RoleStatusPoint>(nHCriteriaRoleStatue);
            var roleStatus = NHibernateQuerier.CriteriaSelect<RoleStatus>(nHCriteriaRoleStatue);
            if (rolePoint != null&& roleStatus!=null)
            {
                Dictionary<byte, object> dataDict = new Dictionary<byte, object>();
                dataDict.Add((byte)ParameterCode.RoleStatus, roleStatus);
                dataDict.Add((byte)ParameterCode.RoleStatusPoint, ChangeRoleStatusPointType(rolePoint));
                RoleStatusSuccessS2C(pointDTO.RoleID, RoleStatusOpCode.GetStatus, dataDict);
                Utility.Debug.LogInfo("YZQ获得角色属性2" + Utility.Json.ToJson(dataDict));
            }
            else
                RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.GetStatus);
        }

       async  void SetRolePointMySql(RoleStatusPointDTO pointDTO)
        {
            NHCriteria nHCriteriaRoleStatue =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", pointDTO.RoleID);
            var rolePoint = NHibernateQuerier.CriteriaSelect<RoleStatusPoint>(nHCriteriaRoleStatue);
            var roleStatus= NHibernateQuerier.CriteriaSelect<RoleStatus>(nHCriteriaRoleStatue);
            if (rolePoint != null&& roleStatus!=null)
            {
                var obj = ChangeRoleStatusPointType(rolePoint);
                Utility.Debug.LogInfo("YZQ设置加点数据3" + (obj != null));
                var pointObj = RolePointCalculate(obj, pointDTO);
                if (pointObj != null)
                {
                    var status = await GameEntry.practiceManager.RoleAblility(obj, roleStatus);

                    Dictionary<byte, object> dataDict = new Dictionary<byte, object>();
                    dataDict.Add((byte)ParameterCode.RoleStatus, status);
                    dataDict.Add((byte)ParameterCode.RoleStatusPoint, pointObj);
                    RoleStatusSuccessS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint, dataDict);
                    Utility.Debug.LogInfo("YZQ设置加点数据发送成功2" + Utility.Json.ToJson(dataDict));
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAbilityPointPostfix, pointDTO.RoleID.ToString(), pointObj);
                    await NHibernateQuerier.UpdateAsync(ChangeRoleStatusPointType(pointObj));

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, pointDTO.RoleID.ToString(), status);
                    await NHibernateQuerier.UpdateAsync(status);
                }
                else
                    RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint);
            }else
                RoleStatusFailS2C(pointDTO.RoleID, RoleStatusOpCode.SetAddPoint);
        }
        #endregion


        /// <summary>
        /// 人物加点计算验证
        /// </summary>
        /// <param name="pointObj">数据库</param>
        /// <param name="pointDTO">客户端接收数据</param>
        /// <returns></returns>
        RoleStatusPointDTO RolePointCalculate(RoleStatusPointDTO pointObj, RoleStatusPointDTO pointDTO)
        {
            if (pointObj != null)
            {
                var result = pointObj.AbilityPointSln.TryGetValue(pointDTO.SlnNow, out var ability);
                var exist = pointDTO.AbilityPointSln.TryGetValue(pointDTO.SlnNow, out var abilityDTO);
                if (result && exist)
                {
                    var num = abilityDTO.Agility + abilityDTO.Corporeity + abilityDTO.Power + abilityDTO.Soul + abilityDTO.Stamina + abilityDTO.Strength;
                    Utility.Debug.LogInfo("YZQ设置加点数据客户端点数" + num+"剩余点数"+ ability.SurplusAptitudePoint);
                    if (ability.SurplusAptitudePoint >= num)
                    {
                        ability.SurplusAptitudePoint -= num;
                        ability.Agility += abilityDTO.Agility;
                        ability.Corporeity += abilityDTO.Corporeity;
                        ability.Power += abilityDTO.Power;
                        ability.Soul += abilityDTO.Soul;
                        ability.Stamina += abilityDTO.Stamina;
                        ability.Strength += abilityDTO.Strength;
                        pointObj.AbilityPointSln[pointDTO.SlnNow] = ability;
                        pointObj.SlnNow = pointDTO.SlnNow;
                        Utility.Debug.LogInfo("剩余点数" +Utility.Json.ToJson(ability));
                        return pointObj;
                    }
                    else
                        return null;
                }else
                    return null;
            }
            else
                return null;
        }
        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <param name="pointDTO"></param>
        /// <returns></returns>
        RoleStatusPoint ChangeRoleStatusPointType(RoleStatusPointDTO pointDTO)
        {
            RoleStatusPoint roleStatusPoint = new RoleStatusPoint();
            roleStatusPoint.AbilityPointSln = Utility.Json.ToJson(pointDTO.AbilityPointSln);
            roleStatusPoint.RoleID = pointDTO.RoleID;
            roleStatusPoint.SlnNow = pointDTO.SlnNow;
            return roleStatusPoint;
        }
        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <param name="pointDTO"></param>
        /// <returns></returns>
        RoleStatusPointDTO ChangeRoleStatusPointType(RoleStatusPoint point)
        {
            RoleStatusPointDTO roleStatusPoint = new RoleStatusPointDTO();
            roleStatusPoint.AbilityPointSln = Utility.Json.ToObject<Dictionary<int, AbilityDTO>>(point.AbilityPointSln);
            roleStatusPoint.RoleID = point.RoleID;
            roleStatusPoint.SlnNow = point.SlnNow;
            return roleStatusPoint;
        }

    }
}
