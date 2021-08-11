using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Photon.SocketServer;
using AscensionServer.Model;
using AscensionProtocol.DTO;
using Cosmos;
using RedisDotNet;
namespace AscensionServer
{
    public class GetTreasureatticHandler : SyncTreasureatticSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string treasureatticJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.School));


            var schoolObj = Utility.Json.ToObject<SchoolDTO>(treasureatticJson);
            TreasureatticDTO treasureatticObj = new TreasureatticDTO();
            NHCriteria nHCriteriaTreasureattic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.TreasureAtticID);

  
            var treasureatticTemp = NHibernateQuerier.CriteriaSelect<Treasureattic>(nHCriteriaTreasureattic);

            var content = RedisData.ReidsDataProcessing.GetData("TreasureatticDTO"+ treasureatticTemp.ID);

            Utility.Debug.LogInfo("yzqData查找的key值" + "TreasureatticDTO" + treasureatticTemp.ID +"内容为"+ content);
            if (string.IsNullOrEmpty(content))
            {
                if (RedisHelper.Hash.HashExist("Treasureattic", treasureatticTemp.ID.ToString()))
                {
                    var contentTreasureattic = RedisHelper.Hash.HashGet<Dictionary<int, int>>("Treasureattic", treasureatticTemp.ID.ToString());
                    treasureatticObj.ID = treasureatticTemp.ID;
                    treasureatticObj.ItemRedeemedDict = contentTreasureattic;
                    treasureatticObj.ItemNotRefreshDict = contentTreasureattic;
                }

            }
            else
            {
                treasureatticObj.ItemRedeemedDict = Utility.Json.ToObject<Dictionary<int, int>>(content);
                treasureatticObj.ID = treasureatticTemp.ID;
            }
            SetResponseParamters(() =>
            {
                Utility.Debug.LogInfo(">>>>>>>返回加入宗门的数据" + Utility.Json.ToJson(treasureatticObj));
                subResponseParameters.Add((byte)ParameterCode.TreasureAttic, Utility.Json.ToJson(treasureatticObj));
                operationResponse.ReturnCode = (byte)ReturnCode.Success;
            });

            return operationResponse;
        }
    }
}

