using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
   public partial class SecondaryJobManager
    {
        public void AddDemonical(int roleid, int soulid)
        {
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var demonicSoul = NHibernateQuerier.CriteriaSelectAsync<DemonicSoul>(nHCriteriaRole).Result;


            var indexDict = Utility.Json.ToObject<Dictionary<int, int>>(demonicSoul.DemonicSoulIndex);
            var demonicalDict = Utility.Json.ToObject<Dictionary<int, DemonicSoulEntity>>(demonicSoul.DemonicSouls);
            DemonicSoulEntity demonicSoulEntity = new DemonicSoulEntity();
            if (indexDict.ContainsKey(soulid))
            {
                demonicSoulEntity.UniqueID = int.Parse(soulid.ToString() + (indexDict[soulid] + 1));
                indexDict[soulid] += 1;
            }
            else
            {
                demonicSoulEntity.UniqueID = int.Parse(soulid.ToString() + 0);
                indexDict.Add(soulid, 0);
            }
            demonicSoulEntity.GlobalID = soulid;
            GameEntry.PetStatusManager.AddDemonicSoul(soulid, out var skillList);
            demonicSoulEntity.Skills = new List<int>();
            demonicSoulEntity.Skills = skillList;
            demonicalDict.Add(demonicSoulEntity.UniqueID, demonicSoulEntity);

            demonicSoul.DemonicSoulIndex = Utility.Json.ToJson(indexDict);
            demonicSoul.DemonicSouls = Utility.Json.ToJson(demonicalDict);

            NHibernateQuerier.Update(demonicSoul);

            DemonicSoulDTO demonicSoulDTO = new DemonicSoulDTO();
            demonicSoulDTO.RoleID = demonicSoul.RoleID;
            demonicSoulDTO.CompoundList = new List<int>();
            demonicSoulDTO.OperateType = DemonicSoulOperateType.Add;
            demonicSoulDTO.DemonicSouls = demonicalDict;
            Utility.Debug.LogInfo("yzqData添加妖灵精魄" + Utility.Json.ToJson(demonicSoulDTO));
            RoleStatusSuccessS2C(roleid,SecondaryJobOpCode.AddDemonicSoul, demonicSoulDTO);
            Utility.Debug.LogInfo("yzqData添加妖灵精魄" + Utility.Json.ToJson(demonicSoul));
            InventoryManager.AddNewItem(roleid, demonicSoulEntity.UniqueID, 1);
        }

        public async void CompoundDemonical(List<int> soulList, int roleid)
        {
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var demonicSoul = NHibernateQuerier.CriteriaSelectAsync<DemonicSoul>(nHCriteriaRole).Result;


            GameEntry.DataManager.TryGetValue<Dictionary<int, DemonicSoulData>>(out var demonicSoulData);

            var demonicalDict = Utility.Json.ToObject<Dictionary<int, DemonicSoulEntity>>(demonicSoul.DemonicSouls);

            var ringObj =ReferencePool.Accquire<RingDTO>();

            var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteriaRole);
            var nHCriteriaRingID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);
            if (soulList.Count == 0)
            {
                RoleStatusFailS2C(roleid,SecondaryJobOpCode.CompoundDemonicSoul);
                return;
            }

            for (int i = 0; i < soulList.Count; i++)
            {
                if (demonicalDict[soulList[i]].GlobalID != demonicalDict[soulList[0]].GlobalID)
                {
                    RoleStatusFailS2C(roleid, SecondaryJobOpCode.CompoundDemonicSoul);
                    return;
                }
                if (!InventoryManager.VerifyIsExist(soulList[i], nHCriteriaRingID))
                {
                    RoleStatusFailS2C(roleid, SecondaryJobOpCode.CompoundDemonicSoul);
                    return;
                }
            }

            if (!demonicSoulData[demonicalDict[soulList[0]].GlobalID].MergeNumber.Contains(soulList.Count))
            {
                RoleStatusFailS2C(roleid, SecondaryJobOpCode.CompoundDemonicSoul);
                return;
            }

            Random random = new Random();
            if ((demonicSoulData[demonicalDict[soulList[0]].GlobalID].MergeSuccessRate * soulList.Count) < random.Next(0, 101))
            {
                RoleStatusFailS2C(roleid, SecondaryJobOpCode.CompoundDemonicSoul);
            }
            else
            {
                AddDemonical(roleid, demonicSoulData[demonicalDict[soulList[0]].GlobalID].MergeDemonicSoulID);
            }

            for (int i = 0; i < soulList.Count; i++)
            {
                InventoryManager.Remove(roleid, soulList[i]);
                demonicalDict.Remove(soulList[i]);
            }
            demonicSoul.DemonicSouls = Utility.Json.ToJson(demonicalDict);
            await NHibernateQuerier.UpdateAsync(demonicSoul);
        }

        public void GetDemonicSoul(int roleid)
        {
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleid);
            var demonicSoul = NHibernateQuerier.CriteriaSelectAsync<DemonicSoul>(nHCriteriaRole).Result;

            var demonicalDict = Utility.Json.ToObject<Dictionary<int, DemonicSoulEntity>>(demonicSoul.DemonicSouls);
            DemonicSoulDTO demonicSoulDTO = new DemonicSoulDTO();
            demonicSoulDTO.RoleID = demonicSoul.RoleID;
            demonicSoulDTO.CompoundList = new List<int>();
            demonicSoulDTO.OperateType = DemonicSoulOperateType.Get;
            demonicSoulDTO.DemonicSouls = demonicalDict;
            Utility.Debug.LogInfo("yzqData添加妖灵精魄" + Utility.Json.ToJson(demonicSoulDTO));
            RoleStatusSuccessS2C(roleid,SecondaryJobOpCode.GetDemonicSoul, demonicSoulDTO);
        }

    }
}
