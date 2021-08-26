using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using AscensionServer.Model;
using NHibernate.Linq.Clauses;
using AscensionProtocol.DTO;
using Renci.SshNet.Security;
using Cosmos;

namespace AscensionServer
{
    [Module]
    public partial class InventoryManager: Module, IInventoryManager
    {

        /// <summary>
        /// 映射T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nHCriteria"></param>
        /// <returns></returns>
        public static T CriteriaSelectMethod<T>(NHCriteria nHCriteria)
        {
            return NHibernateQuerier.CriteriaSelect<T>(nHCriteria);
        }
        /// <summary>
        /// 返回给客端户的参数
        /// </summary>
        /// <param name="ringServerArray"></param>
        /// <returns></returns>
        public static Dictionary<byte, object> ServerToClientParams(Ring ringServerArray)
        {
            ///返回给客户端我看到了，这个是需要序列化的，我操作一下
            Dictionary<byte, object> subResponseParametersDict = new Dictionary<byte, object>();
            subResponseParametersDict.Add((byte)ParameterCode.Inventory, ringServerArray.RingItems);
            subResponseParametersDict.Add((byte)ParameterCode.RoleRingMagic, ringServerArray.RingMagicDictServer);
            subResponseParametersDict.Add((byte)ParameterCode.RoleTemInventory, ringServerArray.RingAdorn);
            return subResponseParametersDict;
        }
        /// <summary>
        /// 需要count
        /// </summary>
        /// <param name="_ItemHeld"></param>
        /// <param name="_ItemMax"></param>
        /// <returns></returns>
        static int NumberCountMethod(int _ItemHeld, int _ItemMax)
        {
            return _ItemHeld / _ItemMax;
        }
        /// <summary>
        /// 剩余的个数
        /// </summary>
        /// <param name="_ItemHeld"></param>
        /// <param name="_ItemMax"></param>
        /// <returns></returns>
        static int HeldMethod(int _ItemHeld, int _ItemMax)
        {
            return _ItemHeld % _ItemMax;
        }
        /// <summary>
        /// 验证是否存在
        /// </summary>
        public static bool VerifyIsExist(int ItemId, NHCriteria  nHCriteria)
        {
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            var ServerDict = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingItems);
            var ServerDictAdorn = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingAdorn);
            if (ServerDict.ContainsKey(ItemId))
                return true;
            return false;
        }
        /// <summary>
        /// 驗證物品及數量
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="num"></param>
        /// <param name="nHCriteria"></param>
        /// <returns></returns>
        public static bool VerifyIsExist(int ItemId,int num ,int ID)
        {
            NHCriteria nHCriteria =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ID);
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            var ServerDict = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingItems);
            var ServerDictAdorn = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingAdorn);
            if (ServerDict.TryGetValue(ItemId, out var itemsDTO))
            {
                if (itemsDTO.RingItemCount >= num)
                    return true;
                else
                    return false;
            }else
            return false;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="InventoryObj"></param>
        /// <param name="nHCriteria"></param>
        public static void AddDataCmdS2C(int roleId, RingDTO InventoryObj, NHCriteria nHCriteria)
        {
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            var ServerDict = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingItems);
            var ServerDictAdorn = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingAdorn);

            foreach (var client_p in InventoryObj.RingItems)
            {
                if (!ServerDict.ContainsKey(client_p.Key) && !ServerDictAdorn.ContainsKey(client_p.Key))
                {
                    if (client_p.Value.RingItemCount > client_p.Value.RingItemMax)
                    {
                        int held = client_p.Value.RingItemCount;
                        var remainder = HeldMethod(client_p.Value.RingItemCount, client_p.Value.RingItemMax);
                        int numberCount = remainder == 0 || client_p.Value.RingItemMax == 1 ? NumberCountMethod(client_p.Value.RingItemCount, client_p.Value.RingItemMax) : NumberCountMethod(client_p.Value.RingItemCount, client_p.Value.RingItemMax) + 1;
                        for (int o = 0; o < numberCount; o++)
                        {
                            int NowID = client_p.Key;
                            while (true)
                            {
                                var randomNumer = new Random().Next(1000, 3000);

                                if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                    break;
                                NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                            }
                            int numbCount = held > client_p.Value.RingItemMax ? client_p.Value.RingItemMax : remainder;
                            RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = numbCount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn };
                            var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                            if (firstKeyDefault != 0)
                            {
                                ServerDict[firstKeyDefault] = Items;
                                ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? NowID : k.Key, k => k.Value);
                            }
                            else
                                ServerDict.Add(NowID, Items);
                            if (client_p.Value.RingItemMax == 1 || remainder == 0)
                                continue;
                            held -= client_p.Value.RingItemMax;
                        }
                    }
                    else
                    {
                        #region 重复

                        ///TODO
                        int serverItemKey = ServerDict.Keys.ToList().Find(z => Int32.Parse(z.ToString().Substring(0, 5)) == client_p.Key && ServerDict[z].RingItemCount < ServerDict[z].RingItemMax);
                        if (serverItemKey != 0)
                        {
                            if (ServerDict[serverItemKey].RingItemCount + client_p.Value.RingItemCount <= ServerDict[serverItemKey].RingItemMax)
                            {
                                var severValue = ServerDict[serverItemKey];
                                if (client_p.Value.RingItemCount > 0)
                                    severValue.RingItemCount += client_p.Value.RingItemCount;
                                if (severValue.RingItemAdorn != client_p.Value.RingItemAdorn)
                                    severValue.RingItemAdorn = client_p.Value.RingItemAdorn;
                                if (severValue.RingItemTime != client_p.Value.RingItemTime)
                                    severValue.RingItemTime = client_p.Value.RingItemTime;
                                if (severValue.RingItemMax != client_p.Value.RingItemMax)
                                    severValue.RingItemMax = client_p.Value.RingItemMax;
                                if (severValue.RingItemType != client_p.Value.RingItemType)
                                    severValue.RingItemType = client_p.Value.RingItemType;
                            }
                            else
                            {

                                var severValue = ServerDict[serverItemKey];
                                int Amount = (severValue.RingItemCount + client_p.Value.RingItemCount) - client_p.Value.RingItemMax;
                                severValue.RingItemCount = client_p.Value.RingItemMax;
                                if (severValue.RingItemAdorn != client_p.Value.RingItemAdorn)
                                    severValue.RingItemAdorn = client_p.Value.RingItemAdorn;
                                if (severValue.RingItemTime != client_p.Value.RingItemTime)
                                    severValue.RingItemTime = client_p.Value.RingItemTime;
                                if (severValue.RingItemMax != client_p.Value.RingItemMax)
                                    severValue.RingItemMax = client_p.Value.RingItemMax;
                                if (severValue.RingItemType != client_p.Value.RingItemType)
                                    severValue.RingItemType = client_p.Value.RingItemType;
                                Utility.Debug.LogInfo("<Amount>" + Amount);

                                if (Amount > client_p.Value.RingItemMax)
                                {
                                    int held = Amount;
                                    var remainder = HeldMethod(Amount, client_p.Value.RingItemMax);
                                    int numberCount = remainder == 0 || client_p.Value.RingItemMax == 1 ? NumberCountMethod(Amount, client_p.Value.RingItemMax) : NumberCountMethod(Amount, client_p.Value.RingItemMax) + 1;
                                    for (int o = 0; o < numberCount; o++)
                                    {
                                        int NowID = client_p.Key;
                                        while (true)
                                        {
                                            var randomNumer = new Random().Next(1000, 3000);
                                            if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                                break;
                                            NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                        }
                                        int numbCount = held > client_p.Value.RingItemMax ? client_p.Value.RingItemMax : remainder;
                                        RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = numbCount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn, RingItemType = client_p.Value.RingItemType };
                                        var firstKeyTrueDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                                        if (firstKeyTrueDefault != 0)
                                        {
                                            ServerDict[firstKeyTrueDefault] = Items;
                                            ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyTrueDefault ? NowID : k.Key, k => k.Value);
                                        }
                                        else
                                            ServerDict.Add(NowID, Items);
                                        if (client_p.Value.RingItemMax == 1 || remainder == 0)
                                            continue;
                                        held -= client_p.Value.RingItemMax;
                                    }
                                }
                                else
                                {
                                    int NowID = client_p.Key;
                                    while (true)
                                    {
                                        var randomNumer = new Random().Next(1000, 3000);
                                        if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                            break;
                                        NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                    }
                                    Utility.Debug.LogInfo($"<NowID>{NowID}");

                                    RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = Amount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn, RingItemType = client_p.Value.RingItemType };
                                    var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                                    if (firstKeyDefault != 0)
                                    {
                                        ServerDict[firstKeyDefault] = Items;
                                        ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? NowID : k.Key, k => k.Value);
                                    }
                                    else
                                        ServerDict.Add(NowID, Items);
                                }

                            }

                        }
                        else
                        {
                            var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;
                            if (firstKeyDefault != 0)
                            {
                                ServerDict[firstKeyDefault] = client_p.Value;
                                ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? client_p.Key : k.Key, k => k.Value);
                            }
                            else
                                ServerDict.Add(client_p.Key, client_p.Value);
                        }
                    }
                    #endregion

                }
                else
                {
                    int serverItemKey = ServerDict.Keys.ToList().Find(z => Int32.Parse(z.ToString().Substring(0, 5)) == client_p.Key && ServerDict[z].RingItemCount < ServerDict[z].RingItemMax);//(q => Int32.Parse(q.Key.ToString().Substring(0, 5)) == client_p.Key).Key;

                    Utility.Debug.LogInfo("<keylen>" + serverItemKey);

                    if (serverItemKey == 0)
                    {
                        if (client_p.Value.RingItemCount > client_p.Value.RingItemMax)
                        {
                            int held = client_p.Value.RingItemCount;
                            var remainder = HeldMethod(client_p.Value.RingItemCount, client_p.Value.RingItemMax);
                            int numberCount = remainder == 0 || client_p.Value.RingItemMax == 1 ? NumberCountMethod(client_p.Value.RingItemCount, client_p.Value.RingItemMax) : NumberCountMethod(client_p.Value.RingItemCount, client_p.Value.RingItemMax) + 1;
                            for (int o = 0; o < numberCount; o++)
                            {
                                int NowID = 0;
                                while (true)
                                {
                                    var randomNumer = new Random().Next(1000, 3000);
                                    NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                    if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                        break;
                                }
                                int numbCount = held > client_p.Value.RingItemMax ? client_p.Value.RingItemMax : remainder;
                                RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = numbCount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn, RingItemType = client_p.Value.RingItemType };
                                var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                                if (firstKeyDefault != 0)
                                {
                                    ServerDict[firstKeyDefault] = Items;
                                    ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? NowID : k.Key, k => k.Value);
                                }
                                else
                                    ServerDict.Add(NowID, Items);
                                if (client_p.Value.RingItemMax == 1 || remainder == 0)
                                    continue;
                                held -= client_p.Value.RingItemMax;
                            }
                        }
                        else
                        {
                            int NowID = 0;
                            while (true)
                            {
                                var randomNumer = new Random().Next(1000, 3000);
                                NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                    break;
                            }
                            RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = client_p.Value.RingItemCount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn, RingItemType = client_p.Value.RingItemType };
                            var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                            if (firstKeyDefault != 0)
                            {
                                ServerDict[firstKeyDefault] = Items;
                                ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? NowID : k.Key, k => k.Value);
                            }
                            else
                                ServerDict.Add(NowID, Items);
                        }
                    }
                    else
                    {
                        if (ServerDict[serverItemKey].RingItemCount + client_p.Value.RingItemCount <= ServerDict[serverItemKey].RingItemMax)
                        {
                            var severValue = ServerDict[serverItemKey];
                            if (client_p.Value.RingItemCount > 0)
                                severValue.RingItemCount += client_p.Value.RingItemCount;
                            if (severValue.RingItemAdorn != client_p.Value.RingItemAdorn)
                                severValue.RingItemAdorn = client_p.Value.RingItemAdorn;
                            if (severValue.RingItemTime != client_p.Value.RingItemTime)
                                severValue.RingItemTime = client_p.Value.RingItemTime;
                            if (severValue.RingItemMax != client_p.Value.RingItemMax)
                                severValue.RingItemMax = client_p.Value.RingItemMax;
                            if (severValue.RingItemType != client_p.Value.RingItemType)
                                severValue.RingItemType = client_p.Value.RingItemType;
                        }
                        else if (ServerDict[serverItemKey].RingItemCount + client_p.Value.RingItemCount > ServerDict[serverItemKey].RingItemMax)
                        {
                            var severValue = ServerDict[serverItemKey];
                            int Amount = (severValue.RingItemCount + client_p.Value.RingItemCount) - client_p.Value.RingItemMax;
                            severValue.RingItemCount = client_p.Value.RingItemMax;
                            if (severValue.RingItemAdorn != client_p.Value.RingItemAdorn)
                                severValue.RingItemAdorn = client_p.Value.RingItemAdorn;
                            if (severValue.RingItemTime != client_p.Value.RingItemTime)
                                severValue.RingItemTime = client_p.Value.RingItemTime;
                            if (severValue.RingItemMax != client_p.Value.RingItemMax)
                                severValue.RingItemMax = client_p.Value.RingItemMax;
                            if (severValue.RingItemType != client_p.Value.RingItemType)
                                severValue.RingItemType = client_p.Value.RingItemType;
                            Utility.Debug.LogInfo("<Amount>" + Amount);

                            if (Amount > client_p.Value.RingItemMax)
                            {
                                int held = Amount;
                                var remainder = HeldMethod(Amount, client_p.Value.RingItemMax);
                                int numberCount = remainder == 0 || client_p.Value.RingItemMax == 1 ? NumberCountMethod(Amount, client_p.Value.RingItemMax) : NumberCountMethod(Amount, client_p.Value.RingItemMax) + 1;
                                for (int o = 0; o < numberCount; o++)
                                {
                                    int NowID = 0;
                                    while (true)
                                    {
                                        var randomNumer = new Random().Next(1000, 3000);
                                        NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                        if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                            break;
                                    }
                                    int numbCount = held > client_p.Value.RingItemMax ? client_p.Value.RingItemMax : remainder;
                                    RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = numbCount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn, RingItemType = client_p.Value.RingItemType };
                                    var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                                    if (firstKeyDefault != 0)
                                    {
                                        ServerDict[firstKeyDefault] = Items;
                                        ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? NowID : k.Key, k => k.Value);
                                    }
                                    else
                                        ServerDict.Add(NowID, Items);
                                    if (client_p.Value.RingItemMax == 1 || remainder == 0)
                                        continue;
                                    held -= client_p.Value.RingItemMax;
                                }
                            }
                            else
                            {
                                int NowID = 0;
                                while (true)
                                {
                                    var randomNumer = new Random().Next(1000, 3000);
                                    NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                    if (!ServerDict.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                        break;
                                }
                                RingItemsDTO Items = new RingItemsDTO() { RingItemTime = client_p.Value.RingItemTime, RingItemCount = Amount, RingItemMax = client_p.Value.RingItemMax, RingItemAdorn = client_p.Value.RingItemAdorn, RingItemType = client_p.Value.RingItemType };
                                var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;

                                if (firstKeyDefault != 0)
                                {
                                    ServerDict[firstKeyDefault] = Items;
                                    ServerDict = ServerDict.ToDictionary(k => k.Key == firstKeyDefault ? NowID : k.Key, k => k.Value);
                                }
                                else
                                    ServerDict.Add(NowID, Items);
                            }
                        }
                    }
                }
                NHibernateQuerier.Update(new Ring() { ID = ringServerArray.ID, RingId = ringServerArray.RingId, RingItems = Utility.Json.ToJson(ServerDict), RingMagicDictServer = ringServerArray.RingMagicDictServer, RingAdorn = Utility.Json.ToJson(ServerDictAdorn) });
                //GetDataCmdS2C(roleId, InventoryObj, nHCriteria);
                SendDataCmdS2C(roleId, nHCriteria, InventoryInstructions.AddData);
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="InventoryObj"></param>
        /// <param name="nHCriteria"></param>
        public static void UdpateCmdS2C(int roleId, RingDTO InventoryObj, NHCriteria nHCriteria)
        {
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            var ServerDic = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingItems);
            var ServerMagicDic = Utility.Json.ToObject<Dictionary<int, int>>(ringServerArray.RingMagicDictServer);
            var ServerDictAdorn = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingAdorn);

            foreach (var client_p in InventoryObj.RingItems)
            {
                Utility.Debug.LogInfo("<client_pkey>" + client_p.Key);
                RingItemsDTO serverData = null;
                if (ServerDic.TryGetValue(client_p.Key, out serverData) || ServerDictAdorn.TryGetValue(client_p.Key, out serverData))
                {
                    if (serverData.RingItemCount >= client_p.Value.RingItemCount)
                    {
                        serverData.RingItemCount -= client_p.Value.RingItemCount;
                        serverData.RingItemTime = serverData.RingItemTime;
                        serverData.RingItemAdorn = client_p.Value.RingItemAdorn;
                        serverData.RingItemMax = serverData.RingItemMax;
                        serverData.RingItemType = client_p.Value.RingItemType;
                        if (client_p.Value.RingItemCount == serverData.RingItemMax)
                            serverData.RingItemCount = 0;
                        #region onb

                        if (client_p.Value.RingItemAdorn == "2" || client_p.Value.RingItemAdorn == "1" || ServerDictAdorn.ContainsKey(client_p.Key))
                        {
                            //是否同一id
                            var firstAdorn = ServerDictAdorn.FirstOrDefault(q => Int32.Parse(q.Key.ToString().Substring(0, 5)) == Int32.Parse(client_p.Key.ToString().Substring(0, 5))).Key;
                            //是否同一类型
                            var firstType = ServerDictAdorn.FirstOrDefault(q => q.Value.RingItemType == client_p.Value.RingItemType).Key;
                            //是否有空位置
                            var firstKeyDefault = ServerDic.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;
                            Utility.Debug.LogInfo("<firstAdorn>" + firstAdorn);
                            Utility.Debug.LogInfo("<firstType>" + firstType);
                            Utility.Debug.LogInfo("<firstKeyDefault>" + firstKeyDefault);
                            if (firstAdorn == 0 && (firstType == 0 || client_p.Value.RingItemAdorn == "2") && !ServerDictAdorn.ContainsKey(client_p.Key))
                            {
                                if (ServerDic.ContainsKey(client_p.Key))
                                {
                                    Utility.Debug.LogInfo("<NowID>" + client_p.Key);
                                    Utility.Debug.LogInfo("<NowID>" + client_p.Key);
                                    while (true)
                                    {
                                        //Utility.Debug.LogInfo("<NowIDtrue>" + client_p.Key);
                                        int NowID = 0;
                                        var randomNumer = new Random().Next(3001, 4000);
                                        Utility.Debug.LogInfo("<randomNumer>" + randomNumer);
                                        if (client_p.Key.ToString().Length == 6 || client_p.Key.ToString().Length == 7 || client_p.Key.ToString().Length == 8)
                                            NowID = Int32.Parse(client_p.Key.ToString() + randomNumer.ToString().Substring(0, 3));
                                        else if (client_p.Key.ToString().Length > 8)
                                            NowID = client_p.Key + randomNumer;
                                        else
                                            NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                        Utility.Debug.LogInfo("<randomNumer>" + NowID);
                                        if (!ServerDic.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                        {
                                            //Utility.Debug.LogInfo("<RingItemAdorn>" + client_p.Value.RingItemAdorn);
                                            ServerDic = ServerDic.ToDictionary(k => k.Key == client_p.Key ? NowID : k.Key, k => k.Value);
                                            //Utility.Debug.LogInfo("<RingItemAdorn>" + ServerDic[NowID].RingItemAdorn);
                                            ServerDic[NowID].RingItemAdorn = "1";
                                            break;
                                        }
                                    }
                                    if (!ServerDictAdorn.ContainsKey(client_p.Key))
                                        ServerDictAdorn.Add(client_p.Key, serverData);
                                }

                            }
                            else if (firstAdorn != 0 && firstType != 0 && !ServerDictAdorn.ContainsKey(client_p.Key)) //(ServerDictAdorn.Keys.ToList().Find(x =>Int32.Parse(x.ToString().Substring(0, 5)) == Int32.Parse(client_p.Key.ToString().Substring(0, 5))) != 0)) ///存在同一位置 同一个id武器
                            {
                                Utility.Debug.LogInfo("<存在同一位置 同一个id武器>" + firstType);
                                if (firstKeyDefault != 0)
                                {
                                    //ServerDic = ServerDic.ToDictionary(k => k.Key == firstKeyDefault ? firstType : k.Key, k => k.Value);
                                    if (ServerDic.ContainsKey(client_p.Key))
                                    {
                                        Utility.Debug.LogInfo("<NowID>" + client_p.Key);
                                        int NowID = 0;
                                        while (true)
                                        {
                                            Utility.Debug.LogInfo("<NowIDtrue>" + client_p.Key);
                                            var randomNumer = new Random().Next(3001, 4000);
                                            Utility.Debug.LogInfo("<randomNumer>" + randomNumer);
                                            if (client_p.Key.ToString().Length == 6 || client_p.Key.ToString().Length == 7 || client_p.Key.ToString().Length == 8)
                                                NowID = Int32.Parse(client_p.Key.ToString() + randomNumer.ToString().Substring(0, 3));
                                            else if (client_p.Key.ToString().Length > 8)
                                                NowID = client_p.Key + randomNumer;
                                            else
                                                NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                            if (!ServerDic.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                            {
                                                ServerDic = ServerDic.ToDictionary(k => k.Key == client_p.Key ? NowID : k.Key, k => k.Value);
                                                break;
                                            }
                                        }
                                        ServerDic = ServerDic.ToDictionary(k => k.Key == NowID ? firstType : k.Key, k => k.Value);
                                    }
                                    else
                                        ServerDic = ServerDic.ToDictionary(k => k.Key == firstKeyDefault ? firstType : k.Key, k => k.Value);
                                    //ServerDic = ServerDic.ToDictionary(k => k.Key == firstKeyDefault ? firstType : k.Key, k => k.Value);
                                    ServerDictAdorn = ServerDictAdorn.ToDictionary(k => k.Key == firstType ? client_p.Key : k.Key, k => k.Value);
                                    ServerDic[firstType].RingItemAdorn = "0";
                                }
                            }
                            else if (firstAdorn == 0 && firstType != 0)//存在同一位置不同id武器
                            {
                                Utility.Debug.LogInfo("<存在同一位置不同id武器>" + firstAdorn);
                                if (firstKeyDefault != 0)
                                {
                                    if (ServerDic.ContainsKey(client_p.Key))
                                    {
                                        Utility.Debug.LogInfo("<NowID>" + client_p.Key);
                                        int NowID = 0;
                                        while (true)
                                        {
                                            Utility.Debug.LogInfo("<NowIDtrue>" + client_p.Key);
                                            var randomNumer = new Random().Next(3001, 4000);
                                            Utility.Debug.LogInfo("<randomNumer>" + randomNumer);
                                            if (client_p.Key.ToString().Length > 8)
                                                NowID = client_p.Key + randomNumer;
                                            else
                                                NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                            if (!ServerDic.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                            {
                                                ServerDic = ServerDic.ToDictionary(k => k.Key == client_p.Key ? NowID : k.Key, k => k.Value);
                                                break;
                                            }
                                        }
                                        ServerDic = ServerDic.ToDictionary(k => k.Key == NowID ? firstType : k.Key, k => k.Value);
                                    }
                                    else
                                        ServerDic = ServerDic.ToDictionary(k => k.Key == firstKeyDefault ? firstType : k.Key, k => k.Value);
                                    //ServerDic = ServerDic.ToDictionary(k => k.Key == firstKeyDefault ? firstType : k.Key, k => k.Value);
                                    ServerDictAdorn = ServerDictAdorn.ToDictionary(k => k.Key == firstType ? client_p.Key : k.Key, k => k.Value);
                                    ServerDic[firstType].RingItemAdorn = "0";
                                }
                            }
                            else// 不同位置武器
                            {
                                Utility.Debug.LogInfo("<不同位置id武器>" + firstAdorn);
                                if (firstKeyDefault != 0)
                                {
                                    if (ServerDic.ContainsKey(client_p.Key))
                                    {
                                        Utility.Debug.LogInfo("<NowID>" + client_p.Key);
                                        int NowID = 0;
                                        while (true)
                                        {
                                            Utility.Debug.LogInfo("<NowIDtrue>" + client_p.Key);
                                            var randomNumer = new Random().Next(3001, 4000);
                                            Utility.Debug.LogInfo("<randomNumer>" + randomNumer);
                                            if (client_p.Key.ToString().Length > 8)
                                                NowID = client_p.Key + randomNumer;
                                            else
                                                NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                                            if (!ServerDic.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                                            {
                                                ServerDic = ServerDic.ToDictionary(k => k.Key == client_p.Key ? NowID : k.Key, k => k.Value);
                                                break;
                                            }
                                        }
                                        ServerDic = ServerDic.ToDictionary(k => k.Key == NowID ? client_p.Key : k.Key, k => k.Value);
                                    }
                                    else
                                        ServerDic = ServerDic.ToDictionary(k => k.Key == firstKeyDefault ? client_p.Key : k.Key, k => k.Value);
                                    ServerDic[client_p.Key].RingItemAdorn = "0";//serverData;
                                    ServerDic[client_p.Key].RingItemCount = 1;
                                    ServerDictAdorn.Remove(firstAdorn);
                                }
                                else
                                {
                                    ServerDic.Add(firstAdorn, ServerDictAdorn[firstAdorn]);
                                    ServerDic[firstAdorn].RingItemAdorn = "0";
                                    ServerDictAdorn.Remove(firstAdorn);
                                }
                            }
                        }

                        #endregion

                        if (ServerMagicDic.Count != 0)
                        {
                            for (int i = 0; i < ServerMagicDic.Count; i++)
                            {
                                if (ServerMagicDic.Values.ToList()[i] == -1 && client_p.Value.RingItemAdorn == "2")
                                {
                                    ServerMagicDic[i] = client_p.Key;
                                    break;
                                }
                                else if (ServerMagicDic.Values.ToList()[i] == client_p.Key && client_p.Value.RingItemAdorn == "0")
                                {
                                    ServerMagicDic[i] = -1;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Utility.Debug.LogInfo("<服务器的数量小于客户端传过来的数量>");
                    }
                }
                NHibernateQuerier.Update(new Ring() { ID = ringServerArray.ID, RingId = ringServerArray.RingId, RingItems = Utility.Json.ToJson(ServerDic), RingMagicDictServer = Utility.Json.ToJson(ServerMagicDic), RingAdorn = Utility.Json.ToJson(ServerDictAdorn) });
                //GetDataCmdS2C(roleId, InventoryObj, nHCriteria);
                SendDataCmdS2C(roleId, nHCriteria, InventoryInstructions.UpdateData);

            }
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="InventoryObj"></param>
        /// <param name="nHCriteria"></param>
        public static void RemoveCmdS2C(int roleId, RingDTO InventoryObj, NHCriteria nHCriteria)
        {
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            var ServerDic = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingItems);
            var ServerMagicDic = Utility.Json.ToObject<Dictionary<int, int>>(ringServerArray.RingMagicDictServer);
            var ServerDictAdorn = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingAdorn);
            foreach (var client_p in InventoryObj.RingItems)
            {
                if (!ServerDic.ContainsKey(client_p.Key))
                {
                    Utility.Debug.LogInfo("不存在道具id");
                    break;
                }
                else
                {
                    int NowID = 0;
                    while (true)
                    {
                        var randomNumer = new Random().Next(3001, 4000);
                        Utility.Debug.LogInfo("<randomNumer>" + randomNumer);
                        if (client_p.Key.ToString().Length == 6 || client_p.Key.ToString().Length == 7 || client_p.Key.ToString().Length == 8)
                            NowID = Int32.Parse(client_p.Key.ToString() + randomNumer.ToString().Substring(0, 3));
                        else if (client_p.Key.ToString().Length > 8)
                            NowID = client_p.Key + randomNumer;
                        else
                            NowID = Int32.Parse(client_p.Key.ToString() + randomNumer);
                        if (!ServerDic.ContainsKey(NowID) && !ServerDictAdorn.ContainsKey(NowID))
                        {
                            ServerDic = ServerDic.ToDictionary(k => k.Key == client_p.Key ? NowID : k.Key, k => k.Value);
                            ServerDic[NowID].RingItemCount = 0;
                            ServerDic[NowID].RingItemAdorn = "0";
                            break;
                        }
                    }
                }
            }
            NHibernateQuerier.Update(new Ring() { ID = ringServerArray.ID, RingId = ringServerArray.RingId, RingItems = Utility.Json.ToJson(ServerDic), RingMagicDictServer = Utility.Json.ToJson(ServerMagicDic), RingAdorn = Utility.Json.ToJson(ServerDictAdorn) });
            //GetDataCmdS2C(roleId, InventoryObj, nHCriteria);
            SendDataCmdS2C(roleId, nHCriteria, InventoryInstructions.RemoveData);

        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="InventoryObj"></param>
        /// <param name="nHCriteria"></param>
        public static void SortingCmdS2C(int roleId, RingDTO InventoryObj, NHCriteria nHCriteria)
        {
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            var ServerDict = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingItems);
            var ServerMagicDic = Utility.Json.ToObject<Dictionary<int, int>>(ringServerArray.RingMagicDictServer);
            var ServerDictAdorn = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServerArray.RingAdorn);
            while (true)
            {
                var firstKeyDefault = ServerDict.FirstOrDefault(q => q.Value.RingItemAdorn == "1" || q.Value.RingItemAdorn == "2" || q.Value.RingItemCount == 0).Key;
                if (firstKeyDefault != 0)
                    ServerDict.Remove(firstKeyDefault);
                else
                    break;
            }
            var sortDict = ServerDict.OrderByDescending(s => s.Key.ToString().Substring(0, 5)).ToDictionary(s => s.Key, s => s.Value);
            NHibernateQuerier.Update(new Ring() { ID = ringServerArray.ID, RingId = ringServerArray.RingId, RingItems = Utility.Json.ToJson(sortDict), RingMagicDictServer = Utility.Json.ToJson(ServerMagicDic), RingAdorn = Utility.Json.ToJson(ServerDictAdorn) });
            //GetDataCmdS2C(roleId, InventoryObj, nHCriteria);
            SendDataCmdS2C(roleId, nHCriteria, InventoryInstructions.SortingData);
        }
        /// <summary>
        /// 获得背包数据的Cmd
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="InventoryObj"></param>
        /// <param name="nHCriteria"></param>    //都是通过这个来 我看到了，你这个发出去就是属于CMD的，  只是进入服务器需要做一下修改，我看来下  对需要修改的不多，就是那个入口handler你处理下
        static void GetDataCmdS2C(int roleId, RingDTO InventoryObj, NHCriteria nHCriteria)
        {
            //var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            //OperationData opData = new OperationData();
            //opData.DataMessage = ServerToClientParams(ringServerArray);
            //opData.OperationCode = (byte)OperationCode.SyncInventory;
            //GameEntry.RoleManager.SendMessage(roleId, opData);

            SendDataCmdS2C(roleId, nHCriteria, InventoryInstructions.GetData);

        }
        static void SendDataCmdS2C(int roleId, NHCriteria nHCriteria, InventoryInstructions subCode)
        {
            var ringServerArray = CriteriaSelectMethod<Ring>(nHCriteria);
            OperationData opData = new OperationData();
            opData.DataMessage =Utility.Json.ToJson( ServerToClientParams(ringServerArray));
            opData.OperationCode = (byte)OperationCode.SyncInventory;
            opData.SubOperationCode = (byte)subCode;
            GameEntry.RoleManager.SendMessage(roleId, opData);
            Utility.Debug.LogError("背包物品被移除");
        }
        void ProcessInvHandlerC2S(int sessionId, OperationData packet)
        {
            var dictData = Utility.Json.ToObject<Dictionary<byte, object>>(packet.DataMessage.ToString());
            var InventoryRoleData = Utility.GetValue(dictData, (byte)ParameterCode.Role) as string;
            var InventoryData = Utility.GetValue(dictData, (byte)ParameterCode.Inventory) as string;
            Utility.Debug.LogInfo($"InventoryManager : ==>>\n接收roleId{ InventoryRoleData }\n接收背包的数据{InventoryData }");
            var InventoryRoleObj = Utility.Json.ToObject<RoleDTO>(InventoryRoleData);
            var InventoryObj = Utility.Json.ToObject<RingDTO>(InventoryData);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", InventoryRoleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleRing>(nHCriteriaRoleID);
            NHCriteria nHCriteriaRingID = null;
            if (exist)
            {
                var ringServer = NHibernateQuerier.CriteriaSelect<RoleRing>(nHCriteriaRoleID);
                nHCriteriaRingID =ReferencePool.Accquire<NHCriteria>().SetValue("ID", ringServer.RingIdArray);
            }
            //数据就这样解析一下就行
            var areaSubCode = (InventoryInstructions)packet.SubOperationCode;
            switch (areaSubCode)
            {
                case InventoryInstructions.GetData:
                    GetDataCmdS2C(InventoryRoleObj.RoleID, InventoryObj, nHCriteriaRingID);
                    break;
                case InventoryInstructions.AddData:
                    AddDataCmdS2C(InventoryRoleObj.RoleID, InventoryObj, nHCriteriaRingID);
                    break;
                case InventoryInstructions.UpdateData:
                    UdpateCmdS2C(InventoryRoleObj.RoleID, InventoryObj, nHCriteriaRingID);
                    break;
                case InventoryInstructions.RemoveData:
                    RemoveCmdS2C(InventoryRoleObj.RoleID, InventoryObj, nHCriteriaRingID);
                    break;
                case InventoryInstructions.SortingData:
                    SortingCmdS2C(InventoryRoleObj.RoleID, InventoryObj, nHCriteriaRingID);
                    break;
            }
            ReferencePool.Release(nHCriteriaRoleID);
        }
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncInventory, ProcessInvHandlerC2S);
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncTemInventory, ProcessTempInvHandlerC2S);
        }
    }
}


