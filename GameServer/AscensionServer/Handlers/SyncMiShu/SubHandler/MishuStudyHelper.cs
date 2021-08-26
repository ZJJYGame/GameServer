//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Cosmos;
//using AscensionProtocol.DTO;

//namespace AscensionServer
//{
//   public class MishuStudyHelper
//    {
//        public static bool AddMishuJuge(int mishuBookID, RoleDTO roleDTO, out MiShuDTO misuhTemp)
//        {
//            GameEntry. DataManager.TryGetValue<Dictionary<int, MishuBook>>(out var mishubookDict);
//            misuhTemp =ReferencePool.Accquire<MiShuDTO>();
//            mishubookDict.TryGetValue(mishuBookID, out var mishuBook);
//            if (!Utility.Json.ToObject<List<int>>(roleDTO.RoleRoot).Contains(mishuBook.BookProperty) && roleDTO.RoleLevel < mishuBook.NeedRoleLevel)
//            {
//                return false;
//            }
//            misuhTemp.MiShuID = mishuBook.MishuID;
//            misuhTemp.MiShuAdventureSkill = new List<int>();
//            misuhTemp.MiShuSkillArry = new List<int>();
//            GameEntry. DataManager.TryGetValue<Dictionary<int, MiShuData>>(out var mishuDict);
//            for (int i = 0; i < mishuDict[misuhTemp.MiShuID].mishuSkillDatas[0].SkillArrayOne.Count; i++)
//            {
//                misuhTemp.MiShuSkillArry.Add(mishuDict[misuhTemp.MiShuID].mishuSkillDatas[0].SkillArrayOne[0]);
//            }
//            for (int i = 0; i < mishuDict[misuhTemp.MiShuID].mishuSkillDatas[0].SkillArrayTwo.Count; i++)
//            {
//                misuhTemp.MiShuAdventureSkill.Add(mishuDict[misuhTemp.MiShuID].mishuSkillDatas[0].SkillArrayTwo[0]);
//            }

//            Utility.Debug.LogInfo("秘术检测yzqData4" + misuhTemp.MiShuSkillArry.Count);
//            //misuhTemp.CultivationMethodLevelSkillArray.AddRange(gongfaDict[misuhTemp.CultivationMethodID].Skill_One);

//            return true;
//        }
//    }
//}


