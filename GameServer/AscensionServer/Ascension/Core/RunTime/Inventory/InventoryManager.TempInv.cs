using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using AscensionProtocol;
using Cosmos;
using AscensionServer.Model;

namespace AscensionServer
{
    public partial class InventoryManager
    {
        void ProcessTempInvHandlerC2S(int sessionId, OperationData packet)
        {
            var subCode = (SubOperationCode)packet.SubOperationCode;
            switch (subCode)
            {
                case SubOperationCode.Add:
                    Add(sessionId, packet);
                    break;
                case SubOperationCode.Remove:
                    Remove(sessionId, packet);
                    break;
                case SubOperationCode.Get:
                    Get(sessionId, packet);
                    break;
                case SubOperationCode.Update:
                    Update(sessionId, packet);
                    break;
            }
        }
        void Add(int sessionId, OperationData packet)
        {
            var dict = Utility.Json.ToObject<Dictionary<byte, object>>(Convert.ToString(packet.DataMessage));
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.SyncTemInventory,
                SubOperationCode = (short)SubOperationCode.Add
            };
            var TemRingRoleData = Utility.GetValue(dict, (byte)ParameterCode.RoleTemInventory) as string;
            Utility.Debug.LogInfo(">>>>>Add 临时背包" + TemRingRoleData + ">>>>>>>>>>>>>");
            var TemRingRoleObj = Utility.Json.ToObject<TemporaryRingDTO>(TemRingRoleData);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", TemRingRoleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleRing>(nHCriteriaRoleID);
            if (exist)
            {
                var ringServer = NHibernateQuerier.CriteriaSelect<TemporaryRing>(nHCriteriaRoleID);
                var tempRing = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServer.RingItems);

                foreach (var temp in TemRingRoleObj.RingItems)
                {
                    if (!tempRing.ContainsKey(temp.Key))
                        tempRing.Add(temp.Key, temp.Value);
                    else
                    {
                        var tempValue = tempRing[temp.Key];
                        if (temp.Value.RingItemCount > 0)
                            tempValue.RingItemCount += temp.Value.RingItemCount;
                        if (tempValue.RingItemTime != temp.Value.RingItemTime)
                            tempValue.RingItemTime = temp.Value.RingItemTime;
                        if (tempValue.RingItemAdorn != temp.Value.RingItemAdorn)
                            tempValue.RingItemAdorn = temp.Value.RingItemAdorn;
                    }
                    NHibernateQuerier.Update(new TemporaryRing() { RoleID = TemRingRoleObj.RoleID, RingItems = Utility.Json.ToJson(tempRing) });

                }
                opData.ReturnCode = (short)ReturnCode.Success;
            }
            else
            {
                opData.ReturnCode = (short)ReturnCode.Fail;
            }
            ReferencePool.Release(nHCriteriaRoleID);
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
        void Get(int sessionId, OperationData packet)
        {
            var dict = Utility.Json.ToObject<Dictionary<byte, object>>(Convert.ToString(packet.DataMessage));
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.SyncTemInventory,
                SubOperationCode = (short)SubOperationCode.Get
            };
            var TemRingRoleData = Utility.GetValue(dict, (byte)ParameterCode.RoleTemInventory) as string;
            Utility.Debug.LogInfo(">>>>>Get 临时背包 " + TemRingRoleData + ">>>>>>>>>>>>>");
            var TemRingRoleObj = Utility.Json.ToObject<TemporaryRingDTO>(TemRingRoleData);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", TemRingRoleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleRing>(nHCriteriaRoleID);
            if (exist)
            {
                var ringServer = NHibernateQuerier.CriteriaSelect<TemporaryRing>(nHCriteriaRoleID);
                var messageDict = new Dictionary<byte, object>();
                messageDict.Add((byte)ParameterCode.RoleTemInventory, ringServer.RingItems);
                opData.DataMessage = Utility.Json.ToJson(messageDict);
                opData.ReturnCode = (short)ReturnCode.Success;
            }
            else
            {
                opData.ReturnCode = (short)ReturnCode.Fail;
            }
            ReferencePool.Release(nHCriteriaRoleID);
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
        void Update(int sessionId, OperationData packet)
        {
            var dict = Utility.Json.ToObject<Dictionary<byte, object>>(Convert.ToString(packet.DataMessage));
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.SyncTemInventory,
                SubOperationCode = (short)SubOperationCode.Update
            };
            var TemRingRoleData = Utility.GetValue(dict, (byte)ParameterCode.RoleTemInventory) as string;
            Utility.Debug.LogInfo(">>>>>Update 临时背包" + TemRingRoleData + ">>>>>>>>>>>>>");
            var TemRingRoleObj = Utility.Json.ToObject<TemporaryRingDTO>(TemRingRoleData);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", TemRingRoleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleRing>(nHCriteriaRoleID);
            if (exist)
            {
                var ringServer = NHibernateQuerier.CriteriaSelect<TemporaryRing>(nHCriteriaRoleID);
                var tempRing = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServer.RingItems);

