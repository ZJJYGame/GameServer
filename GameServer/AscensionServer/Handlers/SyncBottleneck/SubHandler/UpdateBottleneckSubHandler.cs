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
//    public class UpdateBottleneckSubHandler : SyncBottleneckSubHandler
//    {
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string bottleneckJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleBottleneck));
//            var bottleneckObj = Utility.Json.ToObject<Bottleneck>(bottleneckJson);
//            NHCriteria nHCriteriabottleneck =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", bottleneckObj.RoleID);

//            if (RedisHelper.Hash.HashExist("Bottleneck", bottleneckObj.RoleID.ToString()))
//            {
//                #region Redis逻辑
//                var bottleneckRedis = RedisHelper.Hash.HashGet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString());
//                if ((bottleneckRedis.BreakThroughVauleNow + bottleneckObj.BreakThroughVauleNow) >= bottleneckRedis.BreakThroughVauleMax)
//                {
//                    bottleneckRedis.BreakThroughVauleNow = bottleneckRedis.BreakThroughVauleMax;
//                }
//                else
//                {
//                    bottleneckRedis.BreakThroughVauleNow += bottleneckObj.BreakThroughVauleNow;
//                }
//                bottleneckRedis.DrugNum += bottleneckObj.DrugNum;
//                bottleneckRedis.CraryVaule += bottleneckObj.CraryVaule;
//                NHibernateQuerier.Update<Bottleneck>(bottleneckRedis);
//                RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckRedis);
//                SetResponseParamters(() => {
//                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckRedis));
//                    operationResponse.ReturnCode = (short)ReturnCode.Success;
//                });
//                #endregion
//            }
//            else
//            {
//                #region 数据库逻辑
//                var bottleneckTemp = NHibernateQuerier.CriteriaSelect<Bottleneck>(nHCriteriabottleneck); ;
//                if ((bottleneckTemp.BreakThroughVauleNow + bottleneckObj.BreakThroughVauleNow) >= bottleneckTemp.BreakThroughVauleMax)
//                {
//                    bottleneckTemp.BreakThroughVauleNow = bottleneckTemp.BreakThroughVauleMax;
//                }
//                else
//                {
//                    bottleneckTemp.BreakThroughVauleNow += bottleneckObj.BreakThroughVauleNow;
//                }
//                bottleneckTemp.DrugNum += bottleneckObj.DrugNum;
//                bottleneckTemp.CraryVaule += bottleneckObj.CraryVaule;
//                NHibernateQuerier.Update<Bottleneck>(bottleneckTemp);
//                RedisHelper.Hash.HashSet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString(), bottleneckTemp);
//                SetResponseParamters(() => {
//                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
//                    operationResponse.ReturnCode = (short)ReturnCode.Success;
//                });
//                #endregion
//            }
//            return operationResponse;
//        }
//    }
//}


