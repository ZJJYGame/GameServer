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
    public class UpdateTreasureatticHandler : SyncTreasureatticSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {

            var dict = operationRequest.Parameters;

            string treasureatticJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.TreasureAttic));
            string schoolJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.School));

            var treasureatticObj = Utility.Json.ToObject<TreasureatticDTO>(treasureatticJson);
            var schoolObj = Utility.Json.ToObject<School>(schoolJson);

            NHCriteria nHCriteriaTreasureattic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", treasureatticObj.ID);
            NHCriteria nHCriteriaschool =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.ID);

            var treasureatticTemp = NHibernateQuerier.CriteriaSelect<Treasureattic>(nHCriteriaTreasureattic);
            var schoolTemp = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaschool);
            Dictionary<int, int> itemDict = new Dictionary<int, int>();
            Dictionary<int, int> itemnotrefreshDict = new Dictionary<int, int>();
            Dictionary<string, string> DOdict = new Dictionary<string, string>();


            if (schoolTemp != null)
            {

                Utility.Debug.LogInfo("yzqData兑换藏宝阁物品进来了" + Utility.Json.ToJson(treasureatticObj) + "宗门id为" + Utility.Json.ToJson(schoolObj));
  
                if (schoolTemp.ContributionNow >= schoolObj.ContributionNow)
                {
                    schoolTemp.ContributionNow = schoolTemp.ContributionNow - schoolObj.ContributionNow;
                    NHibernateQuerier.Update<School>(schoolTemp);

                    if (treasureatticTemp != null)
                    {

                        itemDict = Utility.Json.ToObject<Dictionary<int, int>>(treasureatticTemp.ItemRedeemedDict);
                        foreach (var item in treasureatticObj.ItemRedeemedDict)
                        {
                            if (itemDict.ContainsKey(item.Key))
                            {
                                itemDict[item.Key] += item.Value;
                            }
                            else
                            {
                                itemDict.Add(item.Key, item.Value);
                            }
                        }

                        itemnotrefreshDict = Utility.Json.ToObject<Dictionary<int, int>>(treasureatticTemp.ItemNotRefreshDict);
                        Utility.Debug.LogInfo("yzqData兑换藏宝阁物品进来了2"+ itemnotrefreshDict.Count);
                        foreach (var item in treasureatticObj.ItemNotRefreshDict)
                        {
                            if (itemDict.ContainsKey(item.Key))
                            {
                                itemnotrefreshDict[item.Key] += item.Value;
                            }
                            else
                            {
                                itemnotrefreshDict.Add(item.Key, item.Value);
                            }
                        }
                        Utility.Debug.LogInfo("yzqData兑换藏宝阁物品进来了3");
                        #region  Redis数据部分
                        if (RedisHelper.KeyExistsAsync("TreasureatticDTO" + treasureatticTemp.ID).Result)
                        {
                            RedisHelper.String.StringSet<Dictionary<int, int>>("TreasureatticDTO" + treasureatticTemp.ID, itemDict, RedisHelper.KeyTimeToLiveAsync("TreasureatticDTO" + treasureatticTemp.ID).Result);
                        }
                        else
                        {
                            int h = 23 - DateTime.Now.Hour;
                            int m = 59 - DateTime.Now.Minute;
                            int s = 60 - DateTime.Now.Second;
                            RedisHelper.String.StringSet<Dictionary<int, int>>("TreasureatticDTO" + treasureatticTemp.ID, itemDict, new TimeSpan(0, h, m, s));
                        }
                        Utility.Debug.LogInfo("yzqData兑换藏宝阁物品进来了4" + itemDict.Count);
                        RedisHelper.Hash.HashSet<Dictionary<int, int>>("Treasureattic", treasureatticTemp.ID.ToString(), itemnotrefreshDict);
                        #endregion
                        treasureatticTemp.ItemNotRefreshDict = Utility.Json.ToJson(itemnotrefreshDict);
                        treasureatticTemp.ItemRedeemedDict = Utility.Json.ToJson(itemDict);
                        NHibernateQuerier.Update<Treasureattic>(treasureatticTemp);

                        treasureatticObj.ItemRedeemedDict = itemDict;
                        treasureatticObj.ItemNotRefreshDict = itemnotrefreshDict;
                        DOdict.Add("Treasureattic", Utility.Json.ToJson(treasureatticObj));
                        DOdict.Add("School", Utility.Json.ToJson(schoolTemp));

       
                        SetResponseParamters(() =>
                        {

                            subResponseParameters.Add((byte)ParameterCode.TreasureAttic, Utility.Json.ToJson(DOdict));
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
                }
                else
                {
                    SetResponseParamters(() =>
                    {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                }
            }
            else
            {
                SetResponseParamters(() =>
                {
                    operationResponse.ReturnCode = (short)ReturnCode.Fail;
                });
            }

            ReferencePool.Release(nHCriteriaTreasureattic, nHCriteriaschool);
            return operationResponse;
        }
    }
}


