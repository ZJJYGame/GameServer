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
    public class RemoveBottleneckSubHandler : SyncBottleneckSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Remove;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string bottleneckJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleBottleneck));
            var bottleneckObj = Utility.Json.ToObject<Bottleneck>(bottleneckJson);
            NHCriteria nHCriteriabottleneck =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", bottleneckObj.RoleID);

            if (RedisHelper.Hash.HashExist("Bottleneck", bottleneckObj.RoleID.ToString()))
            {
                var bottlenneckRedis = RedisHelper.Hash.HashGet<Bottleneck>("Bottleneck", bottleneckObj.RoleID.ToString());
                Random random = new Random();
                bottlenneckRedis.BreakThroughVauleNow = (random.Next(20, 50) * bottlenneckRedis.BreakThroughVauleMax) / 100;
                NHibernateQuerier.Update<Bottleneck>(bottlenneckRedis);
                SetResponseParamters(() => {
                    subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottlenneckRedis));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
            {
                #region 数据库逻辑
                var bottleneckTemp = NHibernateQuerier.CriteriaSelect<Bottleneck>(nHCriteriabottleneck);
                if (bottleneckTemp != null)
                {
                    Random random = new Random();
                    bottleneckTemp.BreakThroughVauleNow = (random.Next(20, 50) * bottleneckTemp.BreakThroughVauleMax) / 100;
                    NHibernateQuerier.Update<Bottleneck>(bottleneckTemp);
                    SetResponseParamters(() => {
                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckTemp));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
                else
                {
                    SetResponseParamters(() => {
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                }
                #endregion
            }
            return operationResponse;
        }
    }
}


