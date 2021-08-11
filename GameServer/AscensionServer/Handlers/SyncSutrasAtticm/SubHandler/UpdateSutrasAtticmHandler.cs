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
    public class UpdateSutrasAtticmHandler : SyncSutrasAtticmSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string sutrasAtticJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.SutrasAtticm));
            string schoolJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.School));

            var sutrasAtticObj = Utility.Json.ToObject<SutrasAtticDTO>(sutrasAtticJson);
            var schoolObj = Utility.Json.ToObject<School>(schoolJson);
            NHCriteria nHCriteriasutrasAttic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", sutrasAtticObj.ID);
            Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>" + Utility.Json.ToJson(schoolObj.SutrasAtticID) + "更新2藏宝阁" + Utility.Json.ToJson(sutrasAtticObj.SutrasRedeemedDictl));
            NHCriteria nHCriteriaschool =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.ID);
            var sutrasAtticTemp = NHibernateQuerier.CriteriaSelect<SutrasAttic>(nHCriteriasutrasAttic);
            var schoolTemp = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaschool);
            var exit = NHibernateQuerier.Verify<SutrasAttic>(nHCriteriasutrasAttic);
            int contribution = 0;

            Dictionary<int, int> itemDict = new Dictionary<int, int>();
            Dictionary<string, string> DOdict = new Dictionary<string, string>();
            if (schoolTemp != null)
            {
                if (schoolTemp.ContributionNow > schoolObj.ContributionNow)
                {
                    contribution = schoolTemp.ContributionNow - schoolObj.ContributionNow;
                    NHibernateQuerier.Update<School>(new School() { ID = schoolTemp.ID, SchoolID = schoolTemp.SchoolID, SchoolJob = schoolTemp.SchoolJob, TreasureAtticID = schoolTemp.TreasureAtticID, SutrasAtticID = schoolTemp.SutrasAtticID, ContributionNow = contribution });
                    if (exit)
                    {
                        if (!string.IsNullOrEmpty(sutrasAtticTemp.SutrasRedeemedDictl))
                        {

                            itemDict = Utility.Json.ToObject<Dictionary<int, int>>(sutrasAtticTemp.SutrasRedeemedDictl);
                            Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>更新藏宝阁key" + Utility.Json.ToJson(itemDict));
                            foreach (var item in sutrasAtticObj.SutrasRedeemedDictl)
                            {
                                Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>更新藏宝阁key" + item.Key);
                                if (itemDict.ContainsKey(item.Key))
                                {
                                    itemDict[item.Key] += sutrasAtticObj.SutrasRedeemedDictl[item.Key];
                                }
                                else
                                { itemDict.Add(item.Key, sutrasAtticObj.SutrasRedeemedDictl[item.Key]); }

                            }
                        }
                        else
                            itemDict = sutrasAtticObj.SutrasRedeemedDictl;
                        NHibernateQuerier.Update<SutrasAttic>(new SutrasAttic() { ID = sutrasAtticTemp.ID, SutrasAmountDict= sutrasAtticTemp.SutrasAmountDict, SutrasRedeemedDictl = Utility.Json.ToJson(itemDict) });
                    }
                    var saSendObj = NHibernateQuerier.CriteriaSelect<SutrasAttic>(nHCriteriasutrasAttic);
                    var sSendObj = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaschool);
                    DOdict.Add("SutrasAttic", Utility.Json.ToJson(new SutrasAtticDTO() { ID = saSendObj.ID, SutrasRedeemedDictl = Utility.Json.ToObject<Dictionary<int, int>>(saSendObj.SutrasRedeemedDictl) }));
                    DOdict.Add("School", Utility.Json.ToJson(sSendObj));
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.SutrasAtticm, Utility.Json.ToJson(DOdict));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                }
                else
                {
                    SetResponseParamters(() =>
                    {
                        Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>传回到的藏宝阁s失败");
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                }
            }
            else
            {
                SetResponseParamters(() =>
                {
                    Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>传回到的藏宝阁s失败");
                    operationResponse.ReturnCode = (short)ReturnCode.Fail;
                });
            }
            ReferencePool.Release(nHCriteriasutrasAttic, nHCriteriaschool);
            Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>传回到的藏宝阁" + Utility.Json.ToJson(DOdict));
            return operationResponse;
        }
    }
}


