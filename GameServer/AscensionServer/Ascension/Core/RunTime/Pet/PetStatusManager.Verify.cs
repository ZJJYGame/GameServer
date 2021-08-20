using Cosmos;
using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using System.Collections.Generic;
using RedisDotNet;
using System;

namespace AscensionServer
{
   public partial class PetStatusManager
    {
        /// <summary>
        /// 验证宠物当前加点数
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="points"></param>
        public void VerifyPetAbilitypoint(Pet pet,out int points)
        {
            points = 0;          
           GameEntry. DataManager.TryGetValue<Dictionary<int, PetLevelData>>(out var petLevelDataDict);
            foreach (var item in petLevelDataDict)
            {
                if (pet.PetLevel>= item.Key)
                {
                    points += item.Value.FreeAttributes;
                }
            }
        }
        public enum AttributionType
        {
            HP=0,
            MP=1,
            ShenHun=2,
            BestBlood=3,
            AttackPhysical=4,
            DefendPhysical=5,
            AttackPower=6,
            DefendPower=7,
            AttackSpeed=8,
            MoveSpeed=9,
            PhysicalCritProb=10,
            MagicCritProb=11,
            ReduceCritProb=12,
            PhysicalCritDamage=13,
            MagicCritDamage=14,
            ReduceCritDamage=15
        }

        public enum GrowUpDrugType : int
        {
            HPGrowUp = 16039,
            SoulGrowUp = 16041,
            SpeedGrowUp = 16042,
            AttackDamageGrowUp = 16043,
            ResistanceAttackGrowUp = 16044,
            PowerGrowUp = 16045,
            ResistancePowerGrowUp = 16046,
            GrowUp = 16049
        }

        /// <summary>
        /// 验证宠物完整属性
        /// </summary>
        public PetStatus VerifyPetAllStatus(int petid,PetAbilityPointDTO petAbilityPoint=null, PetAptitude petAptitude=null,PetStatus petStatus=null,Pet pet=null)
        {
            if (petAbilityPoint==null)
            {
                var petAbilityPointexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetAbilityPointPerfix, petid.ToString()).Result;
                if (petAbilityPointexist)
                {
                    petAbilityPoint = RedisHelper.Hash.HashGetAsync<PetAbilityPointDTO>(RedisKeyDefine._PetAbilityPointPerfix, petid.ToString()).Result;
                    if (petAbilityPoint==null)
                    {
                        petAbilityPoint = new PetAbilityPointDTO();
                    }
                }
            }
            if (petAptitude == null)
            {
                var petAptitudeexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetAptitudePerfix, petid.ToString()).Result;
                if (petAptitudeexist)
                {
                   var petAptitudeTemp = RedisHelper.Hash.HashGetAsync<PetAptitudeDTO>(RedisKeyDefine._PetAptitudePerfix, petid.ToString()).Result;
                    if (petAptitudeTemp == null)
                    {
                        petAptitude = new PetAptitude();
                    }
                    else
                    {
                        petAptitude = AssignSameFieldValue(new PetAptitude(), petAptitudeTemp); 
                    }
                }
            }

