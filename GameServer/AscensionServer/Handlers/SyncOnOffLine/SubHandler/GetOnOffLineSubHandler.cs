/*
*Author : yingduan
*Since :	2020-06-1
*Description :  离线经验
*/
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
namespace AscensionServer
{
   public  class GetSyncOnOffLineSubHandler : SyncOnOffLineSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            //        var dict = operationRequest.Parameters;
            //        string subDataJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.OnOffLine));
            //        var onofflinetemp = Utility.Json.ToObject<OnOffLine>(subDataJson);
            //        Bottleneck bottleneck = new Bottleneck() {RoleID= onofflinetemp.RoleID };
            //        NHCriteria nHCriteriabottleneck =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", bottleneck.RoleID);
            //        NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", onofflinetemp.RoleID);
            //        var bottleneckObj= NHibernateQuerier.CriteriaSelect<Bottleneck>(nHCriteriabottleneck);
            //        ///获取的时间秒
            //        OffLineTimeDTO offLineTime = new OffLineTimeDTO() { RoleID = onofflinetemp.RoleID };
            //        var obj = NHibernateQuerier.CriteriaSelect<OffLineTime>(nHCriteriaRole);
            //        TimeSpan interval = (DateTime.Now).Subtract(Convert.ToDateTime(obj.OffTime));

            //        if (obj != null)
            //        {
            //            var Exptypeobj = NHibernateQuerier.CriteriaSelect<OnOffLine>(nHCriteriaRole);
            //            if (Exptypeobj.ExpType==1)

            //            {
            //                List<int> date = new List<int>();
            //                int AllExperience = (int)(onofflinetemp.GongFaExp * interval.TotalSeconds / 5);
            //                date.Add(AllExperience);
            //                date.Add(Exptypeobj.MsGfID);
            //                date.Add(Exptypeobj.ExpType);
            //                if (bottleneckObj != null)
            //                {
            //                    SetResponseParamters(() =>
            //                    {
            //                        subResponseParameters.Add((byte)ParameterCode.OnOffLine, Utility.Json.ToJson(date));
            //                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckObj));
            //                        operationResponse.ReturnCode = (short)ReturnCode.Success;
            //                    });
            //                }
            //                else
            //                {
            //                    Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>得到的离线时间1" + Exptypeobj.MsGfID + "id" + Exptypeobj.ExpType);
            //                    SetResponseParamters(() =>
            //                    {
            //                        subResponseParameters.Add((byte)ParameterCode.OnOffLine, Utility.Json.ToJson(date));
            //                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(new BottleneckDTO()));
            //                        operationResponse.ReturnCode = (short)ReturnCode.Success;
            //                    });
            //                }
            //            }
            //            else if (Exptypeobj.ExpType==2)

            //            {
            //                List<int> date = new List<int>();
            //                int AllExperience =(int)(onofflinetemp.MiShuExp * interval.TotalSeconds / 5);
            //                date.Add(AllExperience);
            //                date.Add(Exptypeobj.MsGfID);
            //                date.Add(Exptypeobj.ExpType);
            //                if (bottleneckObj != null)
            //                {
            //                    SetResponseParamters(() =>
            //                    {
            //                        subResponseParameters.Add((byte)ParameterCode.OnOffLine, Utility.Json.ToJson(date));
            //                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(bottleneckObj));
            //                        operationResponse.ReturnCode = (short)ReturnCode.Success;
            //                    });
            //                }
            //                else
            //                {
            //                    Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>得到的离线时间2" + Exptypeobj.MsGfID + "id" + Exptypeobj.ExpType);
            //                    SetResponseParamters(() =>
            //                    {
            //                        subResponseParameters.Add((byte)ParameterCode.OnOffLine, Utility.Json.ToJson(date));
            //                        subResponseParameters.Add((byte)ParameterCode.RoleBottleneck, Utility.Json.ToJson(new BottleneckDTO()));
            //                        operationResponse.ReturnCode = (short)ReturnCode.Success;
            //                    });
            //                }
            //            }
            //            else
            //            {
            //                SetResponseParamters(() =>
            //                {
            //                    operationResponse.ReturnCode = (short)ReturnCode.Fail;
            //                });
            //            }
            //            }else{
            //            SetResponseParamters(() =>
            //            {
            //            operationResponse.ReturnCode = (short)ReturnCode.Fail;
            //            });
            //            }
            //        ReferencePool.Release(nHCriteriaRole);
            return operationResponse;
        }
    }
}



