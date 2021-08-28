using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cosmos;

namespace AscensionServer
{
    /// <summary>
    /// LevelResEntity的数据代理；
    /// </summary>
    public class LevelResEntityDataProxy
    {
        public Dictionary<int, FixCollectable> CollectableDict { get { return collectableDict; } }
        public Dictionary<int, FixCombatable> CombatableDict { get { return combatableDict; } }
        /// <summary>
        /// index---collectable
        /// </summary>
        Dictionary<int, FixCollectable> collectableDict;
        /// <summary>
        /// index---collectable
        /// </summary>
        Dictionary<int, FixCollectable> uncollectableDict;


        /// <summary>
        /// index---combatable
        /// </summary>
        Dictionary<int, FixCombatable> combatableDict;
        /// <summary>
        /// index---combatable
        /// </summary>
        Dictionary<int, FixCombatable> uncombatableDict;
        /// <summary>
        /// index---combatable
        /// </summary>
        Dictionary<int, FixCombatable> pendingCombatableDict;

        public LevelResEntityDataProxy()
        {
            collectableDict = new Dictionary<int, FixCollectable>();
            uncollectableDict = new Dictionary<int, FixCollectable>();
            combatableDict = new Dictionary<int, FixCombatable>();
            uncombatableDict = new Dictionary<int, FixCombatable>();
            pendingCombatableDict = new Dictionary<int, FixCombatable>();
        }
        /// <summary>
        /// 初始化levelEntity的数据；
        /// </summary>
        /// <param name="resSpawnInfoData"></param>
        public void InitEntityRes(MapResSpanwInfoData resSpawnInfoData)
        {
            Random random = new Random();
            var dict = resSpawnInfoData.MapResSpawnInfoDict;
            foreach (var res in dict)

            {
                switch (res.Value.ResType)
                {
                    case LevelResType.Collectable:
                        {
                            var fc = new FixCollectable();
                            fc.Id = res.Value.ResId;
                            fc.CollectableDict = new Dictionary<int, FixResObject>();
                            var length = res.Value.ResAmount;
                            for (int i = 0; i < length; i++)
                            {
                                var resObject = SpawnResObject(random, res.Value, i);
                                fc.CollectableDict.Add(i, resObject);
                            }
                            collectableDict.Add(res.Key, fc);
                        }
                        break;
                    case LevelResType.Combatable:
                        {
                            var fc = new FixCombatable();
                            fc.Id = res.Value.ResId;
                            fc.CombatableDict = new Dictionary<int, FixResObject>();
                            var length = res.Value.ResAmount;
                            for (int i = 0; i < length; i++)
                            {
                                var resObject = SpawnResObject(random, res.Value, i);
                                fc.CombatableDict.Add(i, resObject);
                            }
                            combatableDict.Add(res.Key, fc);
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// 采集；
        /// </summary>
        /// <param name="index">资源生成时的id</param>
        /// <param name="gId">全局id</param>
        /// <param name="eleId">元素id</param>
        /// <returns>是否采集成功</returns>
        public bool Gather(int index, int gId, int eleId)
        {
            if (collectableDict.TryGetValue(index, out var col))
            {
                if (col.Id != gId)
                {
                    return false;
                }
                FixCollectable gatherObj = null; ;
                if (!uncollectableDict.ContainsKey(index))
                {
                    gatherObj = new FixCollectable();
                    gatherObj.Id = gId;
                    gatherObj.CollectableDict = new Dictionary<int, FixResObject>();
                    uncollectableDict.Add(index, gatherObj);
                }
                else
                {
                    uncollectableDict.TryGetValue(index, out var fc);
                    if (fc.Id != gId)
                        return false;
                    gatherObj = fc;
                }
                if (col.CollectableDict.Remove(eleId, out var removeEle))
                {
                    if (gatherObj.CollectableDict.TryAdd(eleId, removeEle))
                    {
                        removeEle.Occupied = true;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool Combat(int index, int gId, int eleId)
        {
            if (combatableDict.TryGetValue(index, out var combatable))
            {
                if (combatable.Id != gId)
                {
                    return false;
                }
                FixCombatable combatObj = null; ;
                //若不在不可战斗缓存中；
                if (!uncombatableDict.ContainsKey(index))
                {
                    //将对象移至挂起缓存中；
                    combatObj = new FixCombatable();
                    combatObj.Id = gId;
                    combatObj.CombatableDict = new Dictionary<int, FixResObject>();
                    pendingCombatableDict.Add(index, combatObj);
                }
                else
                {
                    //若在不可战斗缓存中，则可能存在于挂起或者未被挂起的缓存中；
                    var isPending = pendingCombatableDict.TryGetValue(index, out var pending);
                    combatObj = pending;
                    if (isPending)
                    {
                        if (combatObj.Id != gId)
                            return false;
                    }
                }
                if (combatable.CombatableDict.Remove(gId, out var fixResObject))
                {
                    if (combatObj.CombatableDict.TryAdd(eleId, fixResObject))
                    {
                        fixResObject.Occupied = true;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool OnCombatSuccess(int index, int gId, int eleId)
        {
            if (!pendingCombatableDict.TryGetValue(index, out var pendingObj))
                return false;
            if (pendingObj.Id != gId)
                return false;
            if (!pendingObj.CombatableDict.Remove(eleId, out var pendingElet))
                return false;
            FixCombatable combatObj = null;
            //战斗成功，将挂起的对象转移到不可战斗缓存中；
            if (!uncombatableDict.ContainsKey(index))
            {
                combatObj = new FixCombatable();
                combatObj.CombatableDict = new Dictionary<int, FixResObject>();
                combatObj.Id = gId;
                combatObj.CombatableDict.Add(eleId, pendingElet);
                uncombatableDict.Add(index, combatObj);
            }
            else
            {
                var temp = uncombatableDict[index];
                if (temp.Id != gId)
                    return false;
                combatObj = temp;
            }
            if(pendingObj.CombatableDict.Remove(eleId,out var pendingEle))
            {
                pendingCombatableDict.Remove(index);
                if( combatObj.CombatableDict.TryAdd(eleId, pendingEle))
                {
                    pendingEle.Occupied = true;
                    return true;
                }
            }
            return false;
        }
        public bool OnCombatFailure(int index, int gId, int eleId)
        {
            if (!pendingCombatableDict.TryGetValue(index, out var pendingObj))
                return false;
            if (pendingObj.Id != gId)
                return false;
            if (!pendingObj.CombatableDict.Remove(eleId, out var pendingElet))
                return false;
            FixCombatable combatObj = null;
            //战斗失败，将挂起的对象转移到可战斗缓存中；
            if (!combatableDict.ContainsKey(index))
            {
                combatObj = new FixCombatable();
                combatObj.CombatableDict = new Dictionary<int, FixResObject>();
                combatObj.Id = gId;
                combatObj.CombatableDict.Add(eleId, pendingElet);
                uncombatableDict.Add(index, combatObj);
            }
            else
            {
                var temp= combatableDict[index];
                if (temp.Id != gId)
                    return false;
                combatObj = temp;
            }
            if (pendingObj.CombatableDict.Remove(eleId, out var pendingEle))
            {
                pendingCombatableDict.Remove(index);
                if (combatObj.CombatableDict.TryAdd(eleId, pendingEle))
                {
                    pendingEle.Occupied = false;
                    return true;
                }
            }
            return false;
        }
        FixResObject SpawnResObject(Random random, MapResSpawnInfo spawnInfo, int index)
        {
            var resObject = new FixResObject();
            resObject.Id = index;
            resObject.Occupied = false;
            var vec = spawnInfo.ResSpawnPositon.GetVector();
            var xSign = Sign();
            var xOffset = random.Next(0, spawnInfo.ResSpawnRange);
            vec.x += xSign == true ? xOffset : -xOffset;

            var zSign = Sign();
            var zOffset = random.Next(0, spawnInfo.ResSpawnRange);
            vec.z += zSign == true ? zOffset : -zOffset;

            resObject.FixTransform = new FixTransform(vec, Vector3.zero, Vector3.one);
            return resObject;
        }
        bool IsOdd(int n)
        {
            return Convert.ToBoolean(n % 2);
        }
        bool Sign()
        {
            var result = Utility.Algorithm.RandomRange(0, 200);
            return IsOdd(result);
        }
    }
}
