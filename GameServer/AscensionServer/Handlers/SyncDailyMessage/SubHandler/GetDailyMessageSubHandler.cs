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
//   public  class GetDailyMessageSubHandler: SyncDailyMessageSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
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
//                Utility.Debug.LogInfo("yzqData" + "收到的消息仙盟id为" + dailyMessageTemp.AllianceID + "长度为" + count);
//                if (RedisHelper.Hash.HashExistAsync("DailyMessage", dailyMessageTemp.AllianceID.ToString()).Result)
//                {
//                    dailyMessages = RedisHelper.Hash.HashGetAsync<List<DailyMessageDTO>>("DailyMessage", dailyMessageTemp.AllianceID.ToString()).Result;
//                    if (dailyMessages.Count> index)
//                    {
//                        if (dailyMessages.Count> count)
//                        {
//                            SetResponseParamters(() =>
//                            {
//                                subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages.GetRange(index, count)));
//                                operationResponse.ReturnCode = (short)ReturnCode.Success;
//                            });
//                        }
//                        else
//                        {
//                            SetResponseParamters(() =>
//                            {
//                                subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages.GetRange(index, dailyMessages.Count)));
//                                operationResponse.ReturnCode = (short)ReturnCode.Success;
//                            });
//                        }
//                    }else
//                        SetResponseParamters(() => { operationResponse.ReturnCode = (short)ReturnCode.Empty; });
//                }
//                else
//                    SetResponseParamters(() =>
//                    {
//                        subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages));
//                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
//                    });
//            }
//            else
//                SetResponseParamters(() => { operationResponse.ReturnCode = (short)ReturnCode.Empty; });
//            return operationResponse;
//        }
//    }
//}


