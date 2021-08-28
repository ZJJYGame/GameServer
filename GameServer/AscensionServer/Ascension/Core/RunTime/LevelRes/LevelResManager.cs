using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;
using UnityEngine;

namespace AscensionServer
{
    [Module]
    public class LevelResManager : Module, ILevelResManager
    {

        LevelResEntity adventureLevelResEntity;
        Pool<OperationData> opDataPool;
        Pool<Dictionary<byte, object>> messageDataPool;
        protected override void OnPreparatory()
        {
            opDataPool = new Pool<OperationData>
                (() => { return new OperationData(); }, d => { d.OperationCode = (byte)OperationCode.LevelRes; }, d => { d.Dispose(); });
            messageDataPool = new Pool<Dictionary<byte, object>>(() => new Dictionary<byte, object>(), md => { md.Clear(); });
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.LevelRes, ProcessHandlerC2S);
            GameEntry.LevelManager.OnRoleEnterLevel += SYNResS2C;
            GameEntry.LevelManager.OnRoleExitLevel += FINResS2C;
            try
            {
                adventureLevelResEntity = LevelResEntity.Create(LevelTypeEnum.Adventure, 701);
                adventureLevelResEntity.InitEntityRes();
                adventureLevelResEntity.SetCallback(OnCombatSuccess,OnCombatFailure);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
        void ProcessHandlerC2S(int sessionId, OperationData opData)
        {
            var subCode = (LevelResOpCode)opData.SubOperationCode;
            switch (subCode)
            {
                case LevelResOpCode.Gather:
                    GatherS2C(sessionId, opData);
                    break;
                case LevelResOpCode.StartCombat:
                    CombatS2C(sessionId, opData);
                    break;

            }
        }
        void SYNResS2C(LevelTypeEnum levelType, int levelId, int roleId)
        {
            Utility.Debug.LogWarning("SYNResS2C");
            var opdata = opDataPool.Spawn();
            opdata.SubOperationCode = (byte)LevelResOpCode.SYN;
            var messageData = messageDataPool.Spawn();
            var collectableJson = Utility.Json.ToJson(adventureLevelResEntity.CollectableDict);
            var combatableJson = Utility.Json.ToJson(adventureLevelResEntity.CombatableDict);
            messageData.Add((byte)LevelResParameterCode.Collectable, collectableJson);
            messageData.Add((byte)LevelResParameterCode.Combatable, combatableJson);
            opdata.DataMessage = Utility.Json.ToJson(messageData);
            GameEntry.RoleManager.SendMessage(roleId, opdata);
            opDataPool.Despawn(opdata);
        }
        void FINResS2C(LevelTypeEnum levelType, int levelId, int roleId)
        {
            adventureLevelResEntity.ReleaseCombat(roleId);
            Utility.Debug.LogWarning("FINResS2C");
        }
        void GatherS2C(int sessionId, OperationData packet)
        {
            var json = Convert.ToString(packet.DataMessage);
            var messageDict = Utility.Json.ToObject<Dictionary<byte, object>>(json);
            var gid = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.GId));
            var eleid = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.EleId));
            var index = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.Index));
            var opdata = opDataPool.Spawn();
            opdata.SubOperationCode = (byte)LevelResOpCode.Gather;
            if (adventureLevelResEntity.Gather(index, gid, eleid))
            {
                opdata.DataMessage = json;
                opdata.ReturnCode = (byte)ReturnCode.Success;
                adventureLevelResEntity.BroadCast2AllS2C(opdata);
                Utility.Debug.LogInfo($"采集成功 gid : {gid} , eleid : {eleid}");
            }
            else
            {
                opdata.DataMessage = json;
                opdata.ReturnCode = (byte)ReturnCode.Fail;
                GameEntry.PeerManager.SendMessage(sessionId, opdata);
                Utility.Debug.LogInfo($"采集失败");
            }
            opDataPool.Despawn(opdata);
        }
        void CombatS2C(int sessionId, OperationData packet)
        {
            var json = Convert.ToString(packet.DataMessage);
            var messageDict = Utility.Json.ToObject<Dictionary<byte, object>>(json);
            var gid = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.GId));
            var eleid = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.EleId));
            var index = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.Index));
            var roleId = Convert.ToInt32(Utility.GetValue(messageDict, (byte)LevelResParameterCode.RoleId));
            var opdata = opDataPool.Spawn();
            opdata.SubOperationCode = (byte)LevelResOpCode.StartCombat;

            //opdata.DataMessage = "无法进入战斗，服务器战斗没写好！";
            //GameEntry.PeerManager.SendMessage(sessionId, opdata);

            if (adventureLevelResEntity.Combat(roleId,index, gid, eleid))
            {
                //进入pending状态；
                opdata.DataMessage = json;
                opdata.ReturnCode = (byte)ReturnCode.Success;
                adventureLevelResEntity.BroadCast2AllS2C(opdata);
                Utility.Debug.LogInfo($"进入战斗 成功");
            }
            else
            {
                opdata.DataMessage = json;
                opdata.ReturnCode = (byte)ReturnCode.Fail;
                GameEntry.PeerManager.SendMessage(sessionId, opdata);
                Utility.Debug.LogWarning($"进入战斗 失败");
            }
            opDataPool.Despawn(opdata);
        }
        void OnCombatSuccess(int roleId, LevelResObject levelResObject)
        {
            var opdata = opDataPool.Spawn();
            opdata.SubOperationCode = (byte)LevelResOpCode.CombatResult;
            opdata.DataMessage = Utility.Json.ToJson(levelResObject);
            opdata.ReturnCode = (byte)ReturnCode.Success;
            adventureLevelResEntity.BroadCast2AllS2C(opdata);
            opDataPool.Despawn(opdata);
            Utility.Debug.LogInfo($"玩家：{roleId}历练战斗 成功");
        }
        void OnCombatFailure(int roleId, LevelResObject levelResObject)
        {
            var opdata = opDataPool.Spawn();
            opdata.SubOperationCode = (byte)LevelResOpCode.CombatResult;
            opdata.DataMessage = Utility.Json.ToJson(levelResObject);
            opdata.ReturnCode = (byte)ReturnCode.Fail;
            GameEntry.RoleManager.SendMessage(roleId, opdata);
            opDataPool.Despawn(opdata);
            Utility.Debug.LogInfo($"玩家：{roleId}历练战斗 失败");
        }
    }
}
