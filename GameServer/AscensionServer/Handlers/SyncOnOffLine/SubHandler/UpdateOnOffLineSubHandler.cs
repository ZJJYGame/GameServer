/*
*Author : yingduan
*Since :	2020-05-26
*Description :  离线时间记录
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionServer.Model;
using Cosmos;
namespace AscensionServer
{
   public class UpdateOnOffLineSubHandler : SyncOnOffLineSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string subDataJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.OnOffLine));
            var onofflinetemp = Utility.Json.ToObject<OnOffLine>(subDataJson);
            NHCriteria nHCriteriaOnoff =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", onofflinetemp.RoleID);
            var obj = NHibernateQuerier.CriteriaSelect<OnOffLine>(nHCriteriaOnoff);
            if (obj != null)
            {
                obj.MsGfID = onofflinetemp.MsGfID;
                obj.ExpType = onofflinetemp.ExpType;
                NHibernateQuerier.Update(obj);
                SetResponseParamters(() =>
                {
                    operationResponse.ReturnCode = (byte)ReturnCode.Success;
                });
            }
            else
            {
                NHibernateQuerier.Insert(onofflinetemp);
            }
            ReferencePool.Release(nHCriteriaOnoff);
            return operationResponse;
        }

       
    }
}


