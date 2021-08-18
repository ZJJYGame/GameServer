using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
using AscensionProtocol.DTO;
using RedisDotNet;


namespace AscensionServer
{
    public class AddBottleneckSubHandler : SyncBottleneckSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string bottleneckJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleBottleneck));
            var bottleneckObj = Utility.Json.ToObject<Bottleneck>(bottleneckJson);
            NHCriteria nHCriteriabottleneck =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", bottleneckObj.RoleID);
            GameEntry. DataManager.TryGetValue<Dictionary<int, BottleneckData>>(out var bottleneckData);
            GameEntry. DataManager.TryGetValue<Dictionary<int, DemonData>>(out var demonData);
            if (RedisHelper.Hash.HashExist("Bottleneck", bottleneckObj.RoleID.ToString()))
            {
                Utility.Debug.LogInfo("yzqData得到的瓶颈数据，进入redis判断" + bottleneckJson);
                #region Redis 逻辑
                var bottleneckRedis = RedisHelper.Hash.HashGet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString());
                //判断是否有瓶颈
                List<int> rootPercentNum;
                GetRootPercent(bottleneckData[bottleneckObj.RoleLevel], bottleneckObj.SpiritualRootVaule, out rootPercentNum);
                Utility.Debug.LogInfo("yzqData得到的Redis瓶颈值概率" + GetPercent(rootPercentNum[0] / (float)100));
                if (GetPercent(rootPercentNum[0] / (float)100))
                {
                    bottleneckRedis.IsBottleneck = true;
                    bottleneckRedis.BreakThroughVauleMax = rootPercentNum[1];
                    if (bottleneckData[bottleneckObj.RoleLevel].IsFinalLevel)
                    {
                        bottleneckRedis.IsThunder = true;
                        bottleneckRedis.ThunderRound = bottleneckData[bottleneckObj.RoleLevel].Thunder_Round;
                        int demonIndex = GetDemonPercent(demonData[bottleneckObj.RoleLevel], bottleneckRedis.CraryVaule);
                        if (GetPercent(demonData[bottleneckObj.RoleLevel].Trigger_Chance[demonIndex] / (float)100))
                        {
                            bottleneckRedis.IsDemon = true;
                            bottleneckRedis.DemonID = demonData[bottleneckObj.RoleLevel].Demon_ID[demonIndex];
                        }
                    }
                    RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckRedis);
                    SetResponseParamters(() => {
                        Utility.Debug.LogInfo("1yzqData返回的瓶颈值概率");
                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckRedis));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
                else
                {
                    NHibernateQuerier.Update<Bottleneck>(bottleneckRedis);
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckRedis));
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                    ReferencePool.Release(nHCriteriabottleneck);
                }
                #endregion
            }
            else
            {
                Utility.Debug.LogInfo("yzqData得到的瓶颈数据，进入数据判断" + bottleneckJson);
                #region 数据库逻辑
                var bottleneckTemp = NHibernateQuerier.CriteriaSelect<Bottleneck>(nHCriteriabottleneck);
                //判断是否有瓶颈
                List<int> rootPercentNum;
                GetRootPercent(bottleneckData[bottleneckObj.RoleLevel], bottleneckObj.SpiritualRootVaule, out rootPercentNum);
                var percent = GetPercent(rootPercentNum[0] / (float)100);
                Utility.Debug.LogInfo("yzqData得到的数据库逻辑瓶颈值概率" + percent);
                if (percent)
                {
                    bottleneckTemp.IsBottleneck = true;
                    bottleneckTemp.BreakThroughVauleMax = rootPercentNum[1];
                    if (bottleneckData[bottleneckObj.RoleLevel].IsFinalLevel)
                    {
                        bottleneckTemp.IsThunder = true;
                        bottleneckTemp.ThunderRound = bottleneckData[bottleneckObj.RoleLevel].Thunder_Round;
                        int demonIndex = GetDemonPercent(demonData[bottleneckObj.RoleLevel], bottleneckTemp.CraryVaule);
                        if (GetPercent(demonData[bottleneckObj.RoleLevel].Trigger_Chance[demonIndex] / (float)100))
                        {
                            bottleneckTemp.IsDemon = true;
                            bottleneckTemp.DemonID = demonData[bottleneckObj.RoleLevel].Demon_ID[demonIndex];
                        }
                    }
                    NHibernateQuerier.Update<Bottleneck>(bottleneckTemp);
                    SetResponseParamters(() => {
                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
                else
                {
                    RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckTemp);
                    Utility.Debug.LogInfo("2yzqData返回的瓶颈值概率");
                  SetResponseParamters(() => {
                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                    ReferencePool.Release(nHCriteriabottleneck);
                }
                #endregion
            }
            return operationResponse;
        }

        //获取概率值
        private bool GetPercent(float num)
        {
            Random random = new Random();
            int randommNum = random.Next(1, 10001);
            float percentNum = num * 10000;
            Utility.Debug.LogInfo("yzqData概率值为" + percentNum + "当前值为" + randommNum);
            if (randommNum <= (int)percentNum)
            {
                return true;
            }else
                return false;
        }
        /// <summary>
        /// 获取零灵根概率
        /// </summary>
        /// <param name="bottleneckData"></param>
        /// <param name="rootnum"></param>
        /// <returns></returns>
        private void GetRootPercent(BottleneckData bottleneckData,int rootnum,out List<int> num)
        {
            switch (rootnum)
            {
                case 1:
                    num = bottleneckData.Spiritual_Root_1 ;
                    break;
                case 2:
                    num = bottleneckData.Spiritual_Root_2;
                    break  ;
                case 3:
                    num = bottleneckData.Spiritual_Root_3;
                    break  ;
                case 4:
                    num = bottleneckData.Spiritual_Root_4;
                    break  ;
                case 5:
                    num = bottleneckData.Spiritual_Root_5;
                    break  ;
                default:
                    num =null;
                    break  ;
            }
             
        }
        /// <summary>
        /// 获取心魔对应数组的下标
        /// </summary>
        /// <param name="demonData"></param>
        /// <param name="CraryVaule"></param>
        /// <returns></returns>
        private int GetDemonPercent(DemonData demonData, int CraryVaule)
        {
            int index=0;
            for (int i = 0; i < demonData.Crary_Value.Count; i++)
            {
                if (CraryVaule>=demonData.Crary_Value[i]&& CraryVaule<= demonData.Crary_Value[i+1])
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}


