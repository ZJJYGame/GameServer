using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;
namespace AscensionServer
{
    public partial class PracticeManager
    {
        #region Redis模块
        /// <summary>
        /// 触发瓶颈发送
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="level"></param>
        Bottleneck TriggerBottleneckS2C(int roleID,int level, out bool isbottleneck)
        {
            isbottleneck = false;
            var bottleneckExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleBottleneckPostfix, roleID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            if (!bottleneckExist  || !roleExist )
            {
                var result = TriggerBottleneck(roleID, level,out isbottleneck);
                if (result != null)
                {
                    return result;
                }
                else
                    return null;
            }
            var bottleneck = RedisHelper.Hash.HashGet<Bottleneck>(RedisKeyDefine._RoleBottleneckPostfix, roleID.ToString());
            var role = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;

            int count = Utility.Json.ToObject<List<int>>(role.RoleRoot).Count;
            #region 待优化
            List<int> rootPercentNum;
            GameEntry.DataManager.TryGetValue<Dictionary<int, BottleneckData>>(out var bottleneckData);
            GameEntry.DataManager.TryGetValue<Dictionary<int, DemonData>>(out var demonData);
            GetRootPercent(bottleneckData[level], count, out rootPercentNum);
            if (GetPercent(rootPercentNum[0] / (float)100))
            {
                bottleneck.IsBottleneck = true;
                bottleneck.BreakThroughVauleMax = rootPercentNum[1];
                isbottleneck = true;
                #region 判断天劫心魔
                //if (bottleneckData[level].IsFinalLevel)
                //{
                //    bottleneck.IsThunder = true;
                //    bottleneck.ThunderRound = bottleneckData[level].Thunder_Round;//获取天劫回合数
                //    int demonIndex = GetDemonPercent(demonData[level], bottleneck.CraryVaule);
                //    if (GetPercent(demonData[level].Trigger_Chance[demonIndex] / (float)100))
                //    {
                //        bottleneck.IsDemon = true;
                //        bottleneck.DemonID = demonData[level].Demon_ID[demonIndex];
                //    }

                //}
                #endregion
            }
            else
                isbottleneck = false;
         return bottleneck;
            #endregion
        }
        #endregion
        #region MySql模块
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="level"></param>
        Bottleneck TriggerBottleneck(int roleID, int level, out bool isbottleneck)
        {
            isbottleneck = false;
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var bottleneck = NHibernateQuerier.CriteriaSelectAsync<Bottleneck>(nHCriteria).Result;
            if (bottleneck == null)
            {
                isbottleneck = false;
                return null;
            }
            var roleObj = NHibernateQuerier.CriteriaSelectAsync<Role>(nHCriteria).Result;
            if (roleObj == null)
            {
               // ResultFailS2C(roleID, PracticeOpcode.TriggerBottleneck);
                return null;
            }
            int count = Utility.Json.ToObject<List<int>>(roleObj.RoleRoot).Count;
            List<int> rootPercentNum;
            GameEntry.DataManager.TryGetValue<Dictionary<int, BottleneckData>>(out var bottleneckData);
            GameEntry.DataManager.TryGetValue<Dictionary<int, DemonData>>(out var demonData);
            GetRootPercent(bottleneckData[level], count, out rootPercentNum);
            if (GetPercent(rootPercentNum[0] / (float)100))
            {
                bottleneck.IsBottleneck = true;
                bottleneck.BreakThroughVauleMax = rootPercentNum[1];
                if (bottleneckData[level].IsFinalLevel)
                {
                    bottleneck.IsThunder = true;
                    bottleneck.ThunderRound = bottleneckData[level].Thunder_Round;//获取天劫回合数
                    int demonIndex = GetDemonPercent(demonData[level], bottleneck.CraryVaule);
                    if (GetPercent(demonData[level].Trigger_Chance[demonIndex] / (float)100))
                    {
                        bottleneck.IsDemon = true;
                        bottleneck.DemonID = demonData[level].Demon_ID[demonIndex];
                    }
                    isbottleneck = true;
                }
            }else
                isbottleneck = false;
            return bottleneck;        
        }
        #endregion
        Random random = new Random();
        private bool GetPercent(float num)
        {
            int randommNum = random.Next(1, 10001);
            float percentNum = num * 10000;
            Utility.Debug.LogInfo("yzqData概率值为" + percentNum + "当前值为" + randommNum);
            if (randommNum <= (int)percentNum)
            {
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// 获取零灵根概率
        /// </summary>
        /// <param name="bottleneckData"></param>
        /// <param name="rootnum"></param>
        /// <returns></returns>
        private void GetRootPercent(BottleneckData bottleneckData, int rootnum, out List<int> num)
        {
            switch (rootnum)
            {
                case 1:
                    num = bottleneckData.Spiritual_Root_1;
                    break;
                case 2:
                    num = bottleneckData.Spiritual_Root_2;
                    break;
                case 3:
                    num = bottleneckData.Spiritual_Root_3;
                    break;
                case 4:
                    num = bottleneckData.Spiritual_Root_4;
                    break;
                case 5:
                    num = bottleneckData.Spiritual_Root_5;
                    break;
                default:
                    num = null;
                    break;
            }

        }
        /// <summary>
        /// 获取心魔对应数组的下标
        /// </summary>
        /// <param name="demonData"></param>
        /// <param name="CraryVaule"></param>
        /// <returns></returns>
        private int GetDemonPercent(DemonData demonData, int CraryVaule)
        {
            int index = 0;
            for (int i = 0; i < demonData.Crary_Value.Count; i++)
            {
                if (CraryVaule >= demonData.Crary_Value[i] && CraryVaule <= demonData.Crary_Value[i + 1])
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        /// <summary>
        /// 使用瓶颈丹药
        /// </summary>
        private async void UseBottleneckElixir(int roleid, int itemid)
        {
            var bottleneckExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleBottleneckPostfix, roleid.ToString()).Result;

            if (EstimatedRUG(roleid, itemid, DrugType.RoleBreakthrough, out var drugData))
            {
                if (bottleneckExist)
                {
                    var bottleneckObj = RedisHelper.Hash.HashGetAsync<Bottleneck>(RedisKeyDefine._RoleBottleneckPostfix, roleid.ToString()).Result;
                    if (bottleneckObj != null)
                    {
                        if (bottleneckObj.IsBottleneck && bottleneckObj.BreakThroughVauleNow < bottleneckObj.BreakThroughVauleMax)
                        {
                            if ((bottleneckObj.BreakThroughVauleNow + drugData.Drug_Value) < bottleneckObj.BreakThroughVauleMax)
                            {
                                bottleneckObj.BreakThroughVauleNow += drugData.Drug_Value;
                            }else
                                bottleneckObj.BreakThroughVauleNow = bottleneckObj.BreakThroughVauleMax;

                            ResultSuccseS2C(roleid, PracticeOpcode.UseBottleneckElixir, bottleneckObj);
                            InventoryManager.UpdateNewItem(roleid, itemid,1);

                           await  RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleBottleneckPostfix, roleid.ToString(), bottleneckObj);
                            await NHibernateQuerier.UpdateAsync(bottleneckObj);
                        }
                        ResultFailS2C(roleid, PracticeOpcode.UseBottleneckElixir);
                    }
                    else
                        ResultNotFoundS2C(roleid, PracticeOpcode.UseBottleneckElixir);
                }
            }
            else
            {
                //返回失败
                ResultNotFoundS2C(roleid, PracticeOpcode.UseBottleneckElixir);
            }


            



        }
        /// <summary>
        /// 判断丹药是否存在及类型
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="itemID"></param>
        public bool EstimatedRUG(int roleid,int itemID,DrugType drugType,out DrugData drugData)
        {
            NHCriteria nHCriteria = ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var ringObj = ReferencePool.Accquire<RingDTO>();
            ringObj.RingItems = new Dictionary<int, RingItemsDTO>();
            ringObj.RingItems.Add(itemID, new RingItemsDTO());
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var nHCriteriaRingID = ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);

            GameEntry.DataManager.TryGetValue<Dictionary<int, DrugData>>(out var DrugDataDict);
            drugData = null;

            if (DrugDataDict.TryGetValue(itemID, out drugData))
            {
                if (drugData.Drug_Type != DrugType.RoleBreakthrough)
                {
                    return false;
                }
            }
            else
                return false;

            if (!InventoryManager.VerifyIsExist(itemID, nHCriteriaRingID))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 突破瓶颈暂用
        /// </summary>
        private async void UpdateBottleneckAsync(RoleDTO role )
        {
            //TODO增加瓶颈判断
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString()).Result;
            var roleExits = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, role.RoleID.ToString()).Result;
            var roleStatusExits = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, role.RoleID.ToString()).Result;
            var roleAssestExits = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, role.RoleID.ToString()).Result;
            GameEntry.DataManager.TryGetValue<Dictionary<int, RoleLevelData>>(out var roleDict);
            if (result&& roleExits&&roleStatusExits&& roleAssestExits)
            {
                var bottleneckObj = RedisHelper.Hash.HashGetAsync<BottleneckDTO>(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString()).Result;
                var roleObj= RedisHelper.Hash.HashGetAsync<Role>(RedisKeyDefine._RolePostfix, role.RoleID.ToString()).Result;
                var roleAssetsObj = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RolePostfix, role.RoleID.ToString()).Result;

