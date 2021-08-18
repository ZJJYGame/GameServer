using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionProtocol.DTO;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using System.IO;
using log4net.Config;
using System.Reflection;
using ExitGames.Concurrency.Fibers;
using Cosmos;
using AscensionServer.Model;
using System.Threading;

namespace AscensionServer
{
    /// <summary>
    /// 服务器数据转换
    /// </summary>
    [Implementer]
    public class ServerDataConvertor : IDataConvertor
    {
        public void ConvertData()
        {
            //Utility.Debug.LogInfo("<DataManager> 测试 ConvertData");
            try
            {
                #region 宗门藏宝阁/藏经阁/瓶颈/心魔/仙盟等级/仙盟签到/秘术属性/人物属性/仙盟技能加成/武器装备/宠物属性/宠物成长资质/秘术书/宠物属性点
                GameEntry. DataManager.TryGetValue(typeof(FactionItemData).Name, out var factionitemdata);
                var factionitemDataDict = TransObject<List<FactionItemData>>(factionitemdata).ToDictionary(key => key.FactionItemId, value => value.FactionItem);

                GameEntry. DataManager.TryGetValue(typeof(FactionSkillData).Name, out var factionSkilldata);
                var factionSkillDataDict = TransObject<List<FactionSkillData>>(factionSkilldata).ToDictionary(key => key.SchoolID, value => value.FactionSkill);

                GameEntry. DataManager.TryGetValue(typeof(BottleneckData).Name, out var bottleneckData);
                var bottleneckDataDict = TransObject<List<BottleneckData>>(bottleneckData).ToDictionary(key => key.Level_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(DemonData).Name, out var demonData);
                var demonDataDict = TransObject<List<DemonData>>(demonData).ToDictionary(key => key.Level_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(AllianceLevleUpData).Name, out var alliancelevledata);
                var alliancelevledataDict = TransObject<List<AllianceLevleUpData>>(alliancelevledata).ToDictionary(key => key.Building_Level, value => value);

                GameEntry. DataManager.TryGetValue(typeof(AllianceSigninData).Name, out var allianceSigninData);
                var allianceSigninDataDict = TransObject<List<AllianceSigninData>>(allianceSigninData);

                GameEntry. DataManager.TryGetValue(typeof(MiShuData).Name, out var mishuData);
                var mishuDataDict = TransObject<List<MiShuData>>(mishuData).ToDictionary(key => key.MishuID, value => value);


                GameEntry. DataManager.TryGetValue(typeof(RoleStatusDatas).Name, out var roleStatusDatas);
                var roleStatusDatasDict = TransObject<List<RoleStatusDatas>>(roleStatusDatas).ToDictionary(key => key.LevelID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(AllianceSkillsData).Name, out var allianceSkillDatas);
                var allianceSkillDatasDict = TransObject<List<AllianceSkillsData>>(allianceSkillDatas).ToDictionary(key => key.GangsSkillType, value => value);

                GameEntry. DataManager.TryGetValue(typeof(EquipmentData).Name, out var equipmentData);
                var equipmentDataDict = TransObject<List<EquipmentData>>(equipmentData).ToDictionary(key => key.Weapon_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(PetLevelData).Name, out var petLevelData);
                var petLevelDataDict = TransObject<List<PetLevelData>>(petLevelData).ToDictionary(key => key.PetLevelID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(PetAptitudeData).Name, out var petAptitudeData);
                var petAptitudeDataDict = TransObject<List<PetAptitudeData>>(petAptitudeData).ToDictionary(key => key.PetID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(MishuBook).Name, out var mishuBook);
                var mishuBookDict = TransObject<List<MishuBook>>(mishuBook).ToDictionary(key => key.BookID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(PetAbilityPointData).Name, out var petAbilityPointData);
                var petAbilityPointDataDict = TransObject<List<PetAbilityPointData>>(petAbilityPointData).ToDictionary(key => key.CoefficientType, value => value);

                GameEntry.DataManager.TryGetValue(typeof(RoleAbilityPointData).Name, out var roleAbilityPointData);
                var roleAbilityPointDataDict = TransObject<List<RoleAbilityPointData>>(roleAbilityPointData).ToDictionary(key => key.CoefficientType, value => value);

                GameEntry.DataManager.TryGetValue(typeof(CreatAllianceData).Name, out var creatAllianceData);
                var creatAllianceDataDict = TransObject<List<CreatAllianceData>>(creatAllianceData).ToDictionary(key => key.ID, value => value);
                #endregion

                GameEntry. DataManager.TryGetValue(typeof(PetSkillBookData).Name, out var petSkillBookData);
                var petSkillBookDataDict = TransObject<List<PetSkillBookData>>(petSkillBookData).ToDictionary(key => key.PetSkillBookID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(DemonicSoulData).Name, out var demonicSoulData);
                var demonicSoulDataDict = TransObject<List<DemonicSoulData>>(demonicSoulData).ToDictionary(key => key.DemonicSoulID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(DemonicSoulSkillPool).Name, out var demonicSoulSkillPool);
                var demonicSoulSkillPoolDict = TransObject<List<DemonicSoulSkillPool>>(demonicSoulSkillPool).ToDictionary(key => key.PetSkillLevel, value => value);

                GameEntry. DataManager.TryGetValue(typeof(PetSkillData).Name, out var petSkillData);
                var petSkillDataDict = TransObject<List<PetSkillData>>(petSkillData).ToDictionary(key => key.SkillID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(FormulaData).Name, out var formulaData);
                var formulaDataDict = TransObject<List<FormulaData>>(formulaData).ToDictionary(key => key.FormulaID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(RoleLevelData).Name, out var roleLevelData);
                var roleLevelDataDict = TransObject<List<RoleLevelData>>(roleLevelData).ToDictionary(key=>key.LevelID,value=>value);

                GameEntry.DataManager.TryGetValue(typeof(FlyMagicToolData).Name, out var flyMagicToolData);
                var flyMagicToolDict = TransObject<List<FlyMagicToolData>>(flyMagicToolData).ToDictionary(key => key.FlyMagicToolID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(AllianceJobData).Name, out var allianceJobData);
                var allianceJobDataDict = TransObject<List<AllianceJobData>>(allianceJobData).ToDictionary(key => key.JobID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(AllianceScripturesPlatformData).Name, out var allianceScripturesPlatformData);
                var allianceScripturesPlatformDataDict = TransObject<List<AllianceScripturesPlatformData>>(allianceScripturesPlatformData).ToDictionary(key => key.ItemID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(AllianceConstructionData).Name, out var allianceConstructionData);
                var allianceConstructionDataDict = TransObject<List<AllianceConstructionData>>(allianceConstructionData).ToDictionary(key => key.BuildingType, value => value);

                GameEntry.DataManager.TryGetValue(typeof(AllianceDrugData).Name, out var allianceDrugData);
                var allianceDrugDataDict = TransObject<List<AllianceDrugData>>(allianceDrugData).ToDictionary(key => key.DrugID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(SpiritRangeData).Name, out var spiritRangeData);
                var spiritRangeDataDict = TransObject<List<SpiritRangeData>>(spiritRangeData).ToDictionary(key => key.SpiritRangeID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(FormulaDrugData).Name, out var formulaDrugData);
                var formulaDrugDataDict = TransObject<List<FormulaDrugData>>(formulaDrugData).ToDictionary(key => key.FormulaID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(ForgeParameter).Name, out var forgeParameter);
                var forgeParameterDict = TransObject<List<ForgeParameter>>(forgeParameter).ToDictionary(key => key.WeaponID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(FormulaForgeData).Name, out var formulaForgeData);
                var formulaForgeDataDict = TransObject<List<FormulaForgeData>>(formulaForgeData).ToDictionary(key => key.FormulaID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(FormulaPuppetData).Name, out var formulaPuppetData);
                var formulaPuppetDataDict = TransObject<List<FormulaPuppetData>>(formulaPuppetData).ToDictionary(key => key.FormulaID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(FormulaGlobaID).Name, out var formulaGlobaID);
                var formulaGlobaIDDict = TransObject<List<FormulaGlobaID>>(formulaGlobaID).ToDictionary(key => key.GlobalID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(PuppetUnitParameter).Name, out var puppetUnitParameter);
                var puppetUnitParameterDict = TransObject<List<PuppetUnitParameter>>(puppetUnitParameter).ToDictionary(key => key.PuppetID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(PuppetAssembleData).Name, out var puppetAssembleData);
                var puppetAssembleDataDict = TransObject<List<PuppetAssembleData>>(puppetAssembleData).ToDictionary(key => key.PuppetID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(RepairPuppetData).Name, out var repairPuppetData);
                var repairPuppetDataDict = TransObject<List<RepairPuppetData>>(repairPuppetData).ToDictionary(key => key.PuppetID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(PassiveSkillsWeapon).Name, out var passiveSkillsWeapon);
                var passiveSkillsWeaponDict = TransObject<List<PassiveSkillsWeapon>>(passiveSkillsWeapon).ToDictionary(key => key.SkillID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(PassiveSkillsRole).Name, out var passiveSkillsRole);
                var passiveSkillsRoleDict = TransObject<List<PassiveSkillsRole>>(passiveSkillsRole).ToDictionary(key => key.SkillID, value => value);

                GameEntry.DataManager.TryGetValue(typeof(DailyMsg).Name, out var DailyMsgData);
                var DailyMsgDataDict = TransObject<List<DailyMsg>>(DailyMsgData).ToDictionary(key => key.MsgIndex, value => value);

                GameEntry.DataManager.TryGetValue(typeof(SecondaryJobData).Name, out var SecondaryJob);
                var SecondaryJobDataDict = TransObject<List<SecondaryJobData>>(SecondaryJob).ToDictionary(key => key.SecondaryType, value => value);

                //GameEntry.DataManager.TryGetValue(typeof(PassiveSkillsPet).Name, out var passiveSkillsPet);
                //Utility.Debug.LogError(typeof(PassiveSkillsPet).Name);
                //var passiveSkillsPetDict = TransObject<List<PassiveSkillsPet>>(passiveSkillsPet).ToDictionary(key => key.SkillID, value => value);

                #region 背包json
                GameEntry. DataManager.TryGetValue(typeof(ItemBagBaseData).Name, out var itemBagData);
                var itemBagDict = TransObject<List<ItemBagBaseData>>(itemBagData).ToDictionary(key => key.ItemID, value => value);
                GameEntry. DataManager.TryGetValue(typeof(RingData).Name, out var ringData);
                var ringDict = TransObject<List<RingData>>(ringData).ToDictionary(key => key.Ring_ID, value => value);
                GameEntry. DataManager.TryGetValue(typeof(MagicWeaponData).Name, out var magicWeaponData);
                var magicWeaponDict = TransObject<List<MagicWeaponData>>(magicWeaponData).ToDictionary(key => key.Magic_ID, value => value);

                GameEntry. DataManager.TryAdd(itemBagDict);
                GameEntry. DataManager.TryAdd(ringDict);
                GameEntry. DataManager.TryAdd(magicWeaponDict);
                #endregion

                #region 战斗json


                GameEntry. DataManager.TryGetValue(typeof(GongFaBook).Name, out var gongfaBookSet);
                var gfbDict = TransObject<List<GongFaBook>>(gongfaBookSet).ToDictionary(key => key.BookID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(GongFa).Name, out var gongfaSet);
                var gfDict = TransObject<List<GongFa>>(gongfaSet).ToDictionary(key => key.Gongfa_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(MonsterDatas).Name, out var monsterDatasSet);
                var monsterDict = TransObject<List<MonsterDatas>>(monsterDatasSet).ToDictionary(key => key.Monster_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(SkillGongFaDatas).Name, out var skillGongFaDatasSet);
                var skillGongFaDict = TransObject<List<SkillGongFaDatas>>(skillGongFaDatasSet).ToDictionary(key => key.Skill_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(SkillMiShuDatas).Name, out var skillMiShuDatasSet);
                var skillMiShuDict = TransObject<List<SkillMiShuDatas>>(skillMiShuDatasSet).ToDictionary(key => key.Skill_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(DrugData).Name, out var drugSet);
                var drugDict = TransObject<List<DrugData>>(drugSet).ToDictionary(key => key.Drug_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(RunesData).Name, out var runesSet);
                var runesDict = TransObject<List<RunesData>>(runesSet).ToDictionary(key => key.Runes_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(MagicWeaponData).Name, out var magicSet);
                var magicDict = TransObject<List<MagicWeaponData>>(magicSet).ToDictionary(key => key.Magic_ID, value => value);

                GameEntry. DataManager.TryGetValue(typeof(BattleSkillData).Name, out var battleSkillSet);
                var battleSkillDict = TransObject<List<BattleSkillData>>(battleSkillSet).ToDictionary(key => key.id, value => value);

                GameEntry. DataManager.TryGetValue(typeof(BattleBuffData).Name, out var battleSBuffSet);
                var battleBuffDict = TransObject<List<BattleBuffData>>(battleSBuffSet).ToDictionary(key => key.id, value => value);
                #endregion
                //GameEntry.DataManager.TryAdd(passiveSkillsPetDict);

                GameEntry.DataManager.TryAdd(SecondaryJobDataDict);
                GameEntry.DataManager.TryAdd(DailyMsgDataDict);
                GameEntry.DataManager.TryAdd(passiveSkillsRoleDict);
                GameEntry.DataManager.TryAdd(passiveSkillsWeaponDict);
                GameEntry.DataManager.TryAdd(repairPuppetDataDict);
                GameEntry.DataManager.TryAdd(puppetAssembleDataDict);
                GameEntry.DataManager.TryAdd(puppetUnitParameterDict);
                GameEntry.DataManager.TryAdd(formulaGlobaIDDict);
                GameEntry.DataManager.TryAdd(formulaPuppetDataDict);
                GameEntry.DataManager.TryAdd(formulaForgeDataDict);
                GameEntry.DataManager.TryAdd(forgeParameterDict);
                GameEntry.DataManager.TryAdd(formulaDrugDataDict);
                GameEntry.DataManager.TryAdd(spiritRangeDataDict);
                GameEntry.DataManager.TryAdd(allianceDrugDataDict);
                GameEntry.DataManager.TryAdd(allianceConstructionDataDict);
                GameEntry.DataManager.TryAdd(allianceScripturesPlatformDataDict);
                GameEntry.DataManager.TryAdd(allianceJobDataDict);
                GameEntry.DataManager.TryAdd(creatAllianceDataDict);
                GameEntry.DataManager.TryAdd(roleAbilityPointDataDict);
                GameEntry.DataManager.TryAdd(flyMagicToolDict);
                GameEntry.DataManager.TryAdd(roleLevelDataDict);
                GameEntry. DataManager.TryAdd(formulaDataDict);
                GameEntry. DataManager.TryAdd(petSkillDataDict);
                GameEntry. DataManager.TryAdd(demonicSoulSkillPoolDict);
                GameEntry. DataManager.TryAdd(demonicSoulDataDict);
                GameEntry. DataManager.TryAdd(petSkillBookDataDict);
                GameEntry. DataManager.TryAdd(petAbilityPointDataDict);
                GameEntry. DataManager.TryAdd(mishuBookDict);
                GameEntry. DataManager.TryAdd(petLevelDataDict);
                GameEntry. DataManager.TryAdd(petAptitudeDataDict);
                GameEntry. DataManager.TryAdd(equipmentDataDict);
                GameEntry. DataManager.TryAdd(allianceSkillDatasDict);
                GameEntry. DataManager.TryAdd(roleStatusDatasDict);
                GameEntry. DataManager.TryAdd(mishuDataDict);
                GameEntry. DataManager.TryAdd(allianceSigninDataDict);
                GameEntry. DataManager.TryAdd(alliancelevledataDict);
                GameEntry. DataManager.TryAdd(demonDataDict);
                GameEntry. DataManager.TryAdd(bottleneckDataDict);
                GameEntry. DataManager.TryAdd(factionSkillDataDict);
                GameEntry. DataManager.TryAdd(factionitemDataDict);

                GameEntry. DataManager.TryAdd(gfbDict);
                GameEntry. DataManager.TryAdd(gfDict);
                GameEntry. DataManager.TryAdd(monsterDict);
                GameEntry. DataManager.TryAdd(skillGongFaDict);
                GameEntry. DataManager.TryAdd(skillMiShuDict);
                GameEntry. DataManager.TryAdd(drugDict);
                GameEntry. DataManager.TryAdd(runesDict);
                GameEntry. DataManager.TryAdd(magicDict);
                GameEntry. DataManager.TryAdd(battleSkillDict);
                GameEntry. DataManager.TryAdd(battleBuffDict);

                //GameEntry. DataManager.TryGetValue<Dictionary<int, MonsterDatas>>(out var set);
                //Utility.Debug.LogInfo("<DataManager> 测试 TryGetValue " + skillMiShuDict[21009].Skill_Describe);

                //Utility.Debug.LogInfo("<DataManager> 测试 ConvertData Step0211111111111====>" + monsterDict.Count);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }

           

            //Utility.Debug.LogInfo("<DataManager> 测试 ConvertData Step0211111111111");
        }

        /// <summary>
        /// 转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T TransObject <T>(string data)
        {
            return Utility.Json.ToObject<T>(data);
        } 


    }
}

