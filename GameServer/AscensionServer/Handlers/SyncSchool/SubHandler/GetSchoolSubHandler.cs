using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;

namespace AscensionServer
{
    public class GetSchoolSubHandler : SyncSchoolSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string schoolJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.School));
            var schoolObj = Utility.Json.ToObject<School>(schoolJson);
            NHCriteria nHCriteriaSchool =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.ID);
            var schoolTemp = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaSchool);
            List<string > DTOList = new List<string>(); ;
            if (schoolTemp!=null)
            {
                DTOList.Clear();
                DTOList.Add(Utility.Json.ToJson( schoolTemp));
                NHCriteria nHCriteriaTreasureattic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolTemp.TreasureAtticID);
                var TreasureatticTemp = NHibernateQuerier.CriteriaSelect<Treasureattic>(nHCriteriaTreasureattic);
                if (TreasureatticTemp!=null)
                {
                    DTOList.Add(Utility.Json.ToJson(new TreasureatticDTO() { ID = TreasureatticTemp.ID, ItemAmountDict = Utility.Json.ToObject<Dictionary<int, int>>(TreasureatticTemp.ItemAmountDict), ItemRedeemedDict = Utility.Json.ToObject<Dictionary<int, int>>(TreasureatticTemp.ItemRedeemedDict) }));
                }
                NHCriteria nHCriteriaSutrasAttic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolTemp.SutrasAtticID);
                var SutrasAtticTemp = NHibernateQuerier.CriteriaSelect<SutrasAttic>(nHCriteriaSutrasAttic);

                if (SutrasAtticTemp != null)
                {
                    DTOList.Add(Utility.Json.ToJson(new SutrasAtticDTO() { ID = SutrasAtticTemp.ID, SutrasAmountDict = Utility.Json.ToObject<Dictionary<int, int>>(SutrasAtticTemp.SutrasAmountDict), SutrasRedeemedDictl = Utility.Json.ToObject<Dictionary<int, int>>(SutrasAtticTemp.SutrasRedeemedDictl) }));
                }
                ReferencePool.Release(nHCriteriaTreasureattic, nHCriteriaSutrasAttic);
            }
            else     
                SetResponseParamters(() => {operationResponse.ReturnCode = (byte)ReturnCode.Fail; });
            if (DTOList.Count >0)
            {
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.School, Utility.Json.ToJson(DTOList));
                    operationResponse.ReturnCode = (byte)ReturnCode.Success;
                });
            }
            else
            {
                SetResponseParamters(() => { operationResponse.ReturnCode = (byte)ReturnCode.Fail; });
            }
            ReferencePool.Release(nHCriteriaSchool);
            return operationResponse;
        }
    }
}


