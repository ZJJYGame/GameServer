using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using RedisDotNet;
using AscensionProtocol.DTO;
using AscensionProtocol;
using AscensionServer.Model;

namespace AscensionServer
{
    public partial class PracticeManager
    {
        #region Redis模块
        /// <summary>
        /// 获得角色所有功法
        /// </summary>
        /// <param name="RoleID"></param>
        async void GetRoleGongFaS2C(int RoleID)
        {
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleGongfaPerfix, RoleID.ToString()).Result)
            {
                var roleGongFa = await RedisHelper.Hash.HashGetAsync<RoleGongFaDTO>(RedisKeyDefine._RoleGongfaPerfix, RoleID.ToString());
                if (roleGongFa!=null)
                {
                    ResultSuccseS2C(RoleID,PracticeOpcode.GetRoleGongfa, roleGongFa);
                }
                else
                    GetRoleGongFaMySql(RoleID);
            }
            else
                GetRoleGongFaMySql(RoleID);
        }
        /// <summary>
        /// 获得角色秘术模块
        /// </summary>
        /// <param name="RoleID"></param>
        async void GetRoleMiShuS2C(int RoleID)
        {
            if (RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleMiShuPerfix, RoleID.ToString()).Result)
            {
                Utility.Debug.LogError("获取秘术"+ RoleID);
                var roleMiShu = await RedisHelper.Hash.HashGetAsync<RoleMiShuDTO>(RedisKeyDefine._RoleMiShuPerfix, RoleID.ToString());
                if (roleMiShu != null)
                {
                    ResultSuccseS2C(RoleID, PracticeOpcode.GetRoleMiShu, roleMiShu);
                }
                else
                    GetRoleMiShuMySql(RoleID);
            }
            else
                GetRoleMiShuMySql(RoleID);
        }

        async void AddGongFaS2C(int roleid,int id)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, GongFaBook>>(out var bookDict);
            GameEntry.DataManager.TryGetValue<Dictionary<int, GongFa>>(out var gongfaDict);
            if (!bookDict.TryGetValue(id, out var book))
            {
                Utility.Debug.LogError("学习功法失败1");
                ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
                return;
            }

            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleid.ToString()).Result;
            var rolegongfaExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleGongfaPerfix,roleid.ToString()).Result;
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            if (rolegongfaExist && roleExist && ringServer != null)
            {
                if (!InventoryManager.VerifyIsExist(id, 1, ringServer.RingIdArray))
                {
                    Utility.Debug.LogError("学习功法失败2");
                    ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
                    return;
                }
                var rolegongfa = RedisHelper.Hash.HashGetAsync<RoleGongFaDTO>(RedisKeyDefine._RoleGongfaPerfix, roleid.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<Role>(RedisKeyDefine._RolePostfix, roleid.ToString()).Result;

                if (rolegongfa != null && role != null)
                {
                    if (book.NeedRoleLeve > role.RoleLevel)
                    {
                        Utility.Debug.LogError("学习功法失败3");
                        ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
                        return;
                    }

                    if (!role.RoleRoot.Contains(book.BookProperty.ToString()))
                    {
                        Utility.Debug.LogError("学习功法失败5"+ role.RoleRoot);
                        ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
                        return;
                    }

                    if (rolegongfa.GongFaIDDict.Count != 0)
                    {
                        if (rolegongfa.GongFaIDDict.ContainsKey(book.GongfaID) && !rolegongfa.GongFaIDDict.ContainsKey(book.NeedGongfaID))
                        {
                            Utility.Debug.LogError("学习功法失败4");
                            ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
                            return;
                        }
                        else
                        {
                            CultivationMethodDTO cultivationMethodDTO = new CultivationMethodDTO();
                            cultivationMethodDTO.CultivationMethodID = book.GongfaID;
                            cultivationMethodDTO.CultivationMethodLevel = (short)book.NeedRoleLeve;
                            for (int i = 0; i < gongfaDict[book.GongfaID].Skill_One.Count; i++)
                            {
                                if (role.RoleLevel >= gongfaDict[book.GongfaID].Skill_One_At_Level[i])
                                {
                                    cultivationMethodDTO.CultivationMethodLevelSkillArray.Add(gongfaDict[book.GongfaID].Skill_One[i]);
                                }
                            }
                            rolegongfa.GongFaIDDict.Add(cultivationMethodDTO.CultivationMethodID, cultivationMethodDTO);

                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.RoleGongFa, rolegongfa);
                            dict.Add((byte)ParameterCode.GongFa, cultivationMethodDTO);
                            dict.Add((byte)ParameterCode.RoleStatus, new RoleStatus());
                            dict.Add((byte)ParameterCode.Role, role);
                            ResultSuccseS2C(roleid, PracticeOpcode.AddGongFa, dict);

                            InventoryManager.Remove(roleid, id);
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleGongfaPerfix, roleid.ToString(), rolegongfa);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(rolegongfa));
                        }
                    }
                    else
                    {
                        CultivationMethodDTO cultivationMethodDTO = new CultivationMethodDTO();
                        cultivationMethodDTO.CultivationMethodID = book.GongfaID;
                        cultivationMethodDTO.CultivationMethodLevel = 1;
                        cultivationMethodDTO.CultivationMethodLevelSkillArray.Add(gongfaDict[book.GongfaID].Skill_One[0]);

                        rolegongfa.GongFaIDDict.Add(cultivationMethodDTO.CultivationMethodID, cultivationMethodDTO);

                        role.RoleLevel = 1;


                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.RoleGongFa, rolegongfa);
                        dict.Add((byte)ParameterCode.GongFa, cultivationMethodDTO);
                        dict.Add((byte)ParameterCode.RoleStatus, new RoleStatus());
                        dict.Add((byte)ParameterCode.Role, role);
                        ResultSuccseS2C(roleid, PracticeOpcode.AddGongFa, dict);

                        InventoryManager.Remove(roleid, id);
                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleGongfaPerfix, roleid.ToString(), rolegongfa);
                        await RedisHelper.Hash.HashSetAsync<Role>(RedisKeyDefine._RolePostfix, roleid.ToString(), role);

                        await NHibernateQuerier.UpdateAsync(ChangeDataType(rolegongfa));
                        await NHibernateQuerier.UpdateAsync(role);

                    }
                }
                else
                {
                    Utility.Debug.LogError("学习功法失败4");
                    ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
                }
            }
            else
            {
                Utility.Debug.LogError("学习功法失败4");
                ResultFailS2C(roleid, PracticeOpcode.AddGongFa);
            }

        }

        async void AddMiShuS2C(int roleid, int id)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, MishuBook>>(out var bookDict);
            GameEntry.DataManager.TryGetValue<Dictionary<int, MiShuData>>(out var mishuDict);

            if (!bookDict.TryGetValue(id, out var book))
            {
                Utility.Debug.LogError("学习秘术失败1");
                ResultFailS2C(roleid, PracticeOpcode.AddMiShu);
                return;
            }
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var rolemishuExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleMiShuPerfix,roleid.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleid.ToString()).Result;
            if (rolemishuExist&&roleExist&& ringServer!=null)
            {
                if (!InventoryManager.VerifyIsExist(id,1, ringServer.RingIdArray))
                {
                    Utility.Debug.LogError("学习秘术失败2");
                    ResultFailS2C(roleid, PracticeOpcode.AddMiShu);
                    return;
                }

                var rolemishu = RedisHelper.Hash.HashGetAsync<RoleMiShuDTO>(RedisKeyDefine._RoleMiShuPerfix, roleid.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix, roleid.ToString()).Result;
                if (role!=null&& rolemishu!=null)
                {
                    if (rolemishu.MiShuIDDict.ContainsKey(bookDict[id].MishuID))
                    {
                        Utility.Debug.LogError("学习秘术失败3");
                        ResultFailS2C(roleid, PracticeOpcode.AddMiShu);
                        return;
                    }

                    if (bookDict[id].NeedRoleLevel > role.RoleLevel)
                    {
                        Utility.Debug.LogError("学习秘术失败4");
                        ResultFailS2C(roleid, PracticeOpcode.AddMiShu);
                        return;
                    }

                    if (book.BookProperty!=0)
                    {
                        if (!role.RoleRoot.Contains(book.BookProperty.ToString()))
                        {
                            Utility.Debug.LogError("学习秘术失败5");
                            ResultFailS2C(roleid, PracticeOpcode.AddMiShu);
                            return;
                        }

                    }
                    Utility.Debug.LogError("读取到的学习秘术的数据" + Utility.Json.ToJson(mishuDict[bookDict[id].MishuID].mishuSkillDatas));
                    var mishuData = mishuDict[bookDict[id].MishuID].mishuSkillDatas.Find(x=>x.MishuFloor==1);
                    Utility.Debug.LogError("读取到的学习秘术的数据"+Utility.Json.ToJson(mishuData));
                    MiShuDTO miShuDTO = new MiShuDTO() {  MiShuSkillArry = mishuData.SkillArrayOne, MiShuAdventureSkill = mishuData.SkillArrayTwo, MiShuLevel = (short)mishuData.NeedLevelID, MiShuID = bookDict[id].MishuID };

                    rolemishu.MiShuIDDict.Add(bookDict[id].MishuID, miShuDTO);
                    InventoryManager.Remove(roleid, id);
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.RoleStatus,new RoleStatus());
                    dict.Add((byte)ParameterCode.RoleMiShu, rolemishu);
                    dict.Add((byte)ParameterCode.MiShu, miShuDTO);

                    ResultSuccseS2C(roleid,PracticeOpcode.AddMiShu, dict);

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleMiShuPerfix, roleid.ToString(), rolemishu);

                    await NHibernateQuerier.UpdateAsync(ChangeDataType(rolemishu));

                }
            }

        }
        #endregion

        #region MySql模块
        /// <summary>
        /// 获取数据库数据
        /// </summary>
        /// <param name="RoleID"></param>
        async void GetRoleGongFaMySql(int RoleID)
        {
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", RoleID);
            var role = NHibernateQuerier.CriteriaSelectAsync<RoleGongFa>(nHCriteriaRole).Result;
            if (role != null)
            {
                ResultSuccseS2C(RoleID, PracticeOpcode.GetRoleGongfa, ChangeDataType(role));
            }
            else
            {
                ResultFailS2C(RoleID, PracticeOpcode.GetRoleGongfa);
            }

        }
        /// <summary>
        /// 获取数据库数据
        /// </summary>
        /// <param name="RoleID"></param>
        async void GetRoleMiShuMySql(int RoleID)
        {

            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", RoleID);
            var role = NHibernateQuerier.CriteriaSelectAsync<RoleMiShu>(nHCriteriaRole).Result;
            if (role != null)
            {
                Utility.Debug.LogError("获取秘术" + RoleID);
                ResultSuccseS2C(RoleID, PracticeOpcode.GetRoleMiShu, ChangeDataType(role));
            }
            else
            {
                ResultFailS2C(RoleID, PracticeOpcode.GetRoleMiShu);
            }

        }
        #endregion
    
        #region
        CultivationMethodDTO ChangeGongFa(CultivationMethod cultivation)
        {
            CultivationMethodDTO cultivationMethodDTO = new CultivationMethodDTO();
            cultivationMethodDTO.CultivationMethodExp = cultivation.CultivationMethodExp;
            cultivationMethodDTO.CultivationMethodID = cultivation.CultivationMethodID;
            cultivationMethodDTO.CultivationMethodLevel = cultivation.CultivationMethodLevel;
            cultivationMethodDTO.CultivationMethodLevelSkillArray = Utility.Json.ToObject<List<int>>(cultivation.CultivationMethodLevelSkillArray);
            return cultivationMethodDTO;
        }

        MiShuDTO ChangeMiShu(MiShu miShu)
        {
            MiShuDTO miShuDTO = new MiShuDTO();
            miShuDTO.MiShuAdventureSkill = Utility.Json.ToObject<List<int>>(miShu.MiShuAdventtureSkill);
            miShuDTO.MiShuExp = miShu.MiShuExp;
            miShuDTO.MiShuID = miShu.MiShuID;
            miShuDTO.MiShuLevel = miShu.MiShuLevel;
            miShuDTO.MiShuSkillArry = Utility.Json.ToObject<List<int>>(miShu.MiShuSkillArry);

            return miShuDTO;
        }

        MiShu ChangeMiShu(MiShuDTO miShuDTO)
        {
            MiShu miShu = new MiShu();
            miShu.MiShuAdventtureSkill = Utility.Json.ToJson(miShuDTO.MiShuAdventureSkill);
            miShu.MiShuExp = miShuDTO.MiShuExp;
            miShu.MiShuID = miShuDTO.MiShuID;
            miShu.MiShuLevel = miShuDTO.MiShuLevel;
            miShu.MiShuSkillArry = Utility.Json.ToJson(miShuDTO.MiShuSkillArry);

            return miShu;
        }

        RoleGongFaDTO ChangeDataType(RoleGongFa roleGongFa)
        {
            RoleGongFaDTO roleGongFaObj = new RoleGongFaDTO();
            roleGongFaObj.RoleID = roleGongFa.RoleID;
            roleGongFaObj.GongFaIDDict = Utility.Json.ToObject<Dictionary<int , CultivationMethodDTO>>(roleGongFa.GongFaIDDict);
            return roleGongFaObj;
        }
        RoleGongFa ChangeDataType(RoleGongFaDTO roleGongFa)
        {
            RoleGongFa roleGongFaObj = new RoleGongFa();
            roleGongFaObj.RoleID = roleGongFa.RoleID;
            roleGongFaObj.GongFaIDDict = Utility.Json.ToJson(roleGongFa.GongFaIDDict);
            return roleGongFaObj;
        }

        RoleMiShuDTO ChangeDataType(RoleMiShu roleMiShu)
        {
            RoleMiShuDTO roleMiShuObj = new RoleMiShuDTO();
            roleMiShuObj.RoleID = roleMiShu.RoleID;
            roleMiShuObj.MiShuIDDict = Utility.Json.ToObject<Dictionary<int, MiShuDTO>>(roleMiShu.MiShuIDDict);
            return roleMiShuObj;
        }

        RoleMiShu ChangeDataType(RoleMiShuDTO roleMiShu)
        {
            RoleMiShu roleMiShuObj = new RoleMiShu();
            roleMiShuObj.RoleID = roleMiShu.RoleID;
            roleMiShuObj.MiShuIDDict = Utility.Json.ToJson(roleMiShu.MiShuIDDict);
            return roleMiShuObj;
        }

}
        #endregion

    }


