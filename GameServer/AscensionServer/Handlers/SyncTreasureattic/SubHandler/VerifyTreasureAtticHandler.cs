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
    public class VerifyTreasureAtticHandler : SyncTreasureatticSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Verify;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string schoolJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.TreasureAttic));
            var schoolObj = Utility.Json.ToObject<School>(schoolJson);
            NHCriteria nHCriteriaSchool =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.ID);
            var schoolTemp = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaSchool);
            if (schoolTemp!=null)
            {
                Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>数据库贡献点" + schoolTemp.ContributionNow + "传过来的贡献点" + schoolObj.ContributionNow);
                if (schoolTemp.ContributionNow > schoolObj.ContributionNow)
                {
                    //NHCriteria nHCriteriaTreasureAttic = Singleton<ReferencePoolManager>.Instance.Spawn<NHCriteria>().SetValue("ID", schoolTemp.TreasureAtticID);
                    //var TreasureAtticTemp = Singleton<NHManager>.Instance.CriteriaSelect<Treasureattic>(nHCriteriaTreasureAttic);
                    //if (Utility.Json.ToObject<Dictionary<int, int>>(TreasureAtticTemp.ItemAmountDict).TryGetValue(5, out int amout))
                    //{
                    //    //判断兑换数是否超过存储总数
                    //}
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.TreasureAttic, Utility.Json.ToJson(true));
                        operationResponse.ReturnCode = (byte)ReturnCode.Success;
                    });
                }
                else
                {
                    subResponseParameters.Add((byte)ParameterCode.TreasureAttic, Utility.Json.ToJson(false));
                    operationResponse.ReturnCode = (byte)ReturnCode.Fail;
                }
            }
            ReferencePool.Release(nHCriteriaSchool);
            return operationResponse;
        }
    }
}


