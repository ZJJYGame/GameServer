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
using StackExchange.Redis;

namespace AscensionServer
{
    public class AddImmortalsAllianceHandler : SyncImmortalsAllianceSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string alliancestatusJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleAlliance));
            var alliancestatusObj = Utility.Json.ToObject<AllianceStatus>
                (alliancestatusJson);
            string roleAllianceJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.Alliances));
            var roleAllianceObj = Utility.Json.ToObject<RoleAlliance> 
                (roleAllianceJson);
          
            NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", alliancestatusObj.AllianceName);
            var alliance = NHibernateQuerier.CriteriaSelectAsync<AllianceStatus>(nHCriteriaAllianceName).Result;

            NHCriteria nHCriteriaAllianceMaster =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceMaster", alliancestatusObj.AllianceMaster);
            var allianceMasterObj = NHibernateQuerier.CriteriaSelectAsync<AllianceStatus>(nHCriteriaAllianceMaster).Result;

            List<string> Alliancelist = new List<string>();
            NHCriteria nHCriteriaroleAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleAllianceObj.RoleID);
            var roleAllianceTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriaroleAlliance).Result;

            var roleAssetsTemp = NHibernateQuerier.CriteriaSelectAsync<RoleAssets>(nHCriteriaroleAlliance).Result;

            #region MySql数据模块
            if (alliance == null&& allianceMasterObj==null&& roleAssetsTemp.SpiritStonesLow>=100000)
            {
                List<int> gangslist = new List<int>();
                NHCriteria nHCriteriaAllianceList =ReferencePool.Accquire<NHCriteria>().SetValue("ID", 1);
                var allianceTemp = NHibernateQuerier.CriteriaSelectAsync<Alliances>(nHCriteriaAllianceList).Result;
                gangslist = Utility.Json.ToObject<List<int>>(allianceTemp.AllianceList);

                alliancestatusObj.OnLineNum += 1;
                var allianceslIstObj =NHibernateQuerier.Insert(alliancestatusObj);

                AllianceConstruction allianceConstruction = new AllianceConstruction() { AllianceID = allianceslIstObj.ID};
               NHibernateQuerier.Insert(allianceConstruction);
                AllianceMember allianceMember = new AllianceMember() { AllianceID = allianceslIstObj.ID, ApplyforMember = Utility.Json.ToJson(new List<int>() { }) ,Member = Utility.Json.ToJson(new List<int>() { roleAllianceObj .RoleID}) };
              NHibernateQuerier.Insert(allianceMember);
                gangslist.Add(allianceslIstObj.ID);
                allianceTemp.AllianceList = Utility.Json.ToJson(gangslist);
                if (NHibernateQuerier.CriteriaSelectAsync<RoleAllianceSkill>(nHCriteriaroleAlliance).Result==null)
                {
                    RoleAllianceSkill roleAllianceSkill = new RoleAllianceSkill() { RoleID = roleAllianceObj.RoleID };

                    NHibernateQuerier.Insert(roleAllianceSkill);
                }
                NHibernateQuerier.Update(allianceTemp);

                Alliancelist.Add(Utility.Json.ToJson(allianceslIstObj));
                if (roleAllianceTemp != null)
                {
                    roleAllianceTemp.RoleID = roleAllianceObj.RoleID;
                    roleAllianceTemp.AllianceID = allianceslIstObj.ID;
                    roleAllianceTemp.AllianceJob = roleAllianceObj.AllianceJob;
                    roleAllianceTemp.Reputation = roleAllianceObj.Reputation;
                  NHibernateQuerier.Update(roleAllianceTemp);
                }
                else
                {
                    roleAllianceTemp = new RoleAlliance()
                    {
                        RoleID = roleAllianceObj.RoleID,
                        AllianceID = allianceslIstObj.ID,
                        AllianceJob = roleAllianceObj.AllianceJob,
                        Reputation = roleAllianceObj.Reputation,
                    };
                    NHibernateQuerier.Insert(roleAllianceTemp);
                }
                RoleAllianceDTO roleAllianceDTO = new RoleAllianceDTO() { AllianceID = roleAllianceTemp.AllianceID, AllianceJob = roleAllianceTemp.AllianceJob, JoinTime = roleAllianceTemp.JoinTime, ApplyForAlliance = Utility.Json.ToObject<List<int>>(roleAllianceTemp.ApplyForAlliance), Offline = roleAllianceTemp.Offline, Reputation = roleAllianceTemp.Reputation, ReputationHistroy = roleAllianceTemp.ReputationHistroy, ReputationMonth = roleAllianceTemp.ReputationMonth, RoleID = roleAllianceTemp.RoleID, RoleName = roleAllianceTemp.RoleName };
                Alliancelist.Add(Utility.Json.ToJson(roleAllianceDTO));
                Alliancelist.Add(Utility.Json.ToJson(allianceConstruction));
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.Alliances, Utility.Json.ToJson(Alliancelist));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });

                #region  Redis模块
                RedisHelper.String.StringSet("Alliance", allianceTemp.AllianceList);
                RedisHelper.Hash.HashSet("AllianceConstruction", allianceConstruction.AllianceID.ToString(), allianceConstruction);
                RedisHelper.Hash.HashSet("AllianceMember", allianceMember.AllianceID.ToString(), allianceMember);
                RedisHelper.Hash.HashSet("RoleAlliance", roleAllianceDTO.RoleID.ToString(), roleAllianceDTO);
                RedisHelper.Hash.HashSet("AllianceStatus", allianceslIstObj.ID.ToString(), allianceslIstObj);
                RedisHelper.Hash.HashSet<RoleAssets>("RoleAssets", roleAllianceTemp.RoleID.ToString(), new RoleAssets() { RoleID = roleAssetsTemp.RoleID, SpiritStonesLow = roleAssetsTemp.SpiritStonesLow, XianYu = roleAssetsTemp.XianYu });
                #endregion
                ReferencePool.Release(nHCriteriaAllianceList, nHCriteriaAllianceName);
            }
            else
            {
                SetResponseParamters(() =>
                {
                    operationResponse.ReturnCode = (short)ReturnCode.Fail;
                });
            }
            #endregion
            return operationResponse;
        }
    }
}


