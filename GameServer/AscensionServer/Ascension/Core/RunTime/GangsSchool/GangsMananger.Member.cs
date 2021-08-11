using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisDotNet;
using AscensionProtocol;
using AscensionProtocol.DTO;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
using NHibernate.Criterion;
namespace AscensionServer
{
    public partial class GangsMananger
    {
        #region  Redis模塊
        /// <summary>
        /// 申请加入宗门
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
       async void ApplyJoinAllianceS2C(int roleID, int id)
        {
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            var alianceExists = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
            if (result && alianceExists)
            {
                var roleAlliance = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                var alliance = RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                if (roleAlliance != null && alliance != null)
                {
                    if (!roleAlliance.ApplyForAlliance.Contains(id) && !alliance.ApplyforMember.Contains(roleID) && !alliance.Member.Contains(roleID))
                    {
                        roleAlliance.ApplyForAlliance.Add(id);
                        alliance.ApplyforMember.Add(roleID);

                        await NHibernateQuerier.UpdateAsync(ChangeDataType(roleAlliance));
                        await NHibernateQuerier.UpdateAsync(ChangeDataType(alliance));

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), roleAlliance);
                        Utility.Debug.LogInfo("申请宗門" + Utility.Json.ToJson(alliance));
                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), alliance);
                        
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.JoinAlliance, roleAlliance);
                        //TODO更新到数据库
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, AllianceOpCode.JoinAlliance);
                    }
                }
                else
                    ApplyJoinAllianceMySql(roleID, id);
            }
            else
                ApplyJoinAllianceMySql(roleID, id);
        }
        /// <summary>
        /// 一鍵同意入宗申請
        /// </summary>
        async void ConsentApplyS2C(int roleID, int id, List<int> roleIDs)
        {
            Utility.Debug.LogError("收到的同意的角色id："+Utility.Json.ToJson(roleIDs));
            var allianceExists = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
            var alliancestatusExists = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
            var onofflineExists = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            List<int> consents = new List<int>();
            List<RoleAllianceDTO> member = new List<RoleAllianceDTO>();
            List<RoleAllianceDTO> apply = new List<RoleAllianceDTO>();
            if (allianceExists && alliancestatusExists&& onofflineExists)
            {
                var allianceObj = RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                var alliancestatusObj = RedisHelper.Hash.HashGetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
                if (allianceObj != null && alliancestatusObj != null)
                {
                    for (int i = 0; i < roleIDs.Count; i++)
                    {
                        if (allianceObj.ApplyforMember.Contains(roleIDs[i]) && (alliancestatusObj.AllianceNumberPeople + consents.Count) <= alliancestatusObj.AlliancePeopleMax)
                        {
                            var rolealliance = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString()).Result;
                            var onofflineObj = RedisHelper.Hash.HashGetAsync<OnOffLineDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString()).Result;

                            if (rolealliance != null && onofflineObj != null)
                            {
                                if (rolealliance.AllianceID == 0)
                                {
                                    if (onofflineObj.OffTime == "在线")
                                    {
                                        alliancestatusObj.OnLineNum++;
                                    }
                                    rolealliance.AllianceID = id;
                                    rolealliance.AllianceJob = 931;
                                    rolealliance.ApplyForAlliance.Clear();
                                    rolealliance.JoinTime = DateTime.Now.ToString();
                                    consents.Add(roleIDs[i]);
                                    allianceObj.ApplyforMember.Remove(roleIDs[i]);
                                    allianceObj.Member.Add(roleIDs[i]);
                                    if (allianceObj.JobNumDict.ContainsKey(rolealliance.AllianceJob))
                                    {
                                        allianceObj.JobNumDict[rolealliance.AllianceJob]++;
                                    }
                                    else
                                    {
                                        allianceObj.JobNumDict.Add(rolealliance.AllianceJob, 1);
                                    }
                                    RoleStatusSuccessS2C(roleIDs[i], AllianceOpCode.JoinAllianceSuccess, rolealliance);

                                    await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString(), rolealliance);
                                    await NHibernateQuerier.UpdateAsync(ChangeDataType(rolealliance));
                                }
                                else
                                {
                                    allianceObj.ApplyforMember.Remove(roleIDs[i]);
                                }
                            }
                        }
                    }
                    allianceObj.ApplyforMember.Except(consents);
                    Utility.Debug.LogError("当前成员角色id：" + Utility.Json.ToJson(allianceObj.Member));
                    Utility.Debug.LogError("当前需要加入的成员角色id：" + Utility.Json.ToJson(consents));
                    //allianceObj.Member.AddRange(consents);
                    alliancestatusObj.AllianceNumberPeople += consents.Count;

                    //TODO發送數據至客戶端
                    for (int i = 0; i < allianceObj.ApplyforMember.Count; i++)
                    {
                        if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, allianceObj.ApplyforMember[i].ToString()).Result)
                        {
                            var roleAlliance = await RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, allianceObj.ApplyforMember[i].ToString());
                            if (roleAlliance != null)
                            {
                                apply.Add(roleAlliance);
                            }
                        }

                    }
                    for (int i = 0; i < allianceObj.Member.Count; i++)
                    {
                        if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, allianceObj.Member[i].ToString()).Result)
                        {
                            var roleAlliance = await RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, allianceObj.Member[i].ToString());
                            if (roleAlliance != null)
                            {
                                member.Add(roleAlliance);
                                #region 修改通告
                                var daily = DailyMsg(roleAlliance.RoleName, (byte)DailyMsgType.JobChange);
                                AddDailyMsg(roleAlliance.RoleID, daily);
                                #endregion
                            }
                        }
                    }
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.AllianceMember, member);
                    dict.Add((byte)ParameterCode.ApplyMember, apply);
                    dict.Add((byte)ParameterCode.MemberJobNum, allianceObj);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.ConsentApply, dict);

                    //更新到数据库
                    await RedisHelper.Hash.HashSetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), allianceObj);
                    await RedisHelper.Hash.HashSetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, id.ToString(), alliancestatusObj);
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(allianceObj));
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(alliancestatusObj));
                }
                else
                    ConsentApplyMySql(roleID, id, roleIDs);
            }
            else
                ConsentApplyMySql(roleID, id, roleIDs);

        }
        /// <summary>
        /// 一鍵拒絕
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        /// <param name="roleIDs"></param>
        async void RefuseApplyS2C(int roleID, int id, List<int> roleIDs)
        {
            var allianceExists = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
            var alliancestatusExists = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
            List<RoleAllianceDTO> refuses = new List<RoleAllianceDTO>();
            List<int> refuseList = new List<int>();
            if (allianceExists && alliancestatusExists)
            {
                var allianceObj = RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                var alliancestatusObj = RedisHelper.Hash.HashGetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
                if (allianceObj != null && alliancestatusObj != null)
                {
                    for (int i = 0; i < roleIDs.Count; i++)
                    {
                        if (allianceObj.ApplyforMember.Contains(roleIDs[i]))
                        {
                            var rolealliance = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString()).Result;
                            if (rolealliance != null)
                            {
                                if (rolealliance.ApplyForAlliance.Contains(id))
                                {
                                    rolealliance.ApplyForAlliance.Remove(id);
                                    refuseList.Add(roleIDs[i]);
                                }
                                await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString(), rolealliance);
                                await NHibernateQuerier.UpdateAsync(ChangeDataType(rolealliance));
                            }
                        }
                    }
                    Utility.Debug.LogInfo("YZQ删除过后的申请列表"+ Utility.Json.ToJson(allianceObj.ApplyforMember)+ "申请后" + Utility.Json.ToJson(refuseList) );
                    var apply = allianceObj.ApplyforMember.Except(refuseList);
                    Utility.Debug.LogInfo("YZQ删除过后的申请列表" + Utility.Json.ToJson(apply) );
                    allianceObj.ApplyforMember = apply.ToList();
                    for (int i = 0; i < allianceObj.ApplyforMember.Count; i++)
                    {
                        if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, allianceObj.ApplyforMember[i].ToString()).Result)
                        {
                            var roleAlliance = await RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, allianceObj.ApplyforMember[i].ToString());
                            if (roleAlliance != null)
                            {
                                refuses.Add(roleAlliance);
                            }
                        }
                    }
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.RefuseApply, refuses);

                   await RedisHelper.Hash.HashSetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix,id.ToString(), allianceObj);
                   await NHibernateQuerier.UpdateAsync(ChangeDataType(allianceObj));
                }
                else
                {
                    RefuseApplyMySql(roleID,id, roleIDs);
                }
            }
            else
            {
                RefuseApplyMySql(roleID, id, roleIDs);
            }
        }
        /// <summary>
        /// 獲取宗門成員數據
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        async void GetAllianceMemberS2C(int roleID, int id)
        {
            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
            if (allianceExist)
            {
                Utility.Debug.LogInfo("YZQ请求宗门成员1");
                var allianceObj = RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                List<RoleAllianceDTO> member = new List<RoleAllianceDTO>();
                List<RoleAllianceDTO> apply = new List<RoleAllianceDTO>();
                if (allianceObj != null)
                {
                    for (int i = 0; i < allianceObj.ApplyforMember.Count; i++)
                    {
                        if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, allianceObj.ApplyforMember[i].ToString()).Result)
                        {
                            var roleAlliance = await RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, allianceObj.ApplyforMember[i].ToString());
                            if (roleAlliance != null)
                            {
                                apply.Add(roleAlliance);
                            }
                        }

                    }
                    for (int i = 0; i < allianceObj.Member.Count; i++)
                    {
                        if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, allianceObj.Member[i].ToString()).Result)
                        {
                            var roleAlliance = await RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, allianceObj.Member[i].ToString());
                            if (roleAlliance != null)
                            {
                                member.Add(roleAlliance);
                            }
                        }
                    }
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.AllianceMember, member);
                    dict.Add((byte)ParameterCode.ApplyMember, apply);
                    dict.Add((byte)ParameterCode.MemberJobNum, allianceObj);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceMember, dict);
                }
                else
                    GetAllianceMemberMySql(roleID, id);
            }
            else
                GetAllianceMemberMySql(roleID, id);
        }
        /// <summary>
        /// 模糊搜索宗门信息
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        async void SearchAllianceS2C(int roleID,int id,string name)
        {
            List<NHCriteria> nHCriterias = new List<NHCriteria>();
            if (id <= 0)
            {
                NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", name);
                nHCriterias.Add(nHCriteriaAllianceName);
                var allianceNameObj = NHibernateQuerier.CriteriaLikeAsync<AllianceStatus>(nHCriteriaAllianceName, MatchMode.Anywhere).Result;
                if (allianceNameObj.Count == 0)
                {
                    RoleStatusFailS2C(roleID, AllianceOpCode.SearchAlliance);
                }
                else
                {
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.SearchAlliance, allianceNameObj);
                }
            }
            else
            {
                NHCriteria nHCriteriaAllianceID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", id);
                nHCriterias.Add(nHCriteriaAllianceID);
                var allianceIDObj = NHibernateQuerier.CriteriaSelectAsync<AllianceStatus>(nHCriteriaAllianceID).Result;
                List<AllianceStatus> allianceStatuss = new List<AllianceStatus>();
                if (allianceIDObj == null)
                {
                    NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", name.ToString());
                    nHCriterias.Add(nHCriteriaAllianceName);
                    var allianceNameObj = NHibernateQuerier.CriteriaLikeAsync<AllianceStatus>(nHCriteriaAllianceName, MatchMode.Anywhere).Result;
                    if (allianceNameObj.Count == 0)
                    {
                        RoleStatusFailS2C(roleID, AllianceOpCode.SearchAlliance);
                    }
                    else
                    {
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.SearchAlliance, allianceNameObj);
                    }
                }
                else
                {
                    allianceStatuss.Add(allianceIDObj);
                    NHCriteria nHCriteriaAllianceName =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceName", name);
                    nHCriterias.Add(nHCriteriaAllianceName);
                    var allianceNameObj = NHibernateQuerier.CriteriaLikeAsync<AllianceStatus>(nHCriteriaAllianceName, MatchMode.Anywhere).Result;
                    if (allianceNameObj.Count == 0)
                    {
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.SearchAlliance, allianceStatuss);
                    }
                    else
                    {
                        if (!allianceNameObj.Equals(allianceIDObj))
                        {
                            allianceNameObj.Add(allianceIDObj);
                            RoleStatusSuccessS2C(roleID, AllianceOpCode.SearchAlliance, allianceNameObj);
                        }                       
                    }
                    
                }
            }
        }
        /// <summary>
        /// 修改宗门职位
        /// </summary>
        async void CareerAdvancementS2C(int roleID,int playerid,int id,int jobID)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, AllianceJobData>>(out var jobDict);
            var result = jobDict.TryGetValue(jobID, out var jobData);

            if (!result)
            {
                RoleStatusFailS2C(roleID, AllianceOpCode.CareerAdvancement);
                return;
            }
            List<RoleAllianceDTO> roleallianceList = new List<RoleAllianceDTO>();
            Dictionary<byte, object> dict = new Dictionary<byte, object>();

            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            var playerExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, playerid.ToString()).Result;
            if (allianceExist&& allianceExist&& playerExist)
            {
                var Alliance= RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                var player = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, playerid.ToString()).Result;

                if (Alliance != null&& role!=null&& player!=null)
                {
                    if (role.AllianceJob > player.AllianceJob && role.AllianceJob == 937 && jobID == 937)
                    {
                        role.AllianceJob = 931;
                        if (Alliance.JobNumDict[player.AllianceJob] > 0)
                        {
                            Alliance.JobNumDict[player.AllianceJob]--;
                        }
                        player.AllianceJob = 937;
                        roleallianceList.Add(role);
                        roleallianceList.Add(player);

                        dict.Add((byte)ParameterCode.RoleAlliance, roleallianceList);
                        dict.Add((byte)ParameterCode.MemberJobNum, Alliance);
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.CareerAdvancement, dict);
                        return;
                    }
                    if (role.AllianceJob > player.AllianceJob && jobID< role.AllianceJob)
                    {
                        var exist = Alliance.JobNumDict.TryGetValue(jobID, out int num);
                        if (exist)
                        {
                            if (jobData.JobStations > num)
                            {
                                if (Alliance.JobNumDict[player.AllianceJob]>0)
                                {
                                    Alliance.JobNumDict[player.AllianceJob]--;
                                }
                                Alliance.JobNumDict[jobID]++;
                                player.AllianceJob = jobID;
                                roleallianceList.Add(player);
                                #region 修改通告
                                var daily = DailyMsg(player.RoleName, (byte)DailyMsgType.JobChange);
                                AddDailyMsg(player.RoleID, daily);
                                #endregion

                                dict.Add((byte)ParameterCode.RoleAlliance, roleallianceList);
                                dict.Add((byte)ParameterCode.MemberJobNum, Alliance);
                                RoleStatusSuccessS2C(roleID, AllianceOpCode.CareerAdvancement, dict);
                            }
                            else
                            {
                                RoleStatusFailS2C(roleID,AllianceOpCode.CareerAdvancement);
                            }
                        }
                        else
                        {
                            RoleStatusFailS2C(roleID, AllianceOpCode.CareerAdvancement);
                        }
                    }

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), role);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAlliancePerfix, playerid.ToString(), player);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceMemberPerfix, playerid.ToString(), Alliance);

                    await NHibernateQuerier.UpdateAsync(ChangeDataType(role));
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(player));
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(Alliance));
                }
                
            }else
                RoleStatusFailS2C(roleID, AllianceOpCode.CareerAdvancement);
        }
        /// <summary>
        /// 退出宗门
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        async void QuitAllianceS2C(int roleID,int id)
        {
            var result = QuitAllianceRedis(roleID, id).Result;
            if (result != null)
            {
                #region 修改通告
                var daily = DailyMsg(result.RoleName, (byte)DailyMsgType.LeaveAlliance);
                AddDailyMsg(result.RoleID, daily);
                #endregion

                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetRoleAlliance, result);

            }
            else
                QuitAllianceMySql(roleID, id);
        }
        /// <summary>
        /// 踢出宗门
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        async void KickOutAllianceS2C(int roleID, int playerid,int id)
        {
            var roleexist= RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            if (roleexist)
            {
                var roleallianceObj = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                if (roleallianceObj!=null)
                {
                    if (roleallianceObj.AllianceJob==937)
                    {
                        var result = QuitAlliance(playerid, id).Result;
                        if (result != null)
                        {
                            #region 修改通告
                            var daily = DailyMsg(result.RoleName, (byte)DailyMsgType.LeaveAlliance);
                            AddDailyMsg(result.RoleID, daily);
                            #endregion

                            RoleStatusSuccessS2C(roleID, AllianceOpCode.KickOutAlliance, ChangeDataType(result));
                            RoleStatusSuccessS2C(playerid, AllianceOpCode.GetRoleAlliance, ChangeDataType(result));
                        }
                        else
                            KickOutAllianceMySql(roleID, playerid, id);
                    }
                    else
                        RoleStatusFailS2C(roleID, AllianceOpCode.KickOutAlliance);
                }
                else
                    KickOutAllianceMySql(roleID, playerid, id);
            }



        }
        /// <summary>
        /// 退出宗门
        /// </summary>
        async void DissolveAllianceS2C(int roleid,int allianceid,string name)
        {
            DissolveAllianceDTO dissolve = new DissolveAllianceDTO();
            //DailyMessageDTO dailyMessageDTO = new DailyMessageDTO();
            //dailyMessageDTO.Name = name;
            //dailyMessageDTO.Describe = "宗门";
            //dailyMessageDTO.Name = name;
            //dailyMessageDTO.Name = name;

            await RedisHelper.String.StringSetAsync(RedisKeyDefine._DissolveAlliancePerfix+roleid,Utility.Json.ToJson(dissolve));
        }
        #endregion

        #region Redis仙盟解散事件监听
        void RefreshDissolve(string key)
        { RedisManager.Instance.AddKeyExpireListener(key, DeleteSignin); }

        async void DeleteDissolveAlliance(string key)
        {
            //TODO具体解散的逻辑，假设key包含仙盟id，先从仙盟列表删除，再删除每人的仙盟然后慢慢清除仙盟数据
            int id = 0;
            var allianceMemberExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
            if (allianceMemberExist)
            {
                var alliance = RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                for (int i = 0; i < alliance.Member.Count; i++)
                {
                    var roleAllianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, alliance.Member[i].ToString()).Result;
                    if (roleAllianceExist)
                    {
                        var roleAlliance = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, alliance.Member[i].ToString()).Result;
                        if (roleAlliance!=null)
                        {
                            roleAlliance.AllianceID = 0;


                            RoleStatusSuccessS2C(roleAlliance.RoleID, AllianceOpCode.DissolveAlliance, roleAlliance);
                        }
                    }
                }
            }
        }
        #endregion

        #region MySql模塊
        /// <summary>
        /// 申請加入宗門
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        async void ApplyJoinAllianceMySql(int roleID, int id)
        {
            Utility.Debug.LogInfo("角色宗门加入申请");
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var roleAlliance = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaRole);
            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", id);
            var alliance = NHibernateQuerier.CriteriaSelect<AllianceMember>(nHCriteriaAlliance);
            if (alliance != null && roleAlliance != null)
            {
                var roleAllianceList = Utility.Json.ToObject<List<int>>(roleAlliance.ApplyForAlliance);//角色申請的宗廟
                var applyMemberList = Utility.Json.ToObject<List<int>>(alliance.ApplyforMember);//宗門申請列表
                var memberList = Utility.Json.ToObject<List<int>>(alliance.Member);
                if (!roleAllianceList.Contains(id) && !applyMemberList.Contains(roleID) && !memberList.Contains(roleID))
                {
                    roleAllianceList.Add(id);
                    applyMemberList.Add(roleID);
                    roleAlliance.ApplyForAlliance = Utility.Json.ToJson(roleAllianceList);
                    alliance.ApplyforMember = Utility.Json.ToJson(applyMemberList);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.JoinAlliance, ChangeDataType(roleAlliance));
                    //TODO更新到数据库
                    await NHibernateQuerier.UpdateAsync(roleAlliance);
                    await NHibernateQuerier.UpdateAsync(alliance);

                    await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), ChangeDataType(roleAlliance));

                    await RedisHelper.Hash.HashSetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), ChangeDataType(alliance));

                }
                else
                {
                    RoleStatusFailS2C(roleID, AllianceOpCode.JoinAlliance);
                }
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.JoinAlliance);
        }
        /// <summary>
        /// 一鍵同意申請加入宗廟
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roleIDs"></param>
        async void ConsentApplyMySql(int roleid, int id, List<int> roleIDs)
        {
            List<int> consents = new List<int>();
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("ID", id);
            NHCriteria nHCriteriamember =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", id);
            var allianceObj = NHibernateQuerier.CriteriaSelect<AllianceMember>(nHCriteriamember);
            var alliance = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteria);
            List<RoleAllianceDTO> memberlist = new List<RoleAllianceDTO>();
            List<RoleAllianceDTO> applylist = new List<RoleAllianceDTO>();
            if (allianceObj != null && alliance != null)
            {
                var applyeList = Utility.Json.ToObject<List<int>>(allianceObj.ApplyforMember);
                var memberList = Utility.Json.ToObject<List<int>>(allianceObj.Member);
                var jobDict = Utility.Json.ToObject<Dictionary<int,int>>(allianceObj.JobNumDict);

                for (int i = 0; i < roleIDs.Count; i++)
                {
                    if (applyeList.Contains(roleIDs[i]) && (alliance.AllianceNumberPeople + consents.Count) <= alliance.AlliancePeopleMax)
                    {
                        NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleIDs[i]);
                        var roleAlliancej = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaRole);
                        var onofflineObj = NHibernateQuerier.CriteriaSelect<OnOffLine>(nHCriteriaRole);

                        if (roleAlliancej != null)
                        {
                            if (roleAlliancej.AllianceID==0)
                            {
                                if (onofflineObj.OffTime.Equals("在线"))
                                {
                                    alliance.OnLineNum++;
                                }
                                roleAlliancej.AllianceID = id;
                                roleAlliancej.AllianceJob = 931;
                                roleAlliancej.ApplyForAlliance = "[]";
                                roleAlliancej.JoinTime = DateTime.Now.ToString();
                                consents.Add(roleIDs[i]);
                                if (jobDict.ContainsKey(roleAlliancej.AllianceJob))
                                {
                                    jobDict[roleAlliancej.AllianceJob]++;
                                }
                                else
                                    jobDict.Add(roleAlliancej.AllianceJob, 1);

                                RoleStatusSuccessS2C(roleIDs[i], AllianceOpCode.JoinAllianceSuccess, roleAlliancej);

                                await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString(), ChangeDataType(roleAlliancej));
                                await NHibernateQuerier.UpdateAsync(roleAlliancej);
                            }
                        }
                    }
                    else
                        RoleStatusFailS2C(roleid, AllianceOpCode.ConsentApply);
                }
               var  applyeMember = applyeList.Except(consents);
                allianceObj.ApplyforMember = Utility.Json.ToJson(applyeMember);
                //memberList.AddRange(consents);
                allianceObj.JobNumDict = Utility.Json.ToJson(jobDict);
                allianceObj.Member = Utility.Json.ToJson(memberList);
                alliance.AllianceNumberPeople += consents.Count;

                Utility.Debug.LogInfo("YZQ"+Utility.Json.ToJson(allianceObj.ApplyforMember)+">>>><<<<"+ Utility.Json.ToJson(memberList));

                for (int i = 0; i < applyeMember.ToList().Count; i++)
                {
                    NHCriteria nHCriteriaalliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", applyeList[i]);
                    var allianceTemp = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaalliance);
                    applylist.Add(ChangeDataType(allianceTemp));

                }
                for (int i = 0; i < memberList.Count; i++)
                {
                    NHCriteria nHCriteriaalliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", memberList[i]);
                    var allianceTemp = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaalliance);
                    memberlist.Add(ChangeDataType(allianceTemp));

                    #region 修改通告
                    var daily = DailyMsg(allianceTemp.RoleName, (byte)DailyMsgType.JobChange);
                    AddDailyMsg(allianceTemp.RoleID, daily);
                    #endregion
                }

                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.AllianceMember, memberlist);
                dict.Add((byte)ParameterCode.ApplyMember, applylist);
                dict.Add((byte)ParameterCode.MemberJobNum, ChangeDataType(allianceObj));
                RoleStatusSuccessS2C(roleid, AllianceOpCode.ConsentApply, dict);

                await RedisHelper.Hash.HashSetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), ChangeDataType(allianceObj));
                await RedisHelper.Hash.HashSetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, id.ToString(), alliance);
                await NHibernateQuerier.UpdateAsync(allianceObj);
                await NHibernateQuerier.UpdateAsync(alliance);
            } else
                RoleStatusFailS2C(roleid, AllianceOpCode.ConsentApply);
        }
        /// <summary>
        /// 一鍵拒絕加入宗門
        /// </summary>
        async void RefuseApplyMySql(int roleid, int id, List<int> roleIDs)
        {
            List<RoleAllianceDTO> refuses = new List<RoleAllianceDTO>();
            List<int> refuseList = new List<int>();
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("ID", id);
            var allianceObj = NHibernateQuerier.CriteriaSelect<AllianceMember>(nHCriteria);
            var alliance = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteria);
            if (allianceObj != null && alliance != null)
            {
                var applyeList = Utility.Json.ToObject<List<int>>(allianceObj.ApplyforMember);
                for (int i = 0; i < roleIDs.Count; i++)
                {
                    if (applyeList.Contains(roleIDs[i]))
                    {
                        NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleIDs[i]);
                        var roleAlliancej = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaRole);
                        if (roleAlliancej != null)
                        {
                            var applys = Utility.Json.ToObject<List<int>>(roleAlliancej.ApplyForAlliance);
                            if (applys.Contains(id))
                            {
                                applys.Remove(id);
                                refuseList.Add(roleIDs[i]);
                            }
                            roleAlliancej.ApplyForAlliance = Utility.Json.ToJson(applys);
                            await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleIDs[i].ToString(), ChangeDataType(roleAlliancej));
                            await NHibernateQuerier.UpdateAsync(roleAlliancej);
                        }
                    }
                }
                var apply = applyeList.Except(refuseList);
                allianceObj.ApplyforMember = Utility.Json.ToJson(apply);
                for (int i = 0; i < apply.ToList().Count; i++)
                {
                    NHCriteria nHCriteriaalliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", applyeList[i]);
                    var allianceTemp = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaalliance);
                    refuses.Add(ChangeDataType(allianceTemp));
                }
                RoleStatusSuccessS2C(roleid, AllianceOpCode.RefuseApply, refuses);

                await RedisHelper.Hash.HashSetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), ChangeDataType(allianceObj));
                await NHibernateQuerier.UpdateAsync(allianceObj);
            } else
                RoleStatusFailS2C(roleid, AllianceOpCode.RefuseApply);
        }
        /// <summary>
        /// 獲取宗門成員數據
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        async void GetAllianceMemberMySql(int roleID, int id)
        {
            Utility.Debug.LogInfo("YZQ请求宗门成员2");
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", id);
            var allianceObj = NHibernateQuerier.CriteriaSelect<AllianceMember>(nHCriteria);
            List<RoleAllianceDTO> memberlist = new List<RoleAllianceDTO>();
            List<RoleAllianceDTO> applylist = new List<RoleAllianceDTO>();
            if (allianceObj != null)
            {
                var member = Utility.Json.ToObject<List<int>>(allianceObj.Member);
                var apply = Utility.Json.ToObject<List<int>>(allianceObj.ApplyforMember);
                for (int i = 0; i < apply.Count; i++)
                {
                    NHCriteria nHCriteriaalliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", apply[i]);
                    var alliance = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaalliance);
                    applylist.Add(ChangeDataType(alliance));

                }
                for (int i = 0; i < member.Count; i++)
                {
                    NHCriteria nHCriteriaalliance =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", member[i]);
                    var alliance = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaalliance);
                    memberlist.Add(ChangeDataType(alliance));
                }

                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.AllianceMember, memberlist);
                dict.Add((byte)ParameterCode.ApplyMember, applylist);
                dict.Add((byte)ParameterCode.MemberJobNum, ChangeDataType(allianceObj));
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAllianceMember, dict);
            }
            else
                RoleStatusFailS2C(roleID,AllianceOpCode.GetAllianceMember);
        }
        /// <summary>
        /// 退出宗门
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="id"></param>
        async void QuitAllianceMySql(int roleID, int id)
        {
            var result = QuitAlliance(roleID,id).Result;
            if (result != null)
            {
                #region 修改通告
                var daily = DailyMsg(result.RoleName, (byte)DailyMsgType.LeaveAlliance);
                AddDailyMsg(result.RoleID, daily);
                #endregion
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetRoleAlliance, ChangeDataType(result));
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.QuitAlliance);
        }
        /// <summary>
        /// 踢出宗門
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="playerid"></param>
        /// <param name="id"></param>
        async void KickOutAllianceMySql(int roleID,int playerid, int id)
        {
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var roleallianceObj = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriarole);

            if (roleallianceObj!=null)
            {
                if (roleallianceObj.AllianceJob==937)
                {
                    var result = QuitAlliance(playerid, id).Result;
                    if (result != null)
                    {
                        #region 修改通告
                        var daily = DailyMsg(result.RoleName, (byte)DailyMsgType.LeaveAlliance);
                        AddDailyMsg(result.RoleID, daily);
                        #endregion
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.KickOutAlliance, ChangeDataType(result));
                        RoleStatusSuccessS2C(playerid, AllianceOpCode.GetRoleAlliance, ChangeDataType(result));
                    }
                    else
                        RoleStatusFailS2C(roleID, AllianceOpCode.KickOutAlliance);
                }
                else
                    RoleStatusFailS2C(roleID, AllianceOpCode.KickOutAlliance);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.KickOutAlliance);
        }
        #endregion

        async Task<RoleAllianceDTO> QuitAllianceRedis(int roleID, int id)
        {
            var status = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
            var dongfu = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceDongFuPostfix, id.ToString()).Result;
            var member = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
            var rolealliance = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            if (status && dongfu && member && rolealliance)
            {
                var statusObj = RedisHelper.Hash.HashGetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, id.ToString()).Result;
                var dongfuObj = RedisHelper.Hash.HashGetAsync<AllianceDongFuDTO>(RedisKeyDefine._AllianceDongFuPostfix, id.ToString()).Result;
                var memberObj = RedisHelper.Hash.HashGetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, id.ToString()).Result;
                var roleallianceObj = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                if (statusObj != null   && memberObj != null && roleallianceObj != null)
                {
                    if (roleallianceObj.AllianceJob==937)
                    {
                        return null;
                    }

                    statusObj.AllianceNumberPeople -= 1;
                    statusObj.OnLineNum -= 1;
                    if (dongfuObj != null)
                    {
                        var dongfuone = dongfuObj.PreemptOne.Find(x => x.RoleID == roleID);
                        if (dongfuone != null)
                        {
                            dongfuObj.PreemptOne.Remove(dongfuone);
                        }
                        var dongfutow = dongfuObj.PreemptTow.Find(x => x.RoleID == roleID);
                        if (dongfutow != null)
                        {
                            dongfuObj.PreemptOne.Remove(dongfutow);
                        }
                        var dongfuthree = dongfuObj.PreemptThree.Find(x => x.RoleID == roleID);
                        if (dongfuthree != null)
                        {
                            dongfuObj.PreemptOne.Remove(dongfuthree);
                        }
                        var dongfufour = dongfuObj.PreemptFour.Find(x => x.RoleID == roleID);
                        if (dongfufour != null)
                        {
                            dongfuObj.PreemptOne.Remove(dongfufour);
                        }
                        var dongfufive = dongfuObj.PreemptFive.Find(x => x.RoleID == roleID);
                        if (dongfufive != null)
                        {
                            dongfuObj.PreemptOne.Remove(dongfufive);
                        }
                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceDongFuPostfix, id.ToString(), dongfuObj);
                        await NHibernateQuerier.UpdateAsync(ChangeDataType(dongfuObj));
                    }

                    if (memberObj.Member.Contains(roleID))
                    {
                        memberObj.Member.Remove(roleID);
                    }

                    roleallianceObj.AllianceJob = 0;
                    roleallianceObj.AllianceID = 0;

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, id.ToString(), statusObj);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), memberObj);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), roleallianceObj);

                    await NHibernateQuerier.UpdateAsync(statusObj);
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(memberObj));
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(roleallianceObj));

                    return roleallianceObj;
                }
                else
                    return null;
            }
            else
                return null;
        }

        async Task<RoleAlliance> QuitAlliance(int roleID, int id)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", id);
            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", id);
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var memberObj = NHibernateQuerier.CriteriaSelect<AllianceMember>(nHCriteria);
            var roleallianceObj = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriarole);
            var dongfuObj = NHibernateQuerier.CriteriaSelect<AllianceDongFu>(nHCriteria);
            var statusObj = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaAlliance);

            if (statusObj != null   && memberObj != null && roleallianceObj != null)
            {
                if (roleallianceObj.AllianceJob == 937)
                {
                    return null;
                }


                statusObj.AllianceNumberPeople -= 1;
                statusObj.OnLineNum -= 1;
                if (dongfuObj != null)
                {
                    var dongfuones = Utility.Json.ToObject<List<PreemptInfo>>(dongfuObj.PreemptOne);
                    var dongfuone = dongfuones.Find(x => x.RoleID == roleID);
                    if (dongfuone != null)
                    {
                        dongfuones.Remove(dongfuone);
                        dongfuObj.PreemptOne = Utility.Json.ToJson(dongfuones);
                    }
                    var dongfutows = Utility.Json.ToObject<List<PreemptInfo>>(dongfuObj.PreemptTow);
                    var dongfutow = dongfutows.Find(x => x.RoleID == roleID);
                    if (dongfutow != null)
                    {
                        dongfutows.Remove(dongfutow);
                        dongfuObj.PreemptTow = Utility.Json.ToJson(dongfutows);
                    }
                    var dongfuthrees = Utility.Json.ToObject<List<PreemptInfo>>(dongfuObj.PreemptThree);
                    var dongfuthree = dongfuthrees.Find(x => x.RoleID == roleID);
                    if (dongfuthree != null)
                    {
                        dongfuthrees.Remove(dongfuthree);
                        dongfuObj.PreemptThree = Utility.Json.ToJson(dongfuthrees);
                    }
                    var dongfufours = Utility.Json.ToObject<List<PreemptInfo>>(dongfuObj.PreemptFour);
                    var dongfufour = dongfufours.Find(x => x.RoleID == roleID);
                    if (dongfufour != null)
                    {
                        dongfufours.Remove(dongfufour);
                        dongfuObj.PreemptFour = Utility.Json.ToJson(dongfufours);
                    }
                    var dongfufives = Utility.Json.ToObject<List<PreemptInfo>>(dongfuObj.PreemptFive);
                    var dongfufive = dongfufives.Find(x => x.RoleID == roleID);
                    if (dongfufive != null)
                    {
                        dongfufives.Remove(dongfufive);
                        dongfuObj.PreemptFive = Utility.Json.ToJson(dongfufives);
                    }
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceDongFuPostfix, id.ToString(), ChangeDataType(dongfuObj));
                    await NHibernateQuerier.UpdateAsync(dongfuObj);
                }


                var members = Utility.Json.ToObject<List<int>>(memberObj.Member);
                if (members.Contains(roleID))
                {
                    members.Remove(roleID);
                    memberObj.Member = Utility.Json.ToJson(members);
                }

                roleallianceObj.AllianceJob = 0;
                roleallianceObj.AllianceID = 0;

                await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, id.ToString(), statusObj);
                await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceMemberPerfix, id.ToString(), ChangeDataType(memberObj));
                await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), ChangeDataType(roleallianceObj));

                await NHibernateQuerier.UpdateAsync(statusObj);
                await NHibernateQuerier.UpdateAsync(memberObj);
                await NHibernateQuerier.UpdateAsync(roleallianceObj);

                return roleallianceObj;
            }
            else
                return null;
        }

        AllianceMember ChangeDataType(AllianceMemberDTO memberDTO)
        {
            AllianceMember alliance = new AllianceMember();
            alliance.AllianceID = memberDTO.AllianceID;
            alliance.ApplyforMember =Utility.Json.ToJson(memberDTO.ApplyforMember) ;
            alliance.Member = Utility.Json.ToJson(memberDTO.Member);
            alliance.JobNumDict = Utility.Json.ToJson(memberDTO.JobNumDict);
            return alliance;
        }

        AllianceMemberDTO ChangeDataType(AllianceMember member)
        {
            AllianceMemberDTO allianceDTO = new AllianceMemberDTO();
            allianceDTO.AllianceID = member.AllianceID;
            allianceDTO.ApplyforMember = Utility.Json.ToObject<List<int>>(member.ApplyforMember);
            allianceDTO.Member = Utility.Json.ToObject<List<int>>(member.Member);
            allianceDTO.JobNumDict = Utility.Json.ToObject<Dictionary<int,int>>(member.JobNumDict);
            return allianceDTO;
        }

        AllianceStatus ChangeDataType(AllianceStatusDTO statusDTO)
        {
            AllianceStatus allianceStatus = new AllianceStatus();
            allianceStatus.AllianceLevel = statusDTO.AllianceLevel;
            allianceStatus.AllianceMaster = statusDTO.AllianceMaster;
            allianceStatus.AllianceName = statusDTO.AllianceName;
            allianceStatus.AllianceNumberPeople = statusDTO.AllianceNumberPeople;
            allianceStatus.AlliancePeopleMax = statusDTO.AlliancePeopleMax;
            allianceStatus.ID = statusDTO.ID;
            allianceStatus.Manifesto = statusDTO.Manifesto;
            allianceStatus.OnLineNum = statusDTO.OnLineNum;
            allianceStatus.Popularity = statusDTO.Popularity;
            return allianceStatus;
        }

        AllianceDongFu ChangeDataType(AllianceDongFuDTO dongFuDTO)
        {
            AllianceDongFu allianceDongFu = new AllianceDongFu();
            allianceDongFu.AllianceID = dongFuDTO.AllianceID;
            allianceDongFu.Occupant = dongFuDTO.Occupant;
            allianceDongFu.PreemptOne = Utility.Json.ToJson(dongFuDTO.PreemptOne);
            allianceDongFu.PreemptTow = Utility.Json.ToJson(dongFuDTO.PreemptTow);
            allianceDongFu.PreemptThree = Utility.Json.ToJson(dongFuDTO.PreemptThree);
            allianceDongFu.PreemptFour = Utility.Json.ToJson(dongFuDTO.PreemptFour);
            allianceDongFu.PreemptFive = Utility.Json.ToJson(dongFuDTO.PreemptFive);
            return allianceDongFu;
        }

    }
}
