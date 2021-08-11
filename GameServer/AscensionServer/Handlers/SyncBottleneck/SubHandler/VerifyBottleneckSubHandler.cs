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
    class VerifyBottleneckSubHandler : SyncBottleneckSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Verify;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string bottleneckJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleBottleneck));
            var bottleneckObj = Utility.Json.ToObject<Bottleneck>(bottleneckJson);
            NHCriteria nHCriteriabottleneck =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", bottleneckObj.RoleID);

            var roleTemp = NHibernateQuerier.CriteriaSelect<Role>(nHCriteriabottleneck);
            GameEntry. DataManager.TryGetValue<Dictionary<int, BottleneckData>>(out var bottleneckData);

            GameEntry. DataManager.TryGetValue<Dictionary<int, DemonData>>(out var demonData);

            //Utility.Debug.LogInfo("yzqData数据库获得的数据" + Utility.Json.ToJson(bottleneckTemp));
            if (RedisHelper.Hash.HashExist("Bottleneck", bottleneckObj.RoleID.ToString()))
            {
                Utility.Debug.LogInfo("yzqData传过来的瓶颈数据进Redis判断" + bottleneckJson);
                var bottleneckRedis = RedisHelper.Hash.HashGet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString());
                #region Redis数据
                if (bottleneckRedis != null)
                {
                    if (bottleneckRedis.BreakThroughVauleMax <= bottleneckObj.BreakThroughVauleNow)
                    {
                        if (bottleneckData[roleTemp.RoleLevel].IsFinalLevel)
                        {
                            if (bottleneckRedis.IsDemon)
                            {
                                bottleneckRedis.BreakThroughVauleNow = 0;
                                bottleneckRedis.DrugNum = 0;
                                bottleneckRedis.IsDemon = false;
                                NHibernateQuerier.Update<Bottleneck>(bottleneckRedis);
                                RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckRedis);
                                SetResponseParamters(() => {
                                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckRedis));
                                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                                });
                            }
                            else
                            {
                                bottleneckRedis.BreakThroughVauleNow = 0;
                                bottleneckRedis.DrugNum = 0;
                                bottleneckRedis.IsDemon = false;
                                bottleneckRedis.IsThunder = false;
                                bottleneckRedis.IsBottleneck = false;
                                NHibernateQuerier.Update<Bottleneck>(bottleneckRedis);
                                RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckRedis);
                                SetResponseParamters(() => {
                                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckRedis));
                                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                                });
                            }
                        }
                        else
                        {
                            bottleneckRedis.BreakThroughVauleNow = 0;
                            bottleneckRedis.IsBottleneck = false;
                            bottleneckRedis.DrugNum = 0;
                            NHibernateQuerier.Update<Bottleneck>(bottleneckRedis);
                            RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckRedis);
                            SetResponseParamters(() => {
                                subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckRedis));
                                operationResponse.ReturnCode = (short)ReturnCode.Success;
                            });
                        }
                    }
                    else
                    {
                        SetResponseParamters(() => {
                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
                        });
                    }
                }
                else
                    SetResponseParamters(() => {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                #endregion
            }
            else
            {
                var bottleneckTemp = NHibernateQuerier.CriteriaSelect<Bottleneck>(nHCriteriabottleneck);
                #region 数据库数据
                if (bottleneckTemp != null)
                {
                    Utility.Debug.LogInfo("yzqData传过来的瓶颈数据进数据库判断" + bottleneckJson);
                    if (bottleneckTemp.BreakThroughVauleMax <= bottleneckObj.BreakThroughVauleNow)
                    {
                        if (bottleneckData[roleTemp.RoleLevel].IsFinalLevel)
                        {
                            if (bottleneckTemp.IsDemon)
                            {
                                bottleneckTemp.BreakThroughVauleNow = 0;
                                bottleneckTemp.IsDemon = false;
                                bottleneckTemp.DrugNum = 0;
                                NHibernateQuerier.Update<Bottleneck>(bottleneckTemp);
                                RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckTemp);
                                SetResponseParamters(() => {
                                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
                                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                                });
                            }
                            else
                            {
                                bottleneckTemp.BreakThroughVauleNow = 0;
                                bottleneckTemp.DrugNum = 0;
                                bottleneckTemp.IsDemon = false;
                                bottleneckTemp.IsThunder = false;
                                bottleneckTemp.IsBottleneck = false;
                                NHibernateQuerier.Update<Bottleneck>(bottleneckTemp);
                                RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckTemp);
                                SetResponseParamters(() => {
                                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
                                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                                });
                            }
                        }
                        else
                        {
                            bottleneckTemp.BreakThroughVauleNow = 0;
                            bottleneckTemp.DrugNum = 0;
                            bottleneckTemp.IsBottleneck = false;
                            NHibernateQuerier.Update<Bottleneck>(bottleneckTemp);
                            RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckTemp);
                            SetResponseParamters(() => {
                                subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
                                operationResponse.ReturnCode = (short)ReturnCode.Success;
                            });
                        }
                    }
                    else
                    {
                        SetResponseParamters(() => {
                            operationResponse.ReturnCode = (short)ReturnCode.Fail;
                        });
                    }
                }
                else
                    SetResponseParamters(() => {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                #endregion
            }
            return operationResponse;
        }
    }
}


