//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AscensionProtocol;
//using Photon.SocketServer;
//using AscensionServer.Model;
//using Cosmos;
//using AscensionProtocol.DTO;
//using RedisDotNet;
//namespace AscensionServer
//{
//    public class UpdateDailyMessageSubHandler : SyncDailyMessageSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

//        public  override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string dailyMessageJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.DailyMessage));
//            var dailyMessageObj = Utility.Json.ToObject<DailyMessageDTO>(dailyMessageJson);
//            string indexJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AllianceName));
//            var index = Utility.Json.ToObject<int>(indexJson);
//            string countJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AllianceMember));
//            var count = Utility.Json.ToObject<int>(countJson);
//            NHCriteria nHCriteriadailyMessage =ReferencePool.Accquire<NHCriteria>().SetValue("RoleName", dailyMessageObj.Name);
//            var dailyMessageTemp = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriadailyMessage);
//            List<DailyMessageDTO> dailyMessages = new List<DailyMessageDTO>();

//            if (dailyMessageTemp != null)
//            {
//                if (RedisHelper.Hash.HashExistAsync("DailyMessage", dailyMessageTemp.AllianceID.ToString()).Result)
//                {
//                    dailyMessages = RedisHelper.Hash.HashGetAsync<List<DailyMessageDTO>>("DailyMessage", dailyMessageTemp.AllianceID.ToString()).Result;
//                    if (dailyMessages.Count > index)
//                    {
//                        //Utility.Debug.LogInfo("yzqData" + "当前的redis长度为" + dailyMessages.Count + "长度为" + index + "下表为" + count);
//                        if (dailyMessages.Count- index > count)
//                        {
//                            //Utility.Debug.LogInfo("yzqData" + "1收到的消息返回成功" + Utility.Json.ToJson(dailyMessages.GetRange(index,count- index)));
//                            SetResponseParamters(() =>
//                            {
//                                subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages.GetRange(index, count-index)));
//                                operationResponse.ReturnCode = (short)ReturnCode.Success;
//                            });
//                        }
//                        else
//                        {
//                            //Utility.Debug.LogInfo("yzqData" + "2收到的消息返回成功"+ dailyMessages.GetRange(index, dailyMessages.Count-index).Count);
//                            SetResponseParamters(() =>
//                            {                            
//                                subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages.GetRange(index, dailyMessages.Count - index)));
//                                operationResponse.ReturnCode = (short)ReturnCode.Success;
//                            });
//                        }
//                    }
//                    else
//                    {
//                        SetResponseParamters(() => { operationResponse.ReturnCode = (short)ReturnCode.Fail; });
 
//                    }

//                }
//                else
//                    SetResponseParamters(() =>
//                    {
//                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
//                        Utility.Debug.LogInfo("yzqData" + "收到的消息返回失败2");
//                    });

//            }
//            else
//            {
//                SetResponseParamters(() => { operationResponse.ReturnCode = (short)ReturnCode.Fail; });
//                Utility.Debug.LogInfo("yzqData" + "收到的消息返回失败3");
//            }
//            return operationResponse;
//        }
//    }
//}