            if (petStatus == null)
            {
                var petStatusexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetStatusPerfix, petid.ToString()).Result;
                if (petStatusexist)
                {
                     petStatus = RedisHelper.Hash.HashGetAsync<PetStatus>(RedisKeyDefine._PetStatusPerfix, petid.ToString()).Result;
                    if (petStatus == null)
                    {
                        petStatus = new PetStatus();
                    }
                }
            }
            if (pet == null)
            {
                var petexist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PetPerfix, petid.ToString()).Result;
                if (petexist)
                {
                  var  petObj = RedisHelper.Hash.HashGetAsync<PetDTO>(RedisKeyDefine._PetPerfix, petid.ToString()).Result;
                    if (petObj == null)
                    {
                        pet = new Pet();
                    }
                    else
                    {
                        pet= ChangeDataType(petObj);
                    }
                }
            }

            GameEntry. DataManager.TryGetValue<Dictionary<int, PetLevelData>>(out var petLevelDataDict);
            var petStatusObj=  VerifyPetAbilityAddition(petAbilityPoint, petAptitude);
            Utility.Debug.LogError("需要加成的点数" + Utility.Json.ToJson(petStatusObj));

            var skillList = Utility.Json.ToObject<List<int>>(pet.PetSkillArray);

            VerifyPetPassivitySkill(skillList, out var petStatusDTOs);

            //ResetPetStatus(pet, petAptitude,out var petStatusAtitude);

            StatusAddition(petStatusObj, petStatusDTOs, petLevelDataDict[pet.PetLevel], out var petStatusTemp);
            return petStatusTemp;
        }
        /// <summary>
        /// 验证加点后的加成
        /// </summary>
        /// <param name="petAbilityPoint"></param>
        /// <param name="petStatusDTO"></param>
        /// <param name="petLevelData"></param>
        /// <returns></returns>
        public PetStatus VerifyPetAbilityAddition(PetAbilityPointDTO petAbilityPoint, PetAptitude petAptitude)
        {
            PetStatus petStatusDTO = new PetStatus();

            petStatusDTO.PetMaxHP = (petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Corporeity * 10) * (int)petAptitude.Petaptitudecol/1000;
            petStatusDTO.PetMaxMP = (petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Power * 6 + petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Corporeity) * (int)petAptitude.Petaptitudecol / 1000;
            petStatusDTO.PetMaxShenhun = (int)(petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Soul * 4 * (int)petAptitude.Petaptitudecol / 1000);
            petStatusDTO.AttackSpeed =(int) ((petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Agility * 0.5f+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Strength*0.1f+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Corporeity * 0.1f+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Stamina * 0.1f+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Soul * 0.2f) * (int)petAptitude.Petaptitudecol / 1000);
            petStatusDTO.AttackPhysical = petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Strength *2* (int)petAptitude.Petaptitudecol / 1000;
            petStatusDTO.DefendPhysical = (int)(( petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Stamina * 2)* (int)petAptitude.Petaptitudecol / 1000);
            petStatusDTO.AttackPower = petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Power *2* (int)petAptitude.Petaptitudecol / 1000;
            petStatusDTO.DefendPower = (int)((petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Corporeity * 0.4f+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Stamina * 0.8+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Strength *1.2f+ petAbilityPoint.AbilityPointSln[petAbilityPoint.SlnNow].Power * 1)* (int)petAptitude.Petaptitudecol / 1000);
            return petStatusDTO;
        }
        /// <summary>
        ///  获得所有被动技能的加成
        /// </summary>
        /// <param name="skillid"></param>
        public void VerifyPetPassivitySkill(List<int>skillid,out List<PetStatusDTO> petAbilityStatus)
        {
            List<PetSkillData> passivitySkillList = new List<PetSkillData>();
           GameEntry. DataManager.TryGetValue<Dictionary<int, PetSkillData>>(out var petSkillDataDict);
            petAbilityStatus = new List<PetStatusDTO>();
            var petstatusProp = ReferencePool.Accquire<PetStatusDTO>();
            var petstatusFixed =ReferencePool.Accquire<PetStatusDTO>();
            for (int i = 0; i < skillid.Count; i++)
            {
                if (!petSkillDataDict[skillid[i]].ISActiveSkill&& petSkillDataDict[skillid[i]].AttributionType.Count>0)
                {
                    passivitySkillList.Add(petSkillDataDict[skillid[i]]);
                }
            }

            for (int i = 0; i < passivitySkillList.Count; i++)
            {
                for (int j = 0; j < passivitySkillList[i].AttributionType.Count; j++)
                {
                    switch ((AttributionType)passivitySkillList[i].AttributionType[j])
                    {
                        case AttributionType.HP:
                            petstatusProp.PetHP += passivitySkillList[i].Percentage[j];
                            petstatusFixed.PetHP += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.MP:
                            petstatusProp.PetMP += passivitySkillList[i].Percentage[j];
                            petstatusFixed.PetMP += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.ShenHun:
                            petstatusProp.PetShenhun += passivitySkillList[i].Percentage[j];
                            petstatusFixed.PetShenhun += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.AttackPhysical:
                            petstatusProp.AttackPhysical += passivitySkillList[i].Percentage[j];
                            petstatusFixed.AttackPhysical += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.DefendPhysical:
                            petstatusProp.DefendPhysical += passivitySkillList[i].Percentage[j];
                            petstatusFixed.DefendPhysical += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.AttackPower:
                            petstatusProp.AttackPower += passivitySkillList[i].Percentage[j];
                            petstatusFixed.AttackPower += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.DefendPower:
                            petstatusProp.DefendPower += passivitySkillList[i].Percentage[j];
                            petstatusFixed.DefendPower += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.AttackSpeed:
                            petstatusProp.AttackSpeed += passivitySkillList[i].Percentage[j];
                            petstatusFixed.AttackSpeed += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.PhysicalCritProb:
                            petstatusProp.PhysicalCritProb += passivitySkillList[i].Percentage[j];
                            petstatusFixed.PhysicalCritProb += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.MagicCritProb:
                            petstatusProp.MagicCritProb += passivitySkillList[i].Percentage[j];
                            petstatusFixed.MagicCritProb += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.ReduceCritProb:
                            petstatusProp.ReduceCritProb += passivitySkillList[i].Percentage[j];
                            petstatusFixed.ReduceCritProb += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.PhysicalCritDamage:
                            petstatusProp.PhysicalCritDamage += passivitySkillList[i].Percentage[j];
                            petstatusFixed.PhysicalCritDamage += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.MagicCritDamage:
                            petstatusProp.MagicCritDamage += passivitySkillList[i].Percentage[j];
                            petstatusFixed.MagicCritDamage += passivitySkillList[i].FixedValue[j];
                            break;
                        case AttributionType.ReduceCritDamage:
                            petstatusProp.ReduceCritDamage += passivitySkillList[i].Percentage[j];
                            petstatusFixed.ReduceCritDamage += passivitySkillList[i].FixedValue[j];
                            break;
                        default:
                            break;
                    }
                }
            }

            petAbilityStatus.Add(petstatusProp);
            petAbilityStatus.Add(petstatusFixed);
        }
        /// <summary>
        /// 加成计算资质后计算加点后的总加成
        /// </summary>
        /// <param name="petStatus"></param>
        /// <param name="petAbilityStatus"></param>
        /// <param name="petStatusTemp"></param>
        public void StatusAddition(PetStatus petStatus, List<PetStatusDTO> petAbilityStatus, PetLevelData petLevelData, out PetStatus petStatusTemp)
        {
            petStatusTemp= new PetStatus();
            petStatusTemp.AttackPhysical =(petStatus.AttackPhysical ) + (petLevelData.AttackPhysical * (petAbilityStatus[0].AttackPhysical + 100) / 100) + petAbilityStatus[1].AttackPhysical;
            petStatusTemp.AttackPower = (petStatus.AttackPower) + (petLevelData.AttackPower * (petAbilityStatus[0].AttackPower + 100) / 100) + petAbilityStatus[1].AttackPower;
            petStatusTemp.AttackSpeed =  (petStatus.AttackSpeed ) + (petLevelData.AttackSpeed * (petAbilityStatus[0].AttackSpeed + 100) / 100) + petAbilityStatus[1].AttackSpeed;
            petStatusTemp.DefendPhysical = (petStatus.DefendPhysical ) + (petLevelData.DefendPhysical * (petAbilityStatus[0].DefendPhysical + 100) / 100) + petAbilityStatus[1].DefendPhysical;
            petStatusTemp.DefendPower = (petStatus.DefendPower ) + (petLevelData.DefendPower * (petAbilityStatus[0].DefendPower + 100) / 100) + petAbilityStatus[1].DefendPower;
            petStatusTemp.ExpLevelUp =  (petStatus.ExpLevelUp ) + (petLevelData.ExpLevelUp * (petAbilityStatus[0].ExpLevelUp + 100) / 100) + petAbilityStatus[1].ExpLevelUp;
            petStatusTemp.MagicCritDamage =  (petStatus.MagicCritDamage ) + (petLevelData.MagicCritDamage * (petAbilityStatus[0].MagicCritDamage + 100) / 100) + petAbilityStatus[1].MagicCritDamage;
            petStatusTemp.MagicCritProb = (petStatus.MagicCritProb ) + (petLevelData.MagicCritProb * (petAbilityStatus[0].MagicCritProb + 100) / 100) + petAbilityStatus[1].MagicCritProb;
            petStatusTemp.PetMaxHP = (petStatus.PetMaxHP) + (petLevelData.PetHP * (petAbilityStatus[0].PetHP + 100) / 100) + petAbilityStatus[1].PetHP;
            petStatusTemp.PetHP = petStatusTemp.PetMaxHP;
            petStatusTemp.PetMaxMP =  (petStatus.PetMaxMP) + (petLevelData.PetMP * (petAbilityStatus[0].PetMP + 100) / 100) + petAbilityStatus[1].PetMP;
            petStatusTemp.PetMP = petStatusTemp.PetMaxMP;
            petStatusTemp.PetMaxShenhun = (petStatus.PetMaxShenhun) + (int)(petLevelData. Petsoul* (petAbilityStatus[0].PetShenhun + 100) / 100) + petAbilityStatus[1].PetShenhun;
            petStatusTemp.PetShenhun = petStatusTemp.PetMaxShenhun;
            petStatusTemp.PhysicalCritDamage =  (petStatus.PhysicalCritDamage ) + (petLevelData.PhysicalCritDamage * (petAbilityStatus[0].PhysicalCritDamage + 100) / 100) + petAbilityStatus[1].PhysicalCritDamage;
            petStatusTemp.PhysicalCritProb = (petStatus.PhysicalCritProb) + (petLevelData.PhysicalCritProb * (petAbilityStatus[0].PhysicalCritProb + 100) / 100) + petAbilityStatus[1].PhysicalCritProb;
            petStatusTemp.ReduceCritDamage = (petStatus.ReduceCritDamage ) + (petLevelData.ReduceCritDamage * (petAbilityStatus[0].ReduceCritDamage + 100) / 100) + petAbilityStatus[1].ReduceCritDamage;
            petStatusTemp.ReduceCritProb =  (petStatus.ReduceCritProb ) + (petLevelData.ReduceCritProb * (petAbilityStatus[0].ReduceCritProb + 100) / 100) + petAbilityStatus[1].ReduceCritProb;
        }

        /// <summary>
        /// 资质DO转换DTO
        /// </summary>
        /// <param name="petAptitude"></param>
        /// <param name="petAptitudeDTO"></param>
        /// <returns></returns>
        public PetAptitude AssignSameFieldValue(PetAptitude petAptitude, PetAptitudeDTO petAptitudeDTO)
        {
            petAptitude.AttackphysicalAptitude = petAptitudeDTO.AttackphysicalAptitude;
            petAptitude.AttackpowerAptitude = petAptitudeDTO.AttackpowerAptitude;
            petAptitude.AttackspeedAptitude = petAptitudeDTO.AttackspeedAptitude;
            petAptitude.DefendphysicalAptitude = petAptitudeDTO.DefendphysicalAptitude;
            petAptitude.DefendpowerAptitude = petAptitudeDTO.DefendpowerAptitude;
            petAptitude.HPAptitude = petAptitudeDTO.HPAptitude;
            petAptitude.Petaptitudecol = petAptitudeDTO.Petaptitudecol;
            petAptitude.SoulAptitude = petAptitudeDTO.SoulAptitude;
            petAptitude.PetID = petAptitudeDTO.PetID;
            petAptitude.PetAptitudeDrug = Utility.Json.ToJson(petAptitudeDTO.PetAptitudeDrug);
            return petAptitude;
        }
        public PetAptitudeDTO AssignSameFieldValue(PetAptitudeDTO petAptitudeDTO, PetAptitude petAptitude)
        {
            petAptitudeDTO.AttackphysicalAptitude = petAptitude.AttackphysicalAptitude;
            petAptitudeDTO.AttackpowerAptitude = petAptitude.AttackpowerAptitude;
            petAptitudeDTO.AttackspeedAptitude = petAptitude.AttackspeedAptitude;
            petAptitudeDTO.DefendphysicalAptitude = petAptitude.DefendphysicalAptitude;
            petAptitudeDTO.DefendpowerAptitude = petAptitude.DefendpowerAptitude;
            petAptitudeDTO.HPAptitude = petAptitude.HPAptitude;
            petAptitudeDTO.Petaptitudecol = petAptitude.Petaptitudecol;
            petAptitudeDTO.SoulAptitude = petAptitude.SoulAptitude;
            petAptitudeDTO.PetID = petAptitude.PetID;
            petAptitudeDTO.PetAptitudeDrug = Utility.Json.ToObject<Dictionary<int,int>>(petAptitude.PetAptitudeDrug);
            return petAptitudeDTO;
        }

        PetAbilityPointDTO ChangeDataType(PetAbilityPoint petAbility)
        {
            PetAbilityPointDTO pointDTO = new PetAbilityPointDTO();
            pointDTO.AbilityPointSln = Utility.Json.ToObject<Dictionary<int,AbilityDTO>>(petAbility.AbilityPointSln);
            pointDTO.ID = petAbility.ID;
            pointDTO.IsUnlockSlnThree = petAbility.IsUnlockSlnThree;
            pointDTO.SlnNow = petAbility.SlnNow;
            return pointDTO;
        }
        PetAbilityPoint ChangeDataType(PetAbilityPointDTO pointDTO)
        {
            PetAbilityPoint petAbility = new PetAbilityPoint();
            petAbility.AbilityPointSln = Utility.Json.ToJson(pointDTO.AbilityPointSln);
            petAbility.ID = pointDTO.ID;
            petAbility.IsUnlockSlnThree = pointDTO.IsUnlockSlnThree;
            petAbility.SlnNow = pointDTO.SlnNow;
            return petAbility;
        }

        Pet ChangeDataType(PetDTO petDTO)
        {
            Pet pet = new Pet();
            pet.ID = petDTO.ID;
            pet.PetExp = petDTO.PetExp;
            pet.PetID = petDTO.PetID;
            pet.PetLevel = petDTO.PetLevel;
            pet.PetName = petDTO.PetName;
            pet.PetSkillArray = Utility.Json.ToJson(petDTO.PetSkillArray);
            pet.DemonicSoul = Utility.Json.ToJson(petDTO.DemonicSoul);
            return pet;
        }

        PetDTO ChangeDataType(Pet pet)
        {
            PetDTO petDTO = new PetDTO();
            petDTO.ID = pet.ID;
            petDTO.PetExp = pet.PetExp;
            petDTO.PetID = pet.PetID;
            petDTO.PetLevel = pet.PetLevel;
            petDTO.PetName = pet.PetName;
            petDTO.PetSkillArray = Utility.Json.ToObject<List<int>>(pet.PetSkillArray);
            petDTO.DemonicSoul = Utility.Json.ToObject<Dictionary<int, List<int>>>(pet.DemonicSoul);
            return petDTO;
        }

        #region 技能处理
        /// <summary>
        /// 洗练宠物重置技能
        /// </summary>
        public List<int> RestPetSkill(List<int> skillList)
        {
            return RandomArrayElement(skillList);
        }
        /// <summary>
        /// 技能书使用顶替技能
        /// </summary>
        public List<int> RandomSkillRemoveAdd(List<int> skillList, int skillid)
        {
            int num = (int)AverageRandom(0, skillList.Count);
            skillList.RemoveAt(num);
            skillList.Add(skillid);
            return skillList;
        }
        #endregion

        #region 正态分部随机数
        /// <summary>
        /// 正态分布随机数
        /// </summary>

        static Random aa = new Random((int)(DateTime.Now.Ticks / 10000));
        public double AverageRandom(double min, double max)//产生(min,max)之间均匀分布的随机数
        {
            int MINnteger = (int)(min * 10000);
            int MAXnteger = (int)(max * 10000);
            int resultInteger = aa.Next(MINnteger, MAXnteger);
            return resultInteger / 10000.0;
        }
        public double Normal(double x, double miu, double sigma) //正态分布概率密度函数
        {
            return 1.0 / (x * Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(-1 * (Math.Log(x) - miu) * (Math.Log(x) - miu) / (2 * sigma * sigma));
        }
        public double Random_Normal(double miu, double sigma, double min, double max)//产生正态分布随机数
        {
            double x;
            double dScope;
            double y;
            do
            {
                x = AverageRandom(min, max);
                y = Normal(x, miu, sigma);
                dScope = AverageRandom(0, Normal(miu, miu, sigma));
            } while (dScope > y);
            return x;
        }

        #endregion

        #region 技能数组随机
        public List<int> RandomArrayElement(List<int> skillList)
        {
            double randomNum = AverageRandom(0, skillList.Count);
            List<int> numList = new List<int>();
            List<int> tempSkillList = new List<int>();
            for (int i = 0; i < randomNum; i++)
            {
                var temp = (int)AverageRandom(1, skillList.Count);
                if (!numList.Contains(temp))
                {
                    numList.Add(temp);
                    tempSkillList.Add(skillList[temp]);
                }
            }
            return tempSkillList;
        }

        #endregion

        #region 真正的正态分布策划版
        int[,] Map = new int[25, 10]
        {
            {5000,5040,5080,5120,5160,5199,5239,5279,5319,5359},
            {5398,5438,5478,5517,5557,5596,5636,5675,5714,5753},
            {5793,5832,5871,5910,5948,5987,6026,6064,6103,6141},
            {6179,6217,6255,6293,6331,6368,6406,6443,6480,6517},
            {6554,6591,6628,6664,6700,6736,6772,6808,6844,6879},
            {6915,6950,6985,7019,7054,7088,7123,7157,7190,7224},
            {7257,7291,7324,7357,7389,7422,7454,7486,7517,7549},
            {7580,7611,7642,7673,7703,7734,7764,7794,7823,7852},
            {7881,7910,7939,7967,7995,8023,8051,8078,8106,8133},
            {8159,8186,8212,8238,8264,8289,8315,8340,8365,8389},
            {8413,8438,8461,8485,8508,8531,8554,8577,8599,8621},
            {8643,8665,8686,8708,8729,8749,8770,8790,8810,8830},
            {8849,8869,8888,8907,8925,8944,8962,8980,8997,9015},
            {9032,9049,9066,9082,9099,9115,9131,9147,9162,9177},
            {9192,9207,9222,9236,9251,9265,9278,9292,9306,9319},
            {9332,9345,9357,9370,9382,9394,9406,9418,9430,9441},
            {9452,9463,9474,9484,9495,9505,9515,9525,9535,9545},
            {9554,9564,9573,9582,9591,9599,9608,9616,9625,9633},
            {9641,9648,9656,9664,9671,9678,9686,9693,9700,9706},
            {9713,9719,9726,9732,9738,9744,9750,9756,9762,9767},
            {9772,9778,9783,9788,9793,9798,9803,9808,9812,9817},
            {9821,9826,9830,9834,9838,9842,9846,9850,9854,9857},
            {9861,9864,9868,9871,9874,9878,9881,9884,9887,9890},
            {9893,9896,9898,9901,9904,9906,9909,9911,9913,9916},
            {9918,9920,9922,9925,9927,9929,9931,9932,9934,9936}
        };
        Random random = new Random();
        public int NormalRandom2(int min, int max)
        {
            int result = 0;
            bool flag = true;
            while (flag)
            {

                int x = random.Next(0, 250);
                int y = 10000 - Map[x / 10, x % 10];
                if (y >= (int)random.Next(0, 10000))
                {
                    result = x;
                    flag = false;
                }
            }
            if ((int)random.Next(0, 2) == 1)
            {
                result = (min + max) / 2 - (result + (int)random.Next(0, 2)) * (max - min) / 2 / 250;
            }
            else
            {
                result = (min + max) / 2 + (result + (int)random.Next(0, 2)) * (max - min) / 2 / 250;
            }
            return result;
        }
        #endregion

    }
}


