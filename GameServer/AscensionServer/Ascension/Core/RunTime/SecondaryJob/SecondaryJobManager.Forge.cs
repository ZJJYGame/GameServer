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
        #region  Redis模块   
        /// <summary>
        /// 学习新锻造配方
        /// </summary>
       async void UpdateForgeS2C(int roleID,int useItemID)
        {
            var formulaExist = GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaForgeData>>(out var formulaDataDict);
            if (!formulaExist)
            {
                Utility.Debug.LogInfo("YZQ添加锻造配方请求1");
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            var roleexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            if (ringServer == null|| !roleexist)
            {
                Utility.Debug.LogInfo("YZQ添加锻造配方请求2");
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }
            var role = RedisHelper.Hash.HashGetAsync<RoleDTO>(RedisKeyDefine._RolePostfix, roleID.ToString()).Result;
            if (InventoryManager.VerifyIsExist(useItemID, 1, ringServer.RingIdArray)&& role!=null)
            {
                var tempid = Utility.Converter.RetainInt32(useItemID, 5);
                var forgeExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._ForgePerfix, roleID.ToString()).Result;
                Utility.Debug.LogInfo("YZQ添加锻造配方请求判断1"+ forgeExist+ roleID);
                if (forgeExist)
                {
                    var forge = RedisHelper.Hash.HashGetAsync<ForgeDTO>(RedisKeyDefine._ForgePerfix, roleID.ToString()).Result;
                    Utility.Debug.LogInfo("YZQ添加锻造配方请求判断2"+ (forge != null));
                    if (forge != null)
                    {
                        if (formulaDataDict.TryGetValue(tempid, out var formula))
                        {
                            if (formula.NeedJobLevel > forge.JobLevel)
                            {
                                Utility.Debug.LogInfo("YZQ添加锻造配方请求3");
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "副职业等级不够！！");
                                return;
                            }
                            #region 等级判断
                            if (formula.FormulaLevel > role.RoleLevel)
                            {
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "人物等级不够！！");
                                return;
                            }
                            #endregion
                            if (!forge.Recipe_Array.Contains(tempid))
                            {
                                forge.Recipe_Array.Add(tempid);
                                Dictionary<byte, object> dict = new Dictionary<byte, object>();
                                dict.Add((byte)ParameterCode.JobForge, forge);

                                RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, dict);
                                InventoryManager.Remove(roleID, useItemID);
                                await NHibernateQuerier.UpdateAsync(ChangeDataType(forge));
                                await RedisHelper.Hash.HashSetAsync<ForgeDTO>(RedisKeyDefine._ForgePerfix, roleID.ToString(), forge);
                            }
                            else
                            {
                                Utility.Debug.LogInfo("YZQ添加锻造配方请求4,需要提示已习得该配方");
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "此配方已经学会，无法再次学习");
                            }
                        }
                    }
                    else { RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "学习配方失败");
                        // UpdateForgeMySql(roleID, useItemID, nHCriteria);
                    }

                    }
                else { RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "学习配方失败");
                    //UpdateForgeMySql(roleID, useItemID, nHCriteria); 
                    }

                }
            else
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "学习配方失败");

        }
        /// <summary>
        /// 锻造武器法宝
        /// </summary>
        async void CompoundForge(int roleID, int useItemID)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<byte, SecondaryJobData>>(out var secondary);
            var forgeExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._ForgePerfix, roleID.ToString()).Result;
            var roleExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
            var assestExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
            var roleweaponExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleWeaponPostfix, roleID.ToString()).Result;
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteria);
            if (forgeExist && roleExist && assestExist && roleweaponExist)
            {
                var forge = RedisHelper.Hash.HashGetAsync<ForgeDTO>(RedisKeyDefine._ForgePerfix, roleID.ToString()).Result;
                var role = RedisHelper.Hash.HashGetAsync<RoleStatus>(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString()).Result;
                var assest = RedisHelper.Hash.HashGetAsync<RoleAssetsDTO>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                var roleweapon = RedisHelper.Hash.HashGetAsync<RoleWeaponDTO>(RedisKeyDefine._RoleWeaponPostfix, roleID.ToString()).Result;
                if (forge != null && role != null && assest != null && roleweapon != null)
                {
                    var forgeid = 0;//锻造出来的装备法宝唯一ID
                    GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaForgeData>>(out var formulaDataDict);
                    if (forge.Recipe_Array.Contains(useItemID))
                    {
                        formulaDataDict.TryGetValue(useItemID, out var formulaData);
                        for (int i = 0; i < formulaData.NeedItemArray.Count; i++)
                        {
                            if (!InventoryManager.VerifyIsExist(formulaData.NeedItemArray[i], formulaData.NeedItemNumber[i], ringServer.RingIdArray))
                            {
                                RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundForge);
                                return;
                            }
                        }
                        if (formulaData.NeedMoney > assest.SpiritStonesLow || formulaData.NeedVitality > role.Vitality)
                        {
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundForge);
                            return;
                        }
                        var randNum = drollRandom.Next(1, 101);
                        Utility.Debug.LogError("随机出的数据为" + randNum + "成功率为" + formulaData.SuccessRate);
                        if (randNum > formulaData.SuccessRate)
                        {
                            RoleStatusCompoundFailS2C(roleID, SecondaryJobOpCode.CompoundForge, default);
                            Utility.Debug.LogInfo("YZQ鍛造失敗随机数：" + randNum + "成功率：" + formulaData.SuccessRate);
                            return;
                        }
                        forge.JobLevelExp += formulaData.MasteryValue;
                        Utility.Debug.LogInfo("YZQ鍛造增加的经验：" + forge.JobLevelExp);
                        secondary.TryGetValue((byte)FormulaDrugType.Forge, out var secondaryJob);
                        if (secondaryJob.SecondaryJobLevel.Contains(forge.JobLevel))
                        {
                            var index = secondaryJob.SecondaryJobLevel.FindIndex(f => f == forge.JobLevel);
                            if (forge.JobLevel < 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= forge.JobLevelExp)
                                {
                                    forge.JobLevelExp -= secondaryJob.SecondaryJobExp[index];
                                    forge.JobLevel += 1;
                                }
                            }
                            else if (forge.JobLevel == 5)
                            {
                                if (secondaryJob.SecondaryJobExp[index] <= forge.JobLevelExp)
                                {
                                    forge.JobLevelExp = secondaryJob.SecondaryJobExp[index];
                                }
                            }
                            Utility.Debug.LogInfo("YZQ鍛造增加的经验：" + forge.JobLevelExp);
                        }

                        role.Vitality -= formulaData.NeedVitality;
                        assest.SpiritStonesLow -= formulaData.NeedMoney;

                        var weapobObj = ForgeStatusAlgorithm(formulaData.ItemID);
                        if (weapobObj != null)
                        {
                            if (formulaData.SyntheticType != 9)//锻造的非法宝装备
                            {
                                var indexExist = roleweapon.Weaponindex.TryGetValue(formulaData.ItemID, out int id);
                                if (indexExist)
                                {
                                    roleweapon.Weaponindex[formulaData.ItemID] = id + 1;
                                    roleweapon.WeaponStatusDict.Add(Convert.ToInt32(formulaData.ItemID + "" + (id + 1)), weapobObj);
                                    forgeid = Convert.ToInt32(formulaData.ItemID + "" + (id + 1));
                                }
                                else
                                {
                                    roleweapon.Weaponindex.Add(formulaData.ItemID, 1);
                                    roleweapon.WeaponStatusDict.Add(Convert.ToInt32(formulaData.ItemID + "" + 1), weapobObj);
                                    forgeid = Convert.ToInt32(formulaData.ItemID + "" + 1);
                                }
                            }
                            else
                            {
                                var indexExist = roleweapon.Magicindex.TryGetValue(formulaData.ItemID, out int id);
                                if (indexExist)
                                {
                                    roleweapon.Magicindex[formulaData.ItemID] = id + 1;
                                    roleweapon.MagicStatusDict.Add(Convert.ToInt32(formulaData.ItemID + "" + (id + 1)), weapobObj);
                                    forgeid = Convert.ToInt32(formulaData.ItemID + "" + (id + 1));
                                }
                                else
                                {
                                    roleweapon.Magicindex.Add(formulaData.ItemID, 1);
                                    roleweapon.MagicStatusDict.Add(Convert.ToInt32(formulaData.ItemID + "" + 1), weapobObj);
                                    forgeid = Convert.ToInt32(formulaData.ItemID + "" + 1);
                                }
                            }

                            roleweapon.RoleID = roleID;
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleWeaponPostfix, roleID.ToString(), roleweapon);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(roleweapon));
                            InventoryManager.AddNewItem(roleID, forgeid, 1);

                            Dictionary<byte, object> dict = new Dictionary<byte, object>();
                            dict.Add((byte)ParameterCode.JobForge, forge);
                            dict.Add((byte)ParameterCode.RoleAssets, assest);
                            dict.Add((byte)ParameterCode.RoleStatus, role);
                            RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.CompoundForge, dict);

                            #region 更新到数据库

                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._ForgePerfix, roleID.ToString(), forge);
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString(), assest);
                            await RedisHelper.Hash.HashSetAsync(RedisKeyDefine._RoleStatsuPerfix, roleID.ToString(), role);
                            await NHibernateQuerier.UpdateAsync(role);
                            await NHibernateQuerier.UpdateAsync(assest);
                            await NHibernateQuerier.UpdateAsync(ChangeDataType(forge));
                            #endregion
                        }
                        else
                        {
                            Utility.Debug.LogInfo("YZQ计算属性鍛造失");
                            RoleStatusFailS2C(roleID, SecondaryJobOpCode.CompoundForge);
                        }

                    }
                }
            }
        }

        #endregion


        #region MySql
        /// <summary>
        /// 学习新配方
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="useItemID"></param>
        /// <param name="nHCriteria"></param>
        async void UpdateForgeMySql(int roleID, int useItemID, NHCriteria nHCriteria)
        {
            Utility.Debug.LogInfo("YZQ添加锻造配方请求5");
            var forge = NHibernateQuerier.CriteriaSelect<Forge>(nHCriteria);
            if (forge != null)
            {
                var tempid = Utility.Converter.RetainInt32(useItemID, 5);
                var recipe = Utility.Json.ToObject<List<int>>(forge.Recipe_Array);
                if (!recipe.Contains(tempid))
                {

                    Utility.Debug.LogInfo("YZQ添加锻造配方请求6");
                    recipe.Add(tempid);
                    forge.Recipe_Array = Utility.Json.ToJson(recipe);
                    RoleStatusSuccessS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, ChangeDataType(forge));
                    InventoryManager.Remove(roleID, useItemID);
                    await NHibernateQuerier.UpdateAsync(forge);
                    await RedisHelper.Hash.HashSetAsync<ForgeDTO>(RedisKeyDefine._ForgePerfix, roleID.ToString(), ChangeDataType(forge));
                }
                else {
                    RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "学习配方失败");
                }
                   
            }
            else
                RoleStatusFailS2C(roleID, SecondaryJobOpCode.StudySecondaryJobStatus, "学习配方失败");
        }      
        #endregion
        /// <summary>
        /// 装备锻造属性计算
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WeaponDTO ForgeStatusAlgorithm(int id)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, ForgeParameter>>(out var forgeparameter);
            var result = forgeparameter.TryGetValue(id,out var parameter);
            if (result)
            {
                WeaponDTO weapon = new WeaponDTO();
                weapon.WeaponType = parameter.WeaponType;
                for (int i = 0; i < parameter.WeaponAttributeMax.Count; i++)
                {
                    weapon.WeaponAttribute.Add(Utility.Algorithm.RandomRange(parameter.WeaponAttributeMin[i], parameter.WeaponAttributeMax[i] + 1));
                }
                for (int i = 0; i < parameter.SkillProbability.Count; i++)
                {
                    var num = Utility.Algorithm.RandomRange(0,101);
                    if (num<= parameter.SkillProbability[i])
                    {
                        weapon.WeaponSkill.Add(parameter.WeaponSkill[i]);
                    }
                }

                weapon.WeaponDurable = parameter.WeaponDurable;
                return weapon;
            }
            else
                return null;
        }

        ForgeDTO ChangeDataType(Forge forge)
        {
            ForgeDTO forgeDTO = new ForgeDTO();
            forgeDTO.RoleID = forge.RoleID;
            forgeDTO.JobLevel = forge.JobLevel;
            forgeDTO.JobLevelExp = forge.JobLevelExp;
            forgeDTO.Recipe_Array =Utility.Json.ToObject<HashSet<int>>(forge.Recipe_Array);
            return forgeDTO;
        }

        Forge ChangeDataType(ForgeDTO forgeDTO)
        {
            Forge forge = new Forge();
            forge.RoleID = forgeDTO.RoleID;
            forge.JobLevel = forgeDTO.JobLevel;
            forge.JobLevelExp = forgeDTO.JobLevelExp;
            forge.Recipe_Array = Utility.Json.ToJson(forge.Recipe_Array);
            return forge;
        }

        RoleWeapon ChangeDataType(RoleWeaponDTO weaponDTO)
        {
            RoleWeapon weapon = new RoleWeapon();
            weapon.RoleID = weaponDTO.RoleID;
            weapon.Magicindex = Utility.Json.ToJson(weaponDTO.Magicindex);
            weapon.MagicStatusDict = Utility.Json.ToJson(weaponDTO.MagicStatusDict);
            weapon.Weaponindex = Utility.Json.ToJson(weaponDTO.Weaponindex);
            weapon.WeaponStatusDict = Utility.Json.ToJson(weaponDTO.WeaponStatusDict);
            return weapon;
        }

        #region 新版随机数
        public class DrollRandom
        {
            private const int N = 624;
            private const int M = 397;
            private const uint MATRIX_A = 0x9908b0dfU;   // constant vector a
            private const uint UPPER_MASK = 0x80000000U; // most significant w-r bits
            private const uint LOWER_MASK = 0x7fffffffU; // least significant r bits


            private uint seed;

            private int returnLength;

            private int maxSize;

            // the array for the state vector
            private uint[] mt = new uint[N];

            private int mti = N + 1;

            public DrollRandom()
            {
                this.seed = (uint)DateTime.Now.Millisecond;
                var initArray = new uint[] { 0x123, 0x234, 0x345, 0x456 };
                InitByArray(initArray, initArray.Length);
            }

            public DrollRandom(uint seed)
            {
                this.seed = seed;
                var initArray = new uint[] { 0x123, 0x234, 0x345, 0x456 };
                InitByArray(initArray, initArray.Length);
            }

            public uint Seed
            {
                get { return seed; }
            }
            public int[] Twist(uint seed, int returnLength, int maxSize)
            {
                uint[] initArray;
                int[] returnArray;

                this.seed = seed;
                this.returnLength = returnLength;
                this.maxSize = maxSize;

                mti = N + 1;
                mt = new uint[N];

                initArray = new uint[] { 0x123, 0x234, 0x345, 0x456 };
                returnArray = new int[returnLength];
                InitByArray(initArray, initArray.Length);
                for (int i = 0; i < returnLength; i++)
                {
                    returnArray[i] = (int)(GenrandInt32() % maxSize);
                }
                return returnArray;
            }

            /// <summary>
            /// 从0到maxValue,不包括maxValue
            /// </summary>
            /// <param name="maxValue">最大值</param>
            /// <returns></returns>
            public int Next(int maxValue)
            {
                this.maxSize = maxValue;
                return (int)(GenrandInt32() % maxSize);
            }

            /// <summary>
            /// 从minValue到maxValue,不包括maxValue
            /// </summary>
            /// <param name="minValue">最小值</param>
            /// <param name="maxValue">最大值</param>
            /// <returns></returns>
            public int Next(int minValue, int maxValue)
            {
                int tmp = maxValue - minValue;
                return minValue + Next(tmp);
            }

            private uint GenrandInt32()
            {
                uint y;
                uint[] mag01 = new uint[] { 0x0, MATRIX_A };
                if (mti >= N)
                { /* generate N words at one time */
                    int kk;

                    if (mti == N + 1)   /* if init_genrand() has not been called, */
                        InitGenrand(5489U); /* a default initial seed is used */

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                    }
                    y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                    mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];

                    mti = 0;
                }

                y = mt[mti++];

                // Tempering
                y ^= (y >> 11);
                y ^= (y << 7) & 0x9d2c5680U;
                y ^= (y << 15) & 0xefc60000U;
                y ^= (y >> 18);

                return y;
            }

            private void InitByArray(uint[] init_key, int key_length)
            {
                int i, j, k;
                InitGenrand(Seed);
                //init_genrand(19650218);
                i = 1; j = 0;
                k = (N > key_length ? N : key_length);
                for (; k > 0; k--)
                {
                    mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j); /* non linear */
                    mt[i] &= 0xffffffff; // for WORDSIZE > 32 machines
                    i++; j++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                    if (j >= key_length) j = 0;
                }
                for (k = N - 1; k > 0; k--)
                {
                    mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941U)) - i); /* non linear */
                    mt[i] &= 0xffffffffU; // for WORDSIZE > 32 machines
                    i++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                }

                mt[0] = 0x80000000U; // MSB is 1; assuring non-zero initial array
            }

            // initializes mt[N] with a seed
            private void InitGenrand(uint seed)
            {
                mt[0] = seed & 0xffffffffU;
                for (mti = 1; mti < N; mti++)
                {
                    mt[mti] = (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                    mt[mti] &= 0xffffffffU;

                }
            }
        }
        #endregion
    }
}
