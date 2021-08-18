using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisDotNet;
using AscensionProtocol.DTO;
using AscensionProtocol;
using Cosmos;
using AscensionServer.Model;

namespace AscensionServer
{
    public partial class GangsMananger
    {
        #region Redis模块
        async void GetRoleAllianceS2C(int roleID )
        {
            var roleAllianceExist =  RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix,roleID.ToString()).Result;
            if (roleAllianceExist)
            {
                var roleAlliance = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                if (roleAlliance != null)
                {
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.GetRoleAlliance, roleAlliance);
                }
                else
                    GetRoleAliianceMySql(roleID);
            } else
                GetRoleAliianceMySql(roleID);
        }
        /// <summary>
        /// 获得现有宗门,按照
        /// </summary>
        async void GetAllAllianceS2C(int roleID,AlliancesDTO alliances)
        {
            var alliancesExist = RedisHelper.String.StringGetAsync(RedisKeyDefine._AllianceListPerfix).Result;
            if (alliancesExist != null)
            {
                var allianceList = Utility.Json.ToObject<List<int>>(alliancesExist);
                List<AllianceStatusDTO> AllianceList = new List<AllianceStatusDTO>();
                if (alliances.Index <= allianceList.Count)
                {
                    if (alliances.AllIndex <= allianceList.Count)
                    {
                        for (int i = alliances.Index; i < alliances.AllIndex; i++)
                        {
                            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, allianceList[i].ToString()).Result;
                            if (result)
                            {
                                var alliance = RedisHelper.Hash.HashGetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, allianceList[i].ToString()).Result;
                                if (alliance != null)
                                {
                                    AllianceList.Add(alliance);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = alliances.Index; i < allianceList.Count; i++)
                        {
                            Utility.Debug.LogInfo("YZQ获取的Redis宗门的列表的元素" + allianceList[i]);
                            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, allianceList[i].ToString()).Result;
                            if (result)
                            {
                                var alliance = RedisHelper.Hash.HashGetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, allianceList[i].ToString()).Result;
                                if (alliance != null)
                                {
                                    AllianceList.Add(alliance);
                                }
                            }
                        }
                        alliances.ISRefresh = false;
                    }
                }
                else
                {
                    alliances.ISRefresh = false;
                }
                alliances.AllianceList = allianceList;
                Utility.Debug.LogInfo("YZQ获取的Redis宗门的列表的元素" + Utility.Json.ToJson(AllianceList));
                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                dict.Add((byte)ParameterCode.Alliances, alliances);
                dict.Add((byte)ParameterCode.AllianceStatus, AllianceList);
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAlliances, dict);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.GetAlliances);
        }
        /// <summary>
        /// 创建宗门
        /// </summary>
        /// <param name="statusDTO"></param>
        async void CreatAllianceS2C(int roleID,AllianceStatus statusDTO)
        {
            GameEntry. DataManager.TryGetValue<Dictionary<int, CreatAllianceData>>(out var CreatAlliance);

            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("ID", 1);
            var roleAssets = NHibernateQuerier.CriteriaSelectAsync<RoleAssets>(nHCriteriaRole).Result;

            var role = NHibernateQuerier.CriteriaSelectAsync<Role>(nHCriteriaRole).Result;

            var roleAlliance = NHibernateQuerier.CriteriaSelectAsync<RoleAlliance>(nHCriteriaRole).Result;

            var AllAlliance = NHibernateQuerier.CriteriaSelectAsync<Alliances>(nHCriteria).Result;
            Utility.Debug.LogError("创建宗门进来了"+ (roleAssets != null) + (role != null) + (roleAlliance != null));
            if (roleAssets != null && role != null && roleAlliance != null)
            {
                Utility.Debug.LogError("创建宗门进来了" + (roleAssets.SpiritStonesLow >= CreatAlliance[1].ScripturesPlatform) + (role.RoleLevel >= CreatAlliance[1].RoleLevel) );
                //TODO判断灵石够不够
                if (roleAssets.SpiritStonesLow >= CreatAlliance[1].SpiritStones && role.RoleLevel >= CreatAlliance[1].RoleLevel)
                {
                    statusDTO.AllianceLevel = 1;
                    statusDTO.AllianceNumberPeople = 1;
                    statusDTO.AlliancePeopleMax = 30;
                    statusDTO.Popularity = CreatAlliance[1].Popularity;
                    statusDTO.AllianceMaster = role.RoleName;
                    statusDTO.AllianceNumberPeople = 1;
                    statusDTO.OnLineNum = 1;
                    //TODO判断名字是否重复
                    statusDTO = await NHibernateQuerier.InsertAsync(statusDTO);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString(), statusDTO);
                    //初始化宗门数据,宗门建设,
                    if (AllAlliance!=null)
                    {
                        var alliances = Utility.Json.ToObject<List<int>>(AllAlliance.AllianceList);
                        alliances.Add(statusDTO.ID);
                        AllAlliance.AllianceList = Utility.Json.ToJson(alliances);
                        await NHibernateQuerier.UpdateAsync(AllAlliance);
                       await  RedisHelper.String.StringSetAsync(RedisKeyDefine._AllianceListPerfix, alliances);
                    }

                    AllianceConstruction allianceConstruction = new AllianceConstruction();
                    allianceConstruction.AllianceID = statusDTO.ID;
                    allianceConstruction.AllianceAssets = CreatAlliance[1].SpiritStones;
                    await NHibernateQuerier.InsertAsync(allianceConstruction);

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceConstructionPerfix, statusDTO.ID.ToString(), allianceConstruction);

                    AllianceExchangeGoods exchangeGoods = new AllianceExchangeGoods();
                    exchangeGoods.AllianceID = statusDTO.ID; ;
                    await NHibernateQuerier.InsertAsync(exchangeGoods);
                    AllianceExchangeGoodsDTO allianceExchange = new AllianceExchangeGoodsDTO();
                    allianceExchange.AllianceID = statusDTO.ID;
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AllianceExchangeGoodsPerfix, statusDTO.ID.ToString(), allianceExchange);

                    AllianceMember alliance = new AllianceMember();
                    alliance.AllianceID = statusDTO.ID;
                    alliance.Member = Utility.Json.ToJson(new List<int>() { roleID });
                    await NHibernateQuerier.InsertAsync(alliance);
                    AllianceMemberDTO memberDTO = new AllianceMemberDTO();
                    memberDTO.AllianceID = statusDTO.ID;
                    memberDTO.Member = new List<int>() { roleID };
                    await RedisHelper.Hash.HashSetAsync<AllianceMemberDTO>(RedisKeyDefine._AllianceMemberPerfix, statusDTO.ID.ToString(), memberDTO);


                    roleAlliance.AllianceID = statusDTO.ID;
                    roleAlliance.AllianceJob = 937;//需要职位的表
                    roleAlliance.JoinTime = DateTime.Now.ToString();
                    roleAlliance.Reputation = 0;
                    roleAlliance.ReputationHistroy = 0;
                    roleAlliance.ReputationMonth = 0;
                    await RedisHelper.Hash.HashSetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString(), ChangeDataType(roleAlliance));
                    await NHibernateQuerier.UpdateAsync(roleAlliance);

                    //宗门建设信息，个人宗门信息
                    roleAssets.SpiritStonesLow -= CreatAlliance[1].SpiritStones;
                    await RedisHelper.Hash.HashSetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), roleAssets);
                    await NHibernateQuerier.UpdateAsync(roleAssets);


                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.RoleAlliance, ChangeDataType (roleAlliance));
                    dict.Add((byte)ParameterCode.AllianceStatus, statusDTO);
                    dict.Add((byte)ParameterCode.RoleAssets, roleAssets);
                    dict.Add((byte)ParameterCode.AllianceConstruction, allianceConstruction);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.CreatAlliance, dict);
                }
                else
                    RoleStatusFailS2C(roleID, AllianceOpCode.CreatAlliance);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.CreatAlliance);
        }    
        /// <summary>
        /// 获得宗门通告
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="ID"></param>
        async void GetAllianceCallboardS2C(int roleID,int ID, DailyMessageDTO daily)
        {
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._DailyMessagePerfix, ID.ToString()).Result;
            if (result)
            {
                //按照日期记录每天的通告，每次获取先取最新日期，判断是否更新，如若更新发送更新的反之发送前一天的，DateTime.Now.AddDays(-30).ToString("MM-dd")，储存是按照年月为key,欠缺如何删除上一月的数据
                #region 待删
                //var dailyObj = RedisHelper.Hash.HashGetAsync<List<DailyMessageDTO>>(RedisKeyDefine._DailyMessagePerfix, ID.ToString()).Result;
                //Utility.Debug.LogInfo("YZQ>>>>>>>>>>>>" + Utility.Json.ToJson(dailyObj));
                //if (dailyObj != null)
                //{
                //    RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAlliancecallboard, dailyObj);
                //}
                //else
                //    RoleStatusFailS2C(roleID, AllianceOpCode.GetAlliancecallboard);
                #endregion

                DailyMessageDTO dTO = new DailyMessageDTO();
                List<DailyMessageData> dailies = new List<DailyMessageData>();

                var dailyObj = RedisHelper.Hash.HashGetAsync<DailyMessageDTO>(RedisKeyDefine._DailyMessagePerfix, ID.ToString()).Result;
                if (dailyObj!=null)
                {
                    if (dailyObj.DailyMessageDict.TryGetValue(DateTime.Now.ToString("MM-dd"), out var dTOs))
                    {
                        if (dTOs.Count > daily.CurrentIndex)
                        {
                            for (int i = daily.CurrentIndex; i < dTOs.Count; i++)
                            {
                                dailies.Add(dTOs[i]);
                            }
                            dTO.DailyMessageDict.Add(DateTime.Now.ToString("MM-dd"), dailies);

                            if (dailyObj.IsFirstGet)
                            {
                                string data = DateTime.Now.AddDays(-30).ToString("MM-dd");
                                if (dailyObj.DailyMessageDict.TryGetValue(data, out var yesterDTOs))
                                {
                                    dTO.DailyMessageDict.Add(data, yesterDTOs);
                                }
                            }
                        }
                        else
                        {
                            DateTime time = Convert.ToDateTime(dailyObj.DataTimeNow);
                            string data = time.AddDays(-30).ToString("MM-dd");
                            if (dailyObj.DailyMessageDict.TryGetValue(data, out var yesterDTOs))
                            {
                                dTO.DailyMessageDict.Add(data, yesterDTOs);
                            }
                        }
                    }
                    else
                    {
                        DateTime time = Convert.ToDateTime(dailyObj.DataTimeNow);
                        string data = time.AddDays(-30).ToString("MM-dd");
                        if (dailyObj.DailyMessageDict.TryGetValue(data, out var yesterDTOs))
                        {
                            dTO.DailyMessageDict.Add(data, yesterDTOs);
                        }
                        else
                        {
                            RoleStatusFailS2C(roleID, AllianceOpCode.GetAlliancecallboard);
                        }
                    }

                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.DailyMessage, dTO);
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.GetAlliancecallboard, dict);
                    //消息发送客户端
                }
            }
        }

        /// <summary>
        /// 修改宗門名稱
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="statusDTO"></param>
        async void ChangeAllianceNameS2C(int roleID,AllianceStatus statusDTO)
        {
            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
            var assestExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            if (allianceExist && assestExist&& roleExist)
            {
                var alliance = RedisHelper.Hash.HashGetAsync<AllianceStatus>(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString()).Result;
                var assest = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleAllianceDTO>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;
                if (alliance != null && assest != null&& role!=null)
                {
                    if (role.AllianceJob==937)
                    {
                        if (assest.SpiritStonesLow >= 500000)
                        {
                            alliance.AllianceName = statusDTO.AllianceName;
                            assest.SpiritStonesLow -= 500000;
                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.RoleAssets, assest);
                            dict.Add((byte)ParameterCode.AllianceStatus, alliance);
                            RoleStatusSuccessS2C(roleID, AllianceOpCode.ChangeAllianceName, dict);

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix, statusDTO.ID.ToString(), assest);
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString(), alliance);
                            await NHibernateQuerier.UpdateAsync(alliance);
                            await NHibernateQuerier.UpdateAsync(assest);
                        }
                        else
                            RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAllianceName);
                    }else
                        RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAllianceName);
                }
                else
                {
                    ChangeAllianceNameMySQL(roleID, statusDTO);
                }
            }
            else
            {
                ChangeAllianceNameMySQL(roleID, statusDTO);
            }
        }

        /// <summary>
        /// 修改宗門宗旨
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="statusDTO"></param>
        async void ChangeAlliancePurposeS2C(int roleID,AllianceStatus statusDTO)
        {
            var allianceExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAlliancePerfix, statusDTO.ID.ToString()).Result;

            if (allianceExist&&roleExist)
            {
                var alliance = RedisHelper.Hash.HashGetAsync<AllianceStatusDTO>(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleAlliance>(RedisKeyDefine._RoleAlliancePerfix, roleID.ToString()).Result;

                if (alliance != null&& role!=null)
                {
                    if (role.AllianceJob==937)
                    {
                        alliance.Manifesto = statusDTO.Manifesto;
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.ChangeAlliancePurpose, alliance);

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString(), alliance);
                        await NHibernateQuerier.UpdateAsync(alliance);
                    }
                    else
                        RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAlliancePurpose);
                }
                else
                {
                    ChangeAllianceNameMySQL(roleID, statusDTO);
                }
            }
            else
            {
                ChangeAllianceNameMySQL(roleID, statusDTO);
            }
        }

        #endregion
        #region MySql模块
        /// <summary>
        /// 获取角色自身宗门数据
        /// </summary>
        /// <param name="roleID"></param>
        void GetRoleAliianceMySql(int roleID)
        {
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var roleAlliance = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriaRole);
            Utility.Debug.LogInfo("获得角色宗門数据2" +Utility.Json.ToJson(roleAlliance));
            if (roleAlliance != null)
            {
                RoleStatusSuccessS2C(roleID, AllianceOpCode.GetRoleAlliance, ChangeDataType(roleAlliance));
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.GetRoleAlliance);
        }
        /// <summary>
        /// 修改宗門名稱
        /// </summary>
       async void ChangeAllianceNameMySQL(int roleID, AllianceStatus statusDTO)
        {
            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", statusDTO.ID);
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var alliance = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaAlliance);
            NHCriteria nHCriteriaconstruction =ReferencePool.Accquire<NHCriteria>().SetValue("AllianceID", statusDTO.ID);
            var role = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriarole);
            var assest = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriarole);
            if (alliance != null&& assest != null&& role!=null)
            {
                if (role.AllianceJob==937)
                {
                    if (assest.SpiritStonesLow >= 500000)
                    {
                        alliance.AllianceName = statusDTO.AllianceName;
                        assest.SpiritStonesLow -= 500000;
                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.RoleAssets, assest);
                        dict.Add((byte)ParameterCode.AllianceStatus, alliance);
                        RoleStatusSuccessS2C(roleID, AllianceOpCode.ChangeAllianceName, dict);

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix, statusDTO.ID.ToString(), assest);
                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString(), alliance);
                        await NHibernateQuerier.UpdateAsync(alliance);
                        await NHibernateQuerier.UpdateAsync(assest);
                    }
                    else
                        RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAllianceName);
                }
                else
                    RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAllianceName);
            }
            else
                RoleStatusFailS2C(roleID,AllianceOpCode.ChangeAllianceName);

        }
        /// <summary>
        /// 修改宗門宗旨
        /// </summary>
        async void ChangeAlliancePurposeMySql(int roleID, AllianceStatus statusDTO)
        {
            NHCriteria nHCriteriaAlliance =ReferencePool.Accquire<NHCriteria>().SetValue("ID", statusDTO.ID);
            var alliance = NHibernateQuerier.CriteriaSelect<AllianceStatus>(nHCriteriaAlliance);

            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var role = NHibernateQuerier.CriteriaSelect<RoleAlliance>(nHCriteriarole);
            if (alliance != null&& role!=null)
            {
                if (role.AllianceJob==937)
                {
                    if (!string.IsNullOrEmpty(statusDTO.Manifesto))
                    {
                        alliance.Manifesto = statusDTO.Manifesto;
                    }
                    RoleStatusSuccessS2C(roleID, AllianceOpCode.ChangeAlliancePurpose, alliance);

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._AlliancePerfix, statusDTO.ID.ToString(), alliance);
                    await NHibernateQuerier.UpdateAsync(alliance);
                }
                else
                    RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAlliancePurpose);
            }
            else
                RoleStatusFailS2C(roleID, AllianceOpCode.ChangeAlliancePurpose);
        }
        #endregion

        /// <summary>
        ///增加每日宗门通告
        /// </summary>
        /// <param name="describe"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public DailyMessageData DailyMsg(string name, byte describe,string content=null)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, DailyMsg>>(out var dM);
            DailyMessageData messageData = new DailyMessageData();
            messageData.Name = name;
            messageData.Describe = dM[describe].MsgContent;
            messageData.EventContent = content;
            return messageData;
        }
        public async void AddDailyMsg(int roleid, DailyMessageData data)
        {
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._DailyMessagePerfix, roleid.ToString()).Result)
            {
                var daily = RedisHelper.Hash.HashGetAsync<DailyMessageDTO>(RedisKeyDefine._DailyMessagePerfix, roleid.ToString()).Result;
               if (daily!=null)
                {
                    if (daily.DailyMessageDict.TryGetValue(DateTime.Now.ToString("MM-dd"), out var datas))
                    {
                        datas.Add(data);
                        daily.DailyMessageDict[DateTime.Now.ToString("MM-dd")] = datas;
                    }
                    else
                    {
                        List<DailyMessageData> dailies = new List<DailyMessageData>();
                        dailies.Add(data);
                        daily.DailyMessageDict.Add(DateTime.Now.ToString("MM-dd"), dailies);
                    }
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._DailyMessagePerfix, roleid.ToString(), daily);
                }
            }
          
        }

        /// <summary>
        /// 宗门通告类型
        /// </summary>
        public enum DailyMsgType : byte
        {
            JoinAlliance = 0,
            JobChange = 1,
            LevelUp = 2,
            GetTitle = 3,
            Adveture = 4,
            BuildingUp = 5,
            LeaveAlliance = 6,
            Contribute = 7
        }

        RoleAllianceDTO ChangeDataType(RoleAlliance roleAlliance)
        {
            RoleAllianceDTO roleAllianceDTO = new RoleAllianceDTO();
            roleAllianceDTO.AllianceID = roleAlliance.AllianceID;
            roleAllianceDTO.AllianceJob = roleAlliance.AllianceJob;
            roleAllianceDTO.ApplyForAlliance = Utility.Json.ToObject<List<int>>(roleAlliance.ApplyForAlliance);
            roleAllianceDTO.Offline = roleAlliance.Offline;
            roleAllianceDTO.JoinTime = roleAlliance.JoinTime;
            roleAllianceDTO.Reputation = roleAlliance.Reputation;
            roleAllianceDTO.ReputationHistroy = roleAlliance.ReputationHistroy;
            roleAllianceDTO.ReputationMonth = roleAlliance.ReputationMonth;
            roleAllianceDTO.RoleID = roleAlliance.RoleID;
            roleAllianceDTO.RoleName = roleAlliance.RoleName;
            return roleAllianceDTO;
        }

        RoleAlliance ChangeDataType(RoleAllianceDTO AllianceDTO)
        {
            RoleAlliance roleAlliance = new RoleAlliance();
            roleAlliance.AllianceID = AllianceDTO.AllianceID;
            roleAlliance.AllianceJob = AllianceDTO.AllianceJob;
            roleAlliance.ApplyForAlliance = Utility.Json.ToJson(AllianceDTO.ApplyForAlliance);
            roleAlliance.Offline = AllianceDTO.Offline;
            roleAlliance.JoinTime = AllianceDTO.JoinTime;
            roleAlliance.Reputation = AllianceDTO.Reputation;
            roleAlliance.ReputationHistroy = AllianceDTO.ReputationHistroy;
            roleAlliance.ReputationMonth = AllianceDTO.ReputationMonth;
            roleAlliance.RoleID = AllianceDTO.RoleID;
            roleAlliance.RoleName = AllianceDTO.RoleName;
            return roleAlliance;
        }
    }
}
