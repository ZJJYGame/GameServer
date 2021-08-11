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
    public partial class SecondaryJobManager
    {
       /// <summary>
       /// 合成傀儡部件
       /// </summary>
       /// <param name="roleid"></param>
       /// <param name="useItemID"></param>
        async  void CompoundPuppetS2C(int roleID, int useItemID)
        {
            var puppetExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetPerfix, roleID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
            var assestExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            var puppetUnitExist= RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString()).Result;
            NHCriteria nHCriteria = CosmosEntry.ReferencePoolManager.Spawn<NHCriteria>().SetValue("RoleID", roleID);
            GameEntry.DataManager.TryGetValue<Dictionary<byte, SecondaryJobData>>(out var secondary);
            var rolering = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            if (puppetExist && roleExist && assestExist&& puppetUnitExist)
            {
                var puppet = RedisHelper.Hash.HashGetAsync<PuppetDTO>(RedisKeyDefine._PuppetPerfix, roleID.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
                var assest = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                var puppetUnit = RedisHelper.Hash.HashGetAsync<PuppetUnitDTO>(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString()).Result;
                if (puppet != null && role != null && assest != null && puppetUnit!=null)
                {
                    var unitid = 0;//部件唯一id
                    GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaPuppetData>>(out var formulaDataDict);//TODO跟换数据类型
                    if (puppet.Recipe_Array.Contains(useItemID))
                    {
                        formulaDataDict.TryGetValue(useItemID, out var formulaData);
                        for (int i = 0; i < formulaData.NeedItemArray.Count; i++)
                        {
                            Utility.Debug.LogInfo("YZQ合成傀儡部件id"+ formulaData.NeedItemArray[i]+"数量"+ formulaData.NeedItemNumber[i]);
                            if (formulaData.NeedItemArray[i]!=0)
                            {
                                if (!InventoryManager.VerifyIsExist(formulaData.NeedItemArray[i], formulaData.NeedItemNumber[i], rolering.RingIdArray))
                                {
                                    Utility.Debug.LogInfo("YZQ收到的副职业请求1");
                                    RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundPuppet);
                                    return;
                                }
                            }
                        }
                        //if (formulaData.NeedMoney > assest.SpiritStonesLow || formulaData.NeedVitality > role.Vitality)
                        //{
                        //    Utility.Debug.LogInfo("YZQ收到的副职人物属性"+Utility.Json.ToJson(role));
                        //    Utility.Debug.LogInfo("YZQ收到的副职业请求2灵石"+ assest.SpiritStonesLow+"活力"+ role.Vitality);
                        //    RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundPuppet);
                        //    return;
                        //}
                        var randNum = drollRandom.Next(1, 101);
                        Utility.Debug.LogError("随机出的数据为" + randNum + "成功率为" + formulaData.SuccessRate);
                        if (randNum > formulaData.SuccessRate)
                        {
                                Utility.Debug.LogInfo("YZQ收到的副职业请求3");
                            RoleStatusCompoundFailS2C(roleID, SecondaryJobOpCode.CompoundPuppet, default);
                                //鍛造失敗
                                return;
                            }
                        
                        puppet.JobLevelExp += formulaData.MasteryValue;
                        secondary.TryGetValue((byte)FormulaDrugType.Puppet,out var secondaryJob);
                        if (secondaryJob.SecondaryJobLevel.Contains(puppet.JobLevel))
                        {
                           var index =secondaryJob.SecondaryJobLevel.FindIndex(f=>f== puppet.JobLevel);
                            if (puppet.JobLevel<5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= puppet.JobLevelExp)
                                {
                                    puppet.JobLevelExp -= secondaryJob.SecondaryJobExp[index];
                                    puppet.JobLevel += 1;
                                }
                            }
                            else if (puppet.JobLevel == 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= puppet.JobLevelExp)
                                {
                                    puppet.JobLevelExp = secondaryJob.SecondaryJobExp[index];
                                }
                            }
                        }
                        // role.Vitality -= formulaData.NeedVitality;
                        assest.SpiritStonesLow -= formulaData.NeedMoney;

                        var puppetObj = PuppetStatusAlgorithm(formulaData.ItemID);
                        if (puppetObj != null)
                        {
                            puppetObj.UnitLevel = formulaData.FormulaLevel;
                            puppetObj.UnitPart = formulaData.SyntheticType;
                            var indexExist = puppetUnit.UnitIndesDict.TryGetValue(formulaData.ItemID, out int id);
                            if (indexExist)
                            {
                                Utility.Debug.LogInfo("YZQ收到的副职业请求的唯一ID为" + unitid);
                                puppetUnit.UnitIndesDict[formulaData.ItemID] = id + 1;
                                unitid = Convert.ToInt32(formulaData.ItemID + "" + (id + 1));
                                puppetUnit.PuppetUnitInfoDict.Add(unitid, puppetObj);

                            }
                            else
                            {
                                puppetUnit.UnitIndesDict.Add(formulaData.ItemID, 1);
                                unitid = Convert.ToInt32(formulaData.ItemID + "" + 1);
                                puppetUnit.PuppetUnitInfoDict.Add(unitid, puppetObj);

                            }
                            //TODO发送客户端
                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.GetPuppetUnit, puppetUnit);
                            dict.Add((byte)ParameterCode.RoleAssets, assest);
                            dict.Add((byte)ParameterCode.RoleStatus, role);
                            dict.Add((byte)ParameterCode.JobPuppet, puppet);
                            RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.CompoundPuppet, dict);
                            InventoryManager.AddNewItem(roleID, unitid, 1);

                            #region 更新到数据库
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString(), puppetUnit);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(puppetUnit));

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), assest);
                            await NHibernateQuerier.UpdateAsync(assest);

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString(), role);
                            await NHibernateQuerier.UpdateAsync(role);

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PuppetPerfix, roleID.ToString(), puppet);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(puppet));
                            #endregion
                        }
                        else
                        {
                            Utility.Debug.LogInfo("YZQ收到的副职业请求4");
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundPuppet);
                            return;
                        }
                    }
                   
                }
            }
        }
        /// <summary>
        /// 组装傀儡
        /// </summary>
        async  void AssemblePuppetS2C(int roleID, int useItemID,List<int > unit)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, PuppetAssembleData>>(out var unitparameter);
            var result = unitparameter.TryGetValue(useItemID,out var data);
            if (!result)
            {
                return;
            }
            var puppetExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetPerfix, roleID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
            var assestExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            var puppetUnitExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString()).Result;
            var rolepuppetExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePuppetPerfix, roleID.ToString()).Result;
            NHCriteria nHCriteria = CosmosEntry.ReferencePoolManager.Spawn<NHCriteria>().SetValue("RoleID", roleID);
            var rolering = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            if (puppetExist&& roleExist&& assestExist&& puppetUnitExist&& rolepuppetExist)
            {
                var puppet = RedisHelper.Hash.HashGetAsync<PuppetDTO>(RedisKeyDefine._PuppetPerfix, roleID.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
                var assest = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                var puppetUnit = RedisHelper.Hash.HashGetAsync<PuppetUnitDTO>(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString()).Result;
                var rolepuppet = RedisHelper.Hash.HashGetAsync<RolePuppetDTO>(RedisKeyDefine._RolePuppetPerfix,roleID.ToString()).Result;
                if (puppet!=null&& role!=null&& assest!=null&& puppetUnit!=null&& rolepuppet!=null)
                {
                    for (int i = 0; i < unit.Count; i++)
                    {
                        if (!InventoryManager.VerifyIsExist(unit[i], 1, rolering.RingIdArray))
                        {
                            Utility.Debug.LogInfo("YZQ收到的副职业请求1");
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.AssemblePuppet);
                            return;
                        }
                        if (data.NeedMoney > assest.SpiritStonesLow || data.NeedVitality > role.Vitality)
                        {
                         
                            Utility.Debug.LogInfo("YZQ收到的副职业请求2灵石" + assest.SpiritStonesLow + "活力" + role.Vitality);
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.AssemblePuppet);
                            return;
                        }
                    }
                    var puppetObj = PuppetIndividualAlgorithm(puppetUnit, unit);
                    puppetObj.PuppetID = useItemID;
                    if (puppetObj != null)
                    {
                        role.Vitality -= data.NeedVitality;
                        assest.SpiritStonesLow -= data.NeedMoney;

                        //for (int i = 0; i < unit.Count; i++)
                        //{
                        //    InventoryManager.Remove(roleID, unit[i]);
                        //}


                        #region 更新到数据库
                       var puppetTemp=  NHibernateQuerier.Insert(ChangeDataType(puppetObj));
                        rolepuppet.PuppetDict.Add(puppetTemp.ID, data.PuppetID);
                        puppetObj.ID = puppetTemp.ID;
                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PuppetIndividualPerfix, puppetTemp.ID.ToString(), puppetObj);

                        Dictionary<byte, object> dict = new Dictionary<byte, object>();
                        dict.Add((byte)ParameterCode.RoleAssets, assest);
                        dict.Add((byte)ParameterCode.RoleStatus, role);
                        dict.Add((byte)ParameterCode.JobPuppet, puppet);
                        dict.Add((byte)ParameterCode.GetPuppetIndividual, puppetObj);
                        dict.Add((byte)ParameterCode.RolePuppet, rolepuppet);
                        RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.AssemblePuppet, dict);

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RolePuppetPerfix, roleID.ToString(), rolepuppet);
                        Utility.Debug.LogInfo("YZQ收到的副职人物属性" + Utility.Json.ToJson(ChangeDataType(rolepuppet)));
                        await NHibernateQuerier.SaveOrUpdateAsync(ChangeDataType(rolepuppet));

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString(), puppetUnit);
                        await NHibernateQuerier.SaveOrUpdateAsync(ChangeDataType(puppetUnit));

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), assest);
                        await NHibernateQuerier.UpdateAsync(assest);

                        await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString(), role);
                        await NHibernateQuerier.SaveOrUpdateAsync(role);
     
                        #endregion
                    }
                    else
                        RoleStatusFailS2C(roleID, SecondaryJobOpCode.AssemblePuppet);
                }
            }
        }

        void GetPuppetUnitS2C(int roleID)
        {
            var puppetUnitExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString()).Result;
            if (puppetUnitExist)
            {
                var puppetUnit = RedisHelper.Hash.HashGetAsync<PuppetUnitDTO>(RedisKeyDefine._PuppetUnitPerfix, roleID.ToString()).Result;
                if (puppetUnit!=null)
                {
                    RoleStatusSuccessS2C(roleID,SecondaryJobOpCode.GetPuppetUnit, puppetUnit);
                }
            }
        }
        /// <summary>
        /// 修復傀儡
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="id"></param>
        async void RepairPuppetS2C(int roleid ,int id )
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, RepairPuppetData>>(out var repairDict);
            var assestExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleid.ToString()).Result;
            var puppetIndividualExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetIndividualPerfix, id.ToString()).Result; NHCriteria nHCriteria = CosmosEntry.ReferencePoolManager.Spawn<NHCriteria>().SetValue("RoleID", roleid);
            var rolering = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            if (assestExist&& puppetIndividualExist)
            {
                var assestObj = RedisHelper.Hash.HashGetAsync<RoleAssets>(RedisKeyDefine._RoleAssetsPerfix, roleid.ToString()).Result;
                var puppetIndividualObj = RedisHelper.Hash.HashGetAsync<PuppetIndividualDTO>(RedisKeyDefine._PuppetIndividualPerfix, id.ToString()).Result;
                var expend = 0;
                if (assestObj!=null&& puppetIndividualObj!=null)
                {
                    var result = repairDict.TryGetValue(puppetIndividualObj.PuppetID, out var puppetData);
                    for (int i = 0; i < puppetData.RepairMaterials.Count; i++)
                    {
                        if (!InventoryManager.VerifyIsExist(puppetData.RepairMaterials[i], puppetData.MaterialsNumbers[i], rolering.RingIdArray))
                        {
                            RoleStatusFailS2C(roleid,SecondaryJobOpCode.RepairPuppet);
                            return;
                        }
                    }

                    expend = (puppetIndividualObj.HP + puppetIndividualObj.MP + puppetIndividualObj.AttackPower + puppetIndividualObj.AttackPhysical + puppetIndividualObj.AttackSpeed + puppetIndividualObj.DefendPhysical + puppetIndividualObj.DefendPower) * 2 * (puppetIndividualObj.PuppetDurableMax - puppetIndividualObj.PuppetDurable) / puppetIndividualObj.PuppetDurableMax;
                    Utility.Debug.LogInfo("角色傀儡修复的花费发送了" + expend);

                    if (assestObj.SpiritStonesLow< expend)
                    {
                        RoleStatusFailS2C(roleid, SecondaryJobOpCode.RepairPuppet);
                        return;
                    }

                    assestObj.SpiritStonesLow -= expend;
                    puppetIndividualObj.PuppetDurable = puppetIndividualObj.PuppetDurableMax;

                    Dictionary<byte, object> dict = new Dictionary<byte, object>();
                    dict.Add((byte)ParameterCode.GetPuppetIndividual, puppetIndividualObj);
                    dict.Add((byte)ParameterCode.RoleAssets, assestObj);
                    RoleStatusSuccessS2C(roleid,SecondaryJobOpCode.RepairPuppet,dict);

                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix,roleid.ToString(), assestObj);
                    await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._PuppetIndividualPerfix, id.ToString(), puppetIndividualObj);

                   await NHibernateQuerier.UpdateAsync(assestObj);
                    await NHibernateQuerier.UpdateAsync(ChangeDataType(puppetIndividualObj));
                }
            }

        }
        /// <summary>
        /// 学习傀儡配方
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="id"></param>
        async void UpdatePuppetS2C(int roleid, int id)
        {
            var formulaExist = GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaPuppetData>>(out var formulaDataDict);
            if (!formulaExist)
            {
                Utility.Debug.LogInfo("YZQ收到的副职业学习傀儡配方请求>>>1");
                RoleStatusFailS2C(roleid, SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }
            NHCriteria nHCriteria = CosmosEntry.ReferencePoolManager.Spawn<NHCriteria>().SetValue("RoleID", roleid);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var roleexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleid.ToString()).Result;
            if (ringServer == null|| !roleexist)
            {
                Utility.Debug.LogInfo("YZQ收到的副职业学习傀儡配方请求>>>2");
                RoleStatusFailS2C(roleid, SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }
            var role = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix, roleid.ToString()).Result;
            if (InventoryManager.VerifyIsExist(id, 1, ringServer.RingIdArray)&& role!=null)
            {
                var tempid = Utility.Converter.RetainInt32(id, 5);
                var puppetExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetPerfix, roleid.ToString()).Result;
                if (puppetExist)
                {
                    var puppet = RedisHelper.Hash.HashGetAsync<PuppetDTO>(RedisKeyDefine._PuppetPerfix, roleid.ToString()).Result;
                    if (puppet!=null)
                    {
                        if (formulaDataDict.TryGetValue(tempid, out var formula))
                        {
                            if (formula.NeedJobLevel > puppet.JobLevel)
                            {
                                Utility.Debug.LogInfo("YZQ收到的副职业学习傀儡配方请求>>>3");
                                RoleStatusFailS2C(roleid, SecondaryJobOpCode.StudySecondaryJobStatus);
                                return;
                            }
                            #region 等级判断
                            //if (formula.FormulaLevel > role.RoleLevel)
                            //{
                            //    Utility.Debug.LogInfo("YZQ收到的副职业学习傀儡配方请求>>>4");
                            //    RoleStatusFailS2C(roleid, SecondaryJobOpCode.StudySecondaryJobStatus);
                            //    return;
                            //}
                            #endregion

                            if (!puppet.Recipe_Array.Contains(tempid))
                            {
                                puppet.Recipe_Array.Add(tempid);
                                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                                dict.Add((byte)ParameterCode.JobPuppet, puppet);

                                RoleStatusSuccessS2C(roleid, SecondaryJobOpCode.StudySecondaryJobStatus, dict);
                                InventoryManager.Remove(roleid, id);
                                puppet.RoleID = roleid;
                                Utility.Debug.LogError("学习完傀儡配方1" + Utility.Json.ToJson(puppet));
                                await NHibernateQuerier.UpdateAsync(ChangeDataType(puppet));
                                Utility.Debug.LogError("学习完傀儡配方2" + Utility.Json.ToJson(puppet));
                                await RedisHelper.Hash.HashSetAsync<PuppetDTO>(RedisKeyDefine._PuppetPerfix, roleid.ToString(), puppet);

                            }
                            else
                            {
                                Utility.Debug.LogInfo("YZQ收到的副职业学习傀儡配方请求>>>1");
                                RoleStatusFailS2C(roleid, SecondaryJobOpCode.StudySecondaryJobStatus);
                            }
                               
                        }
                    }
                }
            }
            }
        /// <summary>
        /// 傀儡部件属性值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PuppetUnitInfo PuppetStatusAlgorithm(int id)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, PuppetUnitParameter>>(out var unitparameter);
            var result = unitparameter.TryGetValue(id, out var parameter);
            if (result)
            {
                PuppetUnitInfo puppet = new PuppetUnitInfo();
                for (int i = 0; i < parameter.PuppetAttributeMax.Count; i++)
                {
                    puppet.PuppetAttribute.Add(Utility.Algorithm.CreateRandomInt(parameter.PuppetAttributeMin[i], parameter.PuppetAttributeMax[i]));
                }
                puppet.WeaponSkill.Add(parameter.FixedSkill);
                if (Utility.Algorithm.CreateRandomInt(0,100)<= parameter.PuppetSkillProbability)
                {
                    puppet.WeaponSkill.Add(parameter.PuppetSkillpoor[Utility.Algorithm.CreateRandomInt(0, parameter.PuppetSkillpoor.Count)]);
                }

                if (Utility.Algorithm.CreateRandomInt(0, 100)<= parameter.AdditionalAttributeProbability)
                {
                    puppet.AffixType = Utility.Algorithm.CreateRandomInt(0, parameter.AdditionalMaxPercentage.Count );
                    puppet.AffixAddition = Utility.Algorithm.CreateRandomInt(0, parameter.AdditionalMaxPercentage[puppet.AffixType] );
                }
                puppet.PuppetDurable = parameter.PuppetDurable;
                return puppet;
            }
            else
                return null;

        }

        /// <summary>
        /// 傀儡组装属性值
        /// </summary>
        /// <returns></returns>
        PuppetIndividualDTO PuppetIndividualAlgorithm(PuppetUnitDTO unitDTO, List<int> unit)
        {
            PuppetIndividualDTO individualDTO = new PuppetIndividualDTO();
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 0; i < unit.Count; i++)
            {
              var result=  unitDTO.PuppetUnitInfoDict.TryGetValue(unit[i],out var unitInfo);
                if (!result)
                {
                    return null;
                }
                individualDTO.HP += unitInfo.PuppetAttribute[0];
                individualDTO.AttackPhysical += unitInfo.PuppetAttribute[1];
                individualDTO.AttackPower += unitInfo.PuppetAttribute[2];
                individualDTO.DefendPhysical += unitInfo.PuppetAttribute[3];
                individualDTO.DefendPower += unitInfo.PuppetAttribute[4];
                individualDTO.AttackSpeed += unitInfo.PuppetAttribute[5];
                individualDTO.MP += unitInfo.PuppetAttribute[6];

                individualDTO.PuppetDurable += unitInfo.PuppetDurable;

                for (int j = 0; j < unitInfo.WeaponSkill.Count; j++)
                {
                    if (!individualDTO.Skills.Contains(unitInfo.WeaponSkill[j]))
                    {
                        individualDTO.Skills.Add(unitInfo.WeaponSkill[j]);
                    }
                }

                if (dict.ContainsKey(unitInfo.AffixType))
                {
                    dict[unitInfo.AffixType] += unitInfo.AffixAddition;
                }
                else
                {
                    dict.Add(unitInfo.AffixType, unitInfo.AffixAddition);
                }

                #region 属性值
                foreach (var item in dict)
                {
                    switch ((AffixType)item.Key)
                    {
                        case AffixType.HP:
                            individualDTO.HP *= (item.Value + 100) / 100;
                            break;
                        case AffixType.AttackPhysical:
                            individualDTO.AttackPhysical *= (100 + item.Value) / 100;
                            break;
                        case AffixType.DefendPhysical:
                            individualDTO.AttackPower *= (100 + item.Value) / 100;
                            break;
                        case AffixType.AttackPower:
                            individualDTO.DefendPhysical *= (100 + item.Value) / 100;
                            break;
                        case AffixType.DefendPower:
                            individualDTO.DefendPower *= (100 + item.Value) / 100;
                            break;
                        case AffixType.AttackSpeed:
                            individualDTO.AttackSpeed *= (100 + item.Value) / 100;
                            break;
                        default:
                            break;
                    }
                }

                #endregion

            }

            individualDTO.PuppetDurableMax += individualDTO.PuppetDurable;
            return individualDTO;
        }

        PuppetDTO ChangeDataType(Puppet puppet)
        {
            PuppetDTO puppetDTO = new PuppetDTO();
            puppetDTO.RoleID = puppet.RoleID;
            puppetDTO.Recipe_Array = Utility.Json.ToObject<HashSet<int>>(puppet.Recipe_Array);
            puppetDTO.JobLevel = puppet.JobLevel;
            puppetDTO.JobLevelExp = puppet.JobLevelExp;
            return puppetDTO;
        }

        Puppet ChangeDataType(PuppetDTO puppetDTO)
        {
            Puppet puppet = new Puppet();
            puppet.RoleID = puppetDTO.RoleID;
            puppet.Recipe_Array = Utility.Json.ToJson(puppetDTO.Recipe_Array);
            puppet.JobLevel = puppetDTO.JobLevel;
            puppet.JobLevelExp = puppetDTO.JobLevelExp;
            return puppet;
        }

        PuppetUnit ChangeDataType(PuppetUnitDTO unitDTO)
        {
            PuppetUnit puppet = new PuppetUnit();
            puppet.RoleID = unitDTO.RoleID;
            puppet.PuppetUnitInfoDict = Utility.Json.ToJson(unitDTO.PuppetUnitInfoDict);
            puppet.UnitIndesDict = Utility.Json.ToJson(unitDTO.UnitIndesDict);
            return puppet;
        }

        RolePuppet ChangeDataType(RolePuppetDTO puppetDTO)
        {
            RolePuppet rolePuppet = new RolePuppet();
            rolePuppet.RoleID = puppetDTO.RoleID;
            rolePuppet.PuppetDict =Utility.Json.ToJson(puppetDTO.PuppetDict);
            rolePuppet.IsBattle = puppetDTO.IsBattle;
            return rolePuppet;
        }

        PuppetIndividual ChangeDataType(PuppetIndividualDTO individualDTO)
        {
            PuppetIndividual puppetIndividual = new PuppetIndividual();
            puppetIndividual.PuppetID = individualDTO.PuppetID;
            puppetIndividual.AttackPhysical = individualDTO.AttackPhysical;
            puppetIndividual.AttackPower = individualDTO.AttackPower;
            puppetIndividual.AttackSpeed = individualDTO.AttackSpeed;
            puppetIndividual.DefendPower = individualDTO.DefendPower;
            puppetIndividual.DefendPhysical = individualDTO.DefendPhysical;
            puppetIndividual.HP = individualDTO.HP;
            puppetIndividual.ID = individualDTO.ID;
            puppetIndividual.MP = individualDTO.MP;
            puppetIndividual.PuppetDurable = individualDTO.PuppetDurable;
            puppetIndividual.PuppetDurableMax = puppetIndividual.PuppetDurable;
            puppetIndividual.Skills = Utility.Json.ToJson(individualDTO.Skills);
            return puppetIndividual;
        }
    }
}
