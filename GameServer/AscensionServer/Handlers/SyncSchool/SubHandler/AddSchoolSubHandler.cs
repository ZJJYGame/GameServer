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
using RedisDotNet;
namespace AscensionServer
{
    public class AddSchoolSubHandler : SyncSchoolSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;
        public  override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string schoolJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.School));
            var schoolObj = Utility.Json.ToObject<School>(schoolJson);

            Utility.Debug.LogInfo(">>>>>>>加入1宗门的请求收到了"+ schoolJson);
            string treasureatticJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.TreasureAttic));
            var treasureatticObj = Utility.Json.ToObject<TreasureatticDTO>(treasureatticJson);

            string sutrasAtticJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.SutrasAtticm));
            var sutrasAtticObj = Utility.Json.ToObject<SutrasAtticDTO>(sutrasAtticJson);

            NHCriteria nHCriteriaSchool =ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.ID);
            NHCriteria nHCriteriaTreasureattic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", treasureatticObj.ID);
            NHCriteria nHCriteriasutrasAttic =ReferencePool.Accquire<NHCriteria>().SetValue("ID", sutrasAtticObj.ID);

            GameEntry. DataManager.TryGetValue<Dictionary<int, List<FactionItem>>>(out var factionItemDict);
            GameEntry. DataManager.TryGetValue<Dictionary<int, List<FactionSkill>>>(out var factionSkillDict);

            Utility.Debug.LogInfo("yzqData>>>>>>>加入宗门的请求收到了" + Utility.Json.ToJson(factionSkillDict));
            var todayTNum = factionItemDict[schoolObj.SchoolID].ToDictionary(key => key.FactionItem_ID, value => value.FactionItem_TodayNum);
            var todaySNum = factionSkillDict[schoolObj.SchoolID].ToDictionary(key => key.FactionItem_ID, value => value.FactionItem_TodayNum);

            var schoolTemp = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaSchool);
            var treasureatticTemp = NHibernateQuerier.CriteriaSelect<Treasureattic>(nHCriteriaTreasureattic);
            var sutrasAtticTemp = NHibernateQuerier.CriteriaSelect<SutrasAttic>(nHCriteriasutrasAttic);
            Dictionary<string, string> DTOdict = new Dictionary<string, string>();

            if (schoolTemp != null)
            {

                Utility.Debug.LogInfo("yzqData>>>>>>>加入宗门的请求收到了" + Utility.Json.ToJson(todayTNum));
                if (treasureatticTemp != null)
                {
                    treasureatticTemp.ID = treasureatticObj.ID;
                    treasureatticTemp.ItemAmountDict = Utility.Json.ToJson(todayTNum);
                    NHibernateQuerier.Update(treasureatticTemp);
                    #region 测试完成替换
                    //int h = 23 - DateTime.Now.Hour;
                    //int m = 59 - DateTime.Now.Minute;
                    //int s = 60 - DateTime.Now.Second;
                    //await RedisHelper.String.StringSetAsync(redisKey, AllianceAlchemyJson, new TimeSpan(0, h, m, s));
                    #endregion
                    #region redis部分
                    RedisHelper.Hash.HashSet<Dictionary<int, int>>("TreasureatticDTO", treasureatticTemp.ID.ToString(), treasureatticObj.ItemNotRefreshDict);
                    #endregion
                }
                if (sutrasAtticTemp != null)
                    {
                    sutrasAtticTemp.ID = sutrasAtticObj.ID;
                    sutrasAtticTemp.SutrasAmountDict = Utility.Json.ToJson(todaySNum);
                        NHibernateQuerier.Update(sutrasAtticTemp);
                    RedisHelper.Hash.HashSet<SutrasAttic>("SutrasAtticDTO", sutrasAtticTemp.ID.ToString(), sutrasAtticTemp);
                }
                    schoolTemp.SchoolID = schoolObj.SchoolID;
                schoolTemp.SchoolJob = schoolObj.SchoolJob;
                NHibernateQuerier.Update(schoolTemp);
                RedisHelper.Hash.HashSet<School>("SchoolDTO", schoolTemp.ID.ToString(), schoolTemp);
                var schoolSendObj = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaSchool);
                SetResponseParamters(() =>
                {
                    Utility.Debug.LogInfo(">>>>>>>返回加入宗门的数据" + Utility.Json.ToJson(schoolSendObj));
                    subResponseParameters.Add((byte)ParameterCode.School, Utility.Json.ToJson(schoolSendObj));
                    subResponseParameters.Add((byte)ParameterCode.TreasureAttic, Utility.Json.ToJson(factionItemDict[schoolObj.SchoolID]));
                    subResponseParameters.Add((byte)ParameterCode.SutrasAtticm, Utility.Json.ToJson(factionSkillDict[schoolObj.SchoolID]));
                    operationResponse.ReturnCode = (byte)ReturnCode.Success;
                });
            }
            else
                SetResponseParamters(() =>{operationResponse.ReturnCode = (byte)ReturnCode.Fail; });
            Utility.Debug.LogInfo(">>>>>>>加入宗门的请求收到了2" + schoolTemp.SchoolID);
            ReferencePool.Release(nHCriteriaSchool, nHCriteriaTreasureattic, nHCriteriasutrasAttic);
            return operationResponse;
        }
    }
}