                var rolestatusObj = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, role.RoleID.ToString()).Result;

                if (bottleneckObj!=null&& roleObj!=null&& rolestatusObj!=null&&roleAssetsObj!=null)
                {
                    if (roleDict.TryGetValue(roleObj.RoleLevel, out var roleLevelData))
                    {
                        roleObj.RoleLevel = roleLevelData.NextLevelID;
                        //RoleStatusDTO status = new RoleStatusDTO();
                        //status.RoleID = role.RoleID;
                        var status = RoleStatusAlgorithm(roleObj.RoleID, null, null, null, null, null, null, roleObj.RoleLevel);
                        if (status != null)
                        {
                            var obj = StatusVerify(rolestatusObj, status);
                            roleAssetsObj.SpiritStonesLow += 1000000;

                            await NHibernateQuerier.UpdateAsync(roleAssetsObj);
                            await NHibernateQuerier.UpdateAsync(obj);
                            await NHibernateQuerier.UpdateAsync(roleObj);

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, role.RoleID.ToString(), obj);
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RolePostfix, role.RoleID.ToString(), roleObj);
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix, role.RoleID.ToString(), roleAssetsObj);

                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.RoleBottleneck, bottleneckObj);
                            dict.Add((byte)ParameterCode.RoleStatus, obj);
                            dict.Add((byte)ParameterCode.Role, roleObj);
                            dict.Add((byte)ParameterCode.RoleAssets, roleAssetsObj);
                            ResultSuccseS2C(role.RoleID, PracticeOpcode.UpdateBottleneck, dict);
                        }
                    }
                }
            }
  
        }

        /// <summary>
        /// 突破瓶颈
        /// </summary>
        /// <param name="role"></param>
        private async void UpdateBottleneckAsync2(RoleDTO role)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, RoleLevelData>>(out var roleDict);
            var bottleneckExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString()).Result;
            var roleExits = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, role.RoleID.ToString()).Result;
            if (bottleneckExist&& roleExits)
            {
                var bottleneckObj = RedisHelper.Hash.HashGetAsync<Bottleneck>(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString()).Result;
                var roleObj = RedisHelper.Hash.HashGetAsync<Role>(RedisKeyDefine._RolePostfix, role.RoleID.ToString()).Result;
                var rolestatusObj = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, role.RoleID.ToString()).Result;
        
                if (bottleneckObj!=null&& roleObj!=null)
                {
                    if (bottleneckObj.BreakThroughVauleNow == bottleneckObj.BreakThroughVauleMax)
                    {
                        if (roleDict.TryGetValue(roleObj.RoleLevel,out var LevelData))
                        {
                            if (LevelData.IsFinalLevel == 0)
                            {
                                bottleneckObj.IsBottleneck = false;
                                roleObj.RoleLevel = LevelData.NextLevelID;
                                var status = RoleStatusAlgorithm(roleObj.RoleID, null, null, null, null, null, null, roleObj.RoleLevel);
                                if (status != null)
                                {
                                    var obj = StatusVerify(rolestatusObj, status);

                                    await NHibernateQuerier.UpdateAsync(obj);
                                    await NHibernateQuerier.UpdateAsync(roleObj);
                                    await NHibernateQuerier.UpdateAsync(bottleneckObj);

                                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, role.RoleID.ToString(), obj);
                                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RolePostfix, role.RoleID.ToString(), roleObj);
                                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString(), bottleneckObj);

                                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                                    dict.Add((byte)ParameterCode.RoleBottleneck, bottleneckObj);
                                    dict.Add((byte)ParameterCode.RoleStatus, obj);
                                    dict.Add((byte)ParameterCode.Role, roleObj);
                                    ResultSuccseS2C(role.RoleID, PracticeOpcode.UpdateBottleneck, dict);
                                }
                            }
                            else
                            {
                                GameEntry.DataManager.TryGetValue<Dictionary<int, BottleneckData>>(out var bottleneckData);
                                GameEntry.DataManager.TryGetValue<Dictionary<int, DemonData>>(out var demonData);


                                int demonIndex = GetDemonPercent(demonData[roleObj.RoleLevel], bottleneckObj.CraryVaule);
                                if (GetPercent(demonData[roleObj.RoleLevel].Trigger_Chance[demonIndex] / (float)100))
                                {
                                    bottleneckObj.IsDemon = true;
                                    bottleneckObj.DemonID = demonData[roleObj.RoleLevel].Demon_ID[demonIndex];
                                }
                                bottleneckObj.IsThunder = true;
                                bottleneckObj.ThunderRound = bottleneckData[roleObj.RoleLevel].Thunder_Round;//获取天劫回合数
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 触发心魔战斗
        /// </summary>
        private void DemonicBattle(RoleDTO role)
        {
            var bottleneckExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString()).Result;
            if (bottleneckExist)
            {
                var bottleneckObj = RedisHelper.Hash.HashGetAsync<Bottleneck>(RedisKeyDefine._RoleBottleneckPostfix, role.RoleID.ToString()).Result;
                if (bottleneckObj!=null)
                {
                    if (!bottleneckObj.IsBottleneck&& bottleneckObj.IsDemon)
                    {
                        GameEntry.BattleRoomManager.CreateRoom(role.RoleID, new List<int>() { bottleneckObj.DemonID });
                    }
                }
            }
        }
    }
}
