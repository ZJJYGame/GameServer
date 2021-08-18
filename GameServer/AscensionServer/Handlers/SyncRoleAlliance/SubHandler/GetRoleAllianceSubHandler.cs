using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;
namespace AscensionServer
{
    public class GetRoleAllianceSubHandler : SyncRoleAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleallianceJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleAlliance));
            var roleallianceObj = Utility.Json.ToObject<RoleAllianceDTO>
              (roleallianceJson);
            NHCriteria nHCriteriaroleAlliances =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleallianceObj.RoleID);
            var roleallianceTemp = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaroleAlliances);
            GameEntry. DataManager.TryGetValue<Dictionary<int, AllianceLevleUpData>>(out var allianceLevleUpDataDict);
            var content = RedisHelper.KeyExistsAsync("AllianceConstructionDTO" + roleallianceObj.AllianceID).Result;

            List<string> Alliancelist = new List<string>();

            if (roleallianceTemp != null)
            {
                NHCriteria nHCriteriaAlliances =ReferencePool.Accquire<NHCriteria>().SetValue("ID", roleallianceTemp.AllianceID);
                var Role = AlliancelogicManager.Instance.GetNHCriteria<Role>("RoleID", roleallianceObj.RoleID);

                RoleAllianceDTO roleAllianceDTO = new RoleAllianceDTO() { AllianceID = roleallianceTemp.AllianceID, AllianceJob = roleallianceTemp.AllianceJob, JoinTime = roleallianceTemp.JoinTime, ApplyForAlliance = Utility.Json.ToObject<List<int>>(roleallianceTemp.ApplyForAlliance), Offline = roleallianceTemp.Offline, Reputation = roleallianceTemp.Reputation, ReputationHistroy = roleallianceTemp.ReputationHistroy, ReputationMonth = roleallianceTemp.ReputationMonth, RoleID = roleallianceTemp.RoleID, RoleName = roleallianceTemp.RoleName,  RoleLevel = Role.RoleLevel };
                var allianceTemp = NHibernateQuerier.CriteriaSelectAsync<AllianceStatus>(nHCriteriaAlliances).Result;
                NHCriteria nHCriteriaAlliancesConstruction =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", roleallianceTemp.AllianceID);

                var allianceConstructionTemp = NHibernateQuerier.CriteriaSelect<AllianceConstruction>(nHCriteriaAlliancesConstruction);
                var exist = RedisHelper.KeyExistsAsync("AllianceSigninDTO" + allianceTemp.ID).Result;
                Alliancelist.Add(Utility.Json.ToJson(roleAllianceDTO));
                if (allianceConstructionTemp != null)
                {
                    Alliancelist.Add(Utility.Json.ToJson(allianceConstructionTemp));
                    //if (content)
                    //{
                    //    Alliancelist.Add(Utility.Json.ToJson(allianceConstructionTemp));
                    //}
                    //else
                    //{
                    //    #region 每日扣除逻辑
                    //    if (allianceConstructionTemp.AllianceAssets >= allianceLevleUpDataDict[allianceConstructionTemp.AllianceChamber].Daily_Fee_Spirit_Stones)
                    //    {
                    //        allianceConstructionTemp.AllianceAssets -= allianceLevleUpDataDict[allianceConstructionTemp.AllianceChamber].Daily_Fee_Spirit_Stones;
                    //    }
                    //    else
                    //    {
                    //        allianceTemp.Popularity -= allianceLevleUpDataDict[allianceConstructionTemp.AllianceChamber].Daily_Fee_Spirit_Stones / 1000;
                    //    }

                    //    NHibernateQuerier.UpdateAsync(allianceConstructionTemp);
                    //    int h = 23 - DateTime.Now.Hour;
                    //    int m = 59 - DateTime.Now.Minute;
                    //    int s = 60 - DateTime.Now.Second;
                    //    RedisHelper.String.StringSetAsync("AllianceConstructionDTO" + roleallianceObj.AllianceID, "AllianceConstructionDTO" + roleallianceObj.AllianceID, new TimeSpan(0, h, m, s));
                    //    #endregion
                    //}

                }
                if (allianceTemp != null)
                {
                   Alliancelist.Add(Utility.Json.ToJson(allianceTemp));
                    //if (content)
                    //{
                    //    Alliancelist.Add(Utility.Json.ToJson(allianceTemp));
                    //}
                    //else
                    //{
                    //    #region 每日扣除逻辑
                    //    allianceTemp.Popularity -= allianceLevleUpDataDict[allianceConstructionTemp.AllianceChamber].Daily_Fee;
                    //    NHibernateQuerier.UpdateAsync(allianceTemp);
                    //    int h = 23 - DateTime.Now.Hour;
                    //    int m = 59 - DateTime.Now.Minute;
                    //    int s = 60 - DateTime.Now.Second;
                    //    RedisHelper.String.StringSetAsync("AllianceConstructionDTO" + roleallianceObj.AllianceID, "AllianceConstructionDTO" + roleallianceObj.AllianceID, new TimeSpan(0, h, m, s));
                    //    #endregion
                    //}
                }

                if (exist)
                {
                    var allianceSignin = RedisHelper.String.StringGet("AllianceSigninDTO" + allianceTemp.ID);
                    Alliancelist.Add(allianceSignin);
                }
                else
                    Alliancelist.Add(Utility.Json.ToJson(new AllianceSigninDTO() { }));
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.RoleAlliance, Utility.Json.ToJson(Alliancelist));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
                ReferencePool.Release(nHCriteriaroleAlliances, nHCriteriaAlliances, nHCriteriaAlliancesConstruction);
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


        /// <summary>
        /// 判断人气不够解散的逻辑
        /// </summary>
        public void GetPopularity(AllianceStatus allianceStatus , int popularityNum,int aesstNum)
        {
            //判断是否存在人气不够解散的redishuancun
            var Exists = RedisHelper.KeyExistsAsync("AllianceStatus" + allianceStatus.ID).Result;
            if (allianceStatus.Popularity >= popularityNum)
            {
                allianceStatus.Popularity -= popularityNum;
               var hasKey= RedisHelper.KeyDeleteAsync("AllianceStatus" + allianceStatus.ID).Result;
            }
            else
            {
                if (Exists)
                {
                 var stockpileTime=   RedisHelper.Hash.HashGet<DateTime>("AllianceStatus" + allianceStatus.ID, allianceStatus.ID.ToString());

                    int sumSeconds = stockpileTime.Subtract(DateTime.Now).Seconds;
                    if (sumSeconds>=24*60*60*3)
                    {
                            //TODO
                            //使用bool值判断是否解散仙盟,派发请求主动解散仙盟
                    }
                }
                else
                {
                    RedisHelper.Hash.HashSetAsync<DateTime>("AllianceStatus" + allianceStatus.ID, allianceStatus.ID.ToString(), DateTime.Now);
                }
            }
        }

    }
}


