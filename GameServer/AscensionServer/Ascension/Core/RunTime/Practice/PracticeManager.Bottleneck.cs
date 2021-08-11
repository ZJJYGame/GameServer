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
            if (false/*GetPercent(rootPercentNum[0] / (float)100)*/)
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
    
        private bool GetPercent(float num)
        {
            Random random = new Random();
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
    }
}