                foreach (var temp in TemRingRoleObj.RingItems)
                {
                    if (!tempRing.ContainsKey(temp.Key))
                    {
                        opData.ReturnCode = (short)ReturnCode.Fail;
                        continue;
                    }
                    else
                    {
                        var tempValue = tempRing[temp.Key];
                        if (temp.Value.RingItemCount > temp.Value.RingItemCount)
                        {
                            tempValue.RingItemCount -= temp.Value.RingItemCount;
                            tempValue.RingItemTime = tempValue.RingItemTime;
                            tempValue.RingItemAdorn = temp.Value.RingItemAdorn;
                        }
                        else
                            tempRing.Remove(temp.Key);
                    }
                    NHibernateQuerier.Update(new TemporaryRing() { RoleID = TemRingRoleObj.RoleID, RingItems = Utility.Json.ToJson(tempRing) });
                }
                opData.ReturnCode = (short)ReturnCode.Success;
            }
            else
            {
                opData.ReturnCode = (short)ReturnCode.Fail;
            }
            ReferencePool.Release(nHCriteriaRoleID);
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
        void Remove(int sessionId, OperationData packet)
        {
            var dict = Utility.Json.ToObject<Dictionary<byte, object>>(Convert.ToString(packet.DataMessage));
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.SyncTemInventory,
                SubOperationCode = (short)SubOperationCode.Remove
            };
            var TemRingRoleData = Utility.GetValue(dict, (byte)ParameterCode.RoleTemInventory) as string;
            Utility.Debug.LogInfo(">>>>>Remove 临时背包" + TemRingRoleData + ">>>>>>>>>>>>>");
            var TemRingRoleObj = Utility.Json.ToObject<TemporaryRingDTO>(TemRingRoleData);
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", TemRingRoleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleRing>(nHCriteriaRoleID);
            if (exist)
            {
                var ringServer = NHibernateQuerier.CriteriaSelect<TemporaryRing>(nHCriteriaRoleID);
                var tempRing = Utility.Json.ToObject<Dictionary<int, RingItemsDTO>>(ringServer.RingItems);

                foreach (var temp in TemRingRoleObj.RingItems)
                {
                    if (!tempRing.ContainsKey(temp.Key))
                    {
                        opData.ReturnCode = (short)ReturnCode.Fail;
                        continue;
                    }
                    else
                        tempRing.Remove(temp.Key);
                    NHibernateQuerier.Update(new TemporaryRing() { RoleID = TemRingRoleObj.RoleID, RingItems = Utility.Json.ToJson(tempRing) });
                }
                opData.ReturnCode = (short)ReturnCode.Success;
            }
            else
            {
                opData.ReturnCode = (short)ReturnCode.Fail;
            }
            ReferencePool.Release(nHCriteriaRoleID);
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
    }
}
