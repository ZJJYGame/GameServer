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
//  public class AddDailyMessageSubHandler: SyncDailyMessageSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string dailyMessageJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.DailyMessage));
//            var dailyMessageObj = Utility.Json.ToObject<DailyMessageDTO>(dailyMessageJson);
//            NHCriteria nHCriteriadailyMessage =ReferencePool.Accquire<NHCriteria>().SetValue("RoleName", dailyMessageObj.Name);
//            var dailyMessageTemp = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriadailyMessage);
//            List<DailyMessageDTO> dailyMessages = new List<DailyMessageDTO>();
//            if (dailyMessageTemp != null)
//            {
//                if (RedisHelper.Hash.HashExistAsync("DailyMessage", dailyMessageTemp.AllianceID.ToString()).Result)
//                {
//                    dailyMessages = RedisHelper.Hash.HashGetAsync<List<DailyMessageDTO>>("DailyMessage", dailyMessageTemp.AllianceID.ToString()).Result;
//                    dailyMessages.Add(dailyMessageObj);
//                    RedisHelper.Hash.HashSet<List<DailyMessageDTO>>("DailyMessage", dailyMessageTemp.AllianceID.ToString(), dailyMessages);
//                     SetResponseParamters(() =>
//                    {
//                        subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages));
//                        operationResponse.ReturnCode = (short)ReturnCode.Success;
//                    });
//                }
//                else
//                    dailyMessages.Add(dailyMessageObj);
//                RedisHelper.Hash.HashSet<List<DailyMessageDTO>>("DailyMessage", dailyMessageTemp.AllianceID.ToString(), dailyMessages);
//                SetResponseParamters(() => {
//                    subResponseParameters.Add((byte)ParameterCode.DailyMessage, Utility.Json.ToJson(dailyMessages));
//                    operationResponse.ReturnCode = (short)ReturnCode.Success;
//                });
//            }
//            else
//                SetResponseParamters(() => { operationResponse.ReturnCode = (short)ReturnCode.Fail; });

//            return operationResponse;
//        }
//    }
//}


