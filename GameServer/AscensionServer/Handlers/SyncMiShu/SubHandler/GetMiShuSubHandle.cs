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
using RedisDotNet;
namespace AscensionServer
{
    public class GetMiShuSubHandle : SyncMiShuSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;
        List<MiShuDTO> miShuIdList = new List<MiShuDTO>();
        List<NHCriteria> nHCriteriaslist = new List<NHCriteria>();
        List<MiShuDTO> mishulist = new List<MiShuDTO>();
        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleMSJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.MiShu));
            var roleMiShuObj = Utility.Json.ToObject<RoleMiShuDTO>(roleMSJson);
            NHCriteria nHCriteriamishu =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleMiShuObj.RoleID);
            RoleMiShu roleMiShu = NHibernateQuerier.CriteriaSelect<RoleMiShu>(nHCriteriamishu);
            Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>收到获取秘术的数组" + roleMiShu.MiShuIDDict);
            miShuIdList.Clear();
            nHCriteriaslist.Clear();
            mishulist.Clear();
            if (RedisHelper.KeyExistsAsync(RedisKeyDefine._RoleMiShuPerfix+ roleMiShuObj.RoleID).Result)
            {
                var rolemishuRedisObj = RedisHelper.Hash.HashGetAsync<RoleMiShuDTO>(RedisKeyDefine._RoleMiShuPerfix + roleMiShuObj.RoleID, roleMiShuObj.RoleID.ToString()).Result;
                foreach (var item in rolemishuRedisObj.MiShuIDDict)
                {
                    MiShuDTO miShuObj = RedisHelper.Hash.HashGetAsync<MiShuDTO>(RedisKeyDefine._MiShuPerfix + roleMiShuObj.RoleID, roleMiShuObj.RoleID.ToString()).Result;
                    miShuIdList.Add(miShuObj);
                }
                OperationData operationData = new OperationData();
                operationData.DataMessage = Utility.Json.ToJson(miShuIdList);
                operationData.OperationCode = (byte)OperationCode.SyncMiShu;
                GameEntry. RoleManager.SendMessage(roleMiShuObj.RoleID, operationData);
            }
            else
            {
                #region MySQL数据
                if (!string.IsNullOrEmpty(roleMiShu.MiShuIDDict))
                {
                    string rolemishuJson = roleMiShu.MiShuIDDict;
                    if (!string.IsNullOrEmpty(rolemishuJson))
                    {
                      var roleIDict = Utility.Json.ToObject<Dictionary<int, int>>(rolemishuJson);
                        foreach (var roleid in roleIDict)
                        {
                            NHCriteria tmpcriteria =ReferencePool.Accquire<NHCriteria>().SetValue("ID", roleid.Key);
                            MiShu miShu = NHibernateQuerier.CriteriaSelect<MiShu>(tmpcriteria);
                            var mishuMySQL =ReferencePool.Accquire<MiShuDTO>();
                            mishuMySQL.MiShuID = miShu.MiShuID;
                            mishuMySQL.MiShuAdventureSkill =Utility.Json.ToObject<List<int>>(miShu.MiShuAdventtureSkill);
                            mishuMySQL.MiShuSkillArry = Utility.Json.ToObject<List<int>>(miShu.MiShuSkillArry);
                            mishuMySQL.MiShuExp = miShu.MiShuExp;
                            mishuMySQL.MiShuLevel = miShu.MiShuLevel;
                            miShuIdList.Add(mishuMySQL);
                            nHCriteriaslist.Add(tmpcriteria);
                            mishulist.Add(mishuMySQL);
                            //Utility.Debug.LogInfo("yzqData角色秘书数据" + rolemishuJson);
                        }
                    }
                    //Utility.Debug.LogInfo("yzqData角色所有秘术数据" + Utility.Json.ToJson(miShuIdList));
                    OperationData operationData = new OperationData();
                    operationData.DataMessage = Utility.Json.ToJson(miShuIdList);
                    operationData.OperationCode = (byte)OperationCode.SyncMiShu;
                    GameEntry. RoleManager.SendMessage(roleMiShuObj.RoleID, operationData);
                    ReferencePool.Release(nHCriteriaslist.ToArray());
                    ReferencePool.Release(mishulist.ToArray());
                }
                else
                {
                    SetResponseParamters(() =>
                    {
                        Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>收到获取秘术的id为空");
                        subResponseParameters.Add((byte)ParameterCode.MiShu, Utility.Json.ToJson(new List<string>()));
                        operationResponse.ReturnCode = (byte)ReturnCode.Fail;
                    });
                }
                #endregion
            }
            ReferencePool.Release(nHCriteriamishu);
            return operationResponse;
        }
    }
}

