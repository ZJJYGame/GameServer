using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using AscensionProtocol;
using AscensionServer.Model;
using RedisDotNet;
using Cosmos;
namespace AscensionServer
{
    enum AtttributeType
    {
        HP=1,
        AttackPhysical=2,
        AttackPower=3,
        DefendPhysical=4,
        DefendPower=5,
        AttackSpeed=6,
        MP=7,
    }

    public partial class SecondaryJobManager
    {
        #region Redis 模塊     
        /// <summary>
        /// 学习新配方
        /// </summary>
        /// <param name="secondaryJobDTO"></param>
        /// <param name="nHCriteriarole"></param>
         async void UpdateAlchemyS2C(int roleID, int UseItemID)
        {
          var formulaExist=  GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaDrugData>>(out var formulaDataDict);
            if (!formulaExist)
            {
                Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求y");
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }
            var roleexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            if (ringServer== null||!roleexist)
            {
                Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求z");
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }
            var role = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            if (InventoryManager.VerifyIsExist(UseItemID,1 , ringServer.RingIdArray)&& role != null)
            {
                var tempid = Utility.Converter.RetainInt32(UseItemID, 5);
                var alchemyExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlchemyPerfix, roleID.ToString()).Result;
                if (alchemyExist)
                {
                    var alchemy = RedisHelper.Hash.HashGetAsync<AlchemyDTO>(RedisKeyDefine._AlchemyPerfix, roleID.ToString()).Result;
                    if (alchemy != null)
                    {
                        if (formulaDataDict.TryGetValue(tempid, out var formula))
                        {
                            Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求1");
                            if (formula.NeedJobLevel > alchemy.JobLevel)
                            {
                                Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求2");
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                                return;
                            }

                            #region 等级判断
                            //if (formula.FormulaLevel > role.RoleLevel)
                            //{
                            //    RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                            //    return;
                            //}
                            #endregion
                            Utility.Debug.LogInfo("YZQ收到的副职业学习配方配方"+tempid+"请求");
                            Utility.Debug.LogInfo("YZQ当前副职业已学配方为" + tempid + "请求");
                            if (!alchemy.Recipe_Array.Contains(tempid))
                            {
                               
                                alchemy.Recipe_Array.Add(tempid);
                                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                                dict.Add((byte)ParameterCode.JobAlchemy, alchemy);
                                RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, dict);
                                InventoryManager.Remove(roleID, UseItemID);
                                await NHibernateQuerier.SaveOrUpdateAsync(ChangeDataType(alchemy));
                                await RedisHelper.Hash.HashSetAsync<AlchemyDTO>(RedisKeyDefine._AlchemyPostfix, roleID.ToString(), alchemy);
                            }
                            else
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                        }
                        else
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                    }
                    else
                        UpdateAlchemyMySql(roleID, UseItemID, nHCriteria);
                }
                else
                    UpdateAlchemyMySql(roleID, UseItemID, nHCriteria);
            }
            else {
                Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求q");
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                
            }

        }
        /// <summary>
        /// 合成配方
        /// </summary>
        /// <param name="secondaryJobDTO"></param>
        /// <param name="nHCriteriarole"></param>
        void CompoundAlchemyS2C(int roleID, int UseItemID)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<byte, SecondaryJobData>>(out var secondary);
            var alchemyExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlchemyPerfix, roleID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
            var assestExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var rolering = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            Dictionary<byte, object> dict = new Dictionary<byte, object>();
            if (alchemyExist && assestExist && roleExist&& rolering!=null)
            {
                var alchemy = RedisHelper.Hash.HashGetAsync<AlchemyDTO>(RedisKeyDefine._AlchemyPerfix, roleID.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleStatusDTO>(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
                var assest = RedisHelper.Hash.HashGetAsync<RoleAssetsDTO>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                if (alchemy != null && role != null && assest != null)
                {
                    GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaDrugData>>(out var formulaDataDict);
                    if (alchemy.Recipe_Array.Contains(UseItemID))
                    {
                        formulaDataDict.TryGetValue(UseItemID, out var formulaData);
                        for (int i = 0; i < formulaData.NeedItemArray.Count; i++)
                        {
                            if (!InventoryManager.VerifyIsExist(formulaData.NeedItemArray[i], formulaData.NeedItemNumber[i], rolering.RingIdArray))
                            {
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy);
                                return;
                            }
                        }
                        if (formulaData.NeedMoney > assest.SpiritStonesLow || formulaData.NeedVitality > role.Vitality)
                        {
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy);
                            return;
                        }
                        var randNum = drollRandom.Next(1, 101);
                        Utility.Debug.LogError("随机出的数据为"+ randNum+"成功率为"+ formulaData.SuccessRate);
                        if (randNum > formulaData.SuccessRate)
                        {
                            RoleStatusCompoundFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy, default);
                            dict.Add((byte)ParameterCode.JobAlchemy, alchemy);
                            dict.Add((byte)ParameterCode.RoleAssets, assest);
                            dict.Add((byte)ParameterCode.RoleStatus, role);
                            RoleStatusCompoundFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy, dict);
                            return;
                        }


                        alchemy.JobLevelExp += formulaData.MasteryValue;

                        secondary.TryGetValue((byte)FormulaDrugType.Alchemy, out var secondaryJob);
                        if (secondaryJob.SecondaryJobLevel.Contains(alchemy.JobLevel))
                        {
                            var index = secondaryJob.SecondaryJobLevel.FindIndex(f => f == alchemy.JobLevel);
                            if (alchemy.JobLevel < 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= alchemy.JobLevelExp)
                                {
                                    alchemy.JobLevelExp -= secondaryJob.SecondaryJobExp[index];
                                    alchemy.JobLevel += 1;
                                }
                            }
                            else if (alchemy.JobLevel == 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= alchemy.JobLevelExp)
                                {
                                    alchemy.JobLevelExp = secondaryJob.SecondaryJobExp[index];
                                }
                            }
                        }

                        role.Vitality -= formulaData.NeedVitality;
                        assest.SpiritStonesLow -= formulaData.NeedMoney;
                        InventoryManager.AddNewItem(roleID, formulaData.ItemID, 1);
                        dict.Add((byte)ParameterCode.JobAlchemy, alchemy);
                        dict.Add((byte)ParameterCode.RoleAssets, assest);
                        dict.Add((byte)ParameterCode.RoleStatus, role);
                        RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.CompoundAlchemy, dict);
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy);
                    }
                }
                else
                    CompoundAlchemyMySql(roleID, UseItemID);
            }
            else
                CompoundAlchemyMySql(roleID, UseItemID);
        }
        #endregion

        #region MysSql模塊
        /// <summary>
        /// 學習新配方
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="UseItemID"></param>
        /// <param name="nHCriteria"></param>
        async void UpdateAlchemyMySql(int roleID, int UseItemID, NHCriteria nHCriteria)
        {
            Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求5");
            var alchemy = NHibernateQuerier.CriteriaSelect<Alchemy>(nHCriteria);
            if (alchemy != null)
            {
                var tempid = Utility.Converter.RetainInt32(UseItemID, 5);
                var recipe = Utility.Json.ToObject<List<int>>(alchemy.Recipe_Array);
                if (!recipe.Contains(tempid))
                {
                    recipe.Add(tempid);
                    alchemy.Recipe_Array = Utility.Json.ToJson(recipe);
                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.JobAlchemy, ChangeDataType(alchemy));
                    RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, dict);
                    InventoryManager.Remove(roleID, UseItemID);
                    await NHibernateQuerier.UpdateAsync(alchemy);
                    await RedisHelper.Hash.HashSetAsync<AlchemyDTO>(RedisKeyDefine._AlchemyPostfix, roleID.ToString(), ChangeDataType(alchemy));
                }
                else
                    RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
            }
            else
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
        }
        /// <summary>
        /// 煉丹
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="UseItemID"></param>
        async void CompoundAlchemyMySql(int roleID, int UseItemID)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<byte, SecondaryJobData>>(out var secondary);
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var alchemy = NHibernateQuerier.CriteriaSelect<Alchemy>(nHCriteria);
            var rolering = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var role = NHibernateQuerier.CriteriaSelect<RoleStatus>(nHCriteria);
            var assest = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteria);
            Dictionary<byte, object> dict = new Dictionary<byte, object>();
            if (alchemy != null&& role!=null&& assest!=null&& rolering!=null)
            {
                Utility.Debug.LogInfo("YZQ开始合成丹药" );
                GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaDrugData>>(out var formulaDataDict);
                var recipe = Utility.Json.ToObject<List<int>>(alchemy.Recipe_Array);
                if (recipe.Contains(UseItemID))
                {
                    if (formulaDataDict.TryGetValue(UseItemID, out var formulaData))
                    {
                        for (int i = 0; i < formulaData.NeedItemArray.Count; i++)
                        {
                            var result = !InventoryManager.VerifyIsExist(formulaData.NeedItemArray[i], formulaData.NeedItemNumber[i], rolering.RingIdArray);
                            if (result)
                            {
                                Utility.Debug.LogInfo("YZQ开始合成丹药1");
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy);
                                return;
                            }
                        }
                        if (formulaData.NeedMoney > assest.SpiritStonesLow && formulaData.NeedVitality > role.Vitality)
                        {
                            Utility.Debug.LogInfo("YZQ开始合成丹药2");
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy);
                            return;
                        }
                        var num = Utility.Algorithm.RandomRange(0, 101);
                        if (num > formulaData.SuccessRate)
                        {
                            Utility.Debug.LogInfo("3YZQ开始合成丹药" + num + ">>>>" + formulaData.SuccessRate);
                            dict.Add((byte)ParameterCode.JobAlchemy, alchemy);
                            dict.Add((byte)ParameterCode.RoleAssets, assest);
                            dict.Add((byte)ParameterCode.RoleStatus, role);
                            RoleStatusCompoundFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy, dict);
                            //鍛造失敗
                            return;
                        }

                        alchemy.JobLevelExp += formulaData.MasteryValue;
                        secondary.TryGetValue((byte)FormulaDrugType.Alchemy, out var secondaryJob);
                        if (secondaryJob.SecondaryJobLevel.Contains(alchemy.JobLevel))
                        {
                            var index = secondaryJob.SecondaryJobLevel.FindIndex(f => f == alchemy.JobLevel);
                            if (alchemy.JobLevel < 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= alchemy.JobLevelExp)
                                {
                                    alchemy.JobLevelExp -= secondaryJob.SecondaryJobExp[index];
                                    alchemy.JobLevel += 1;
                                }
                            }
                            else if (alchemy.JobLevel == 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= alchemy.JobLevelExp)
                                {
                                    alchemy.JobLevelExp = secondaryJob.SecondaryJobExp[index];
                                }
                            }
                        }
                        role.Vitality -= formulaData.NeedVitality;
                        assest.SpiritStonesLow -= formulaData.NeedMoney;
                        InventoryManager.AddNewItem(roleID, formulaData.ItemID, 1);

                        dict.Add((byte)ParameterCode.JobAlchemy, ChangeDataType(alchemy));
                        dict.Add((byte)ParameterCode.RoleAssets, assest);
                        dict.Add((byte)ParameterCode.RoleStatus, role);
                        RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.CompoundAlchemy, dict);
                    }
                    else { Utility.Debug.LogInfo("YZQ开始合成丹药失败"); RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy); }
                       
                }
                else
                { Utility.Debug.LogInfo("YZQ开始合成丹药失败2"); RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy); }
            }
            else
            { Utility.Debug.LogInfo("YZQ开始合成丹药失败3"); RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundAlchemy); }
        }
        #endregion

        AlchemyDTO ChangeDataType(Alchemy alchemy)
        {
            AlchemyDTO alchemyDTO = new AlchemyDTO();
            alchemyDTO.RoleID = alchemy.RoleID;
            alchemyDTO.JobLevel = alchemy.JobLevel;
            alchemyDTO.JobLevelExp = alchemy.JobLevelExp;
            alchemyDTO.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(alchemy.Recipe_Array);
            return alchemyDTO;
        }

        Alchemy ChangeDataType(AlchemyDTO alchemyDTO)
        {
            Alchemy alchemy = new Alchemy();
            alchemy.RoleID = alchemyDTO.RoleID;
            alchemy.JobLevel = alchemyDTO.JobLevel;
            alchemy.JobLevelExp = alchemyDTO.JobLevelExp;
            alchemy.Recipe_Array = Utility.Json.ToJson(alchemyDTO.Recipe_Array);
            return alchemy;
        }
    }
}


