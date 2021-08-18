using Cosmos;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Linq.Clauses;
using System.ServiceModel.Configuration;
using AscensionProtocol;
using AscensionProtocol.DTO;
using Cosmos.ECS;
namespace AscensionServer
{
    /// <summary>
    /// 场景实体对象
    /// </summary>
    public class LevelEntity : Entity, IReference, IRefreshable
    {
        /// <summary>
        /// None表示当前失活状态；
        /// 用于区分场景属于历练还是秘境类型；
        /// </summary>
        public LevelTypeEnum LevelType { get; private set; }
        /// <summary>
        /// 此Id为单增；
        /// 此Id是用于区分例如秘境等同步人数不多，但是数量众多的散列场景对象；
        /// 如玩家A组队进入秘境场景，则为其分配一个场景对象。玩家B进入秘境，同样分配一个场景；
        /// </summary>
        public long LevelId { get { return Id; } set { Id = value; } }
        /// <summary>
        /// 当前场景对象是否存活；
        /// </summary>
        public bool Available { get; private set; }
        Action<OperationData> roleSendMsgHandler;
        event Action<OperationData> RoleSendMsgHandler
        {
            add { roleSendMsgHandler += value; }
            remove { roleSendMsgHandler -= value; }
        }
        /// <summary>
        /// RoleId---LevelConn
        /// </summary>
        Dictionary<int, LevelConn> connDict;
        List<LevelConn> connCache;
        List<FixTransportData> transportDataCache;
        long currentTick;
        OperationData inputSendOpData;
        /// <summary>
        /// 此场景的容量；
        /// </summary>
        public int Capacity { get; private set; }
        #region Pool
        Pool<OperationData> opDataPool;
        Pool<Dictionary<byte, object>> messageDataPool;
        #endregion

        public int PlayerCount { get { return connDict.Count; } }
        public bool Empty { get { return connDict.Count <= 0; } }
        public bool IsFull { get { return connDict.Count >= Capacity; } }

        public LevelEntity()
        {
            transportDataCache = new List<FixTransportData>();
            connCache = new List<LevelConn>();
            messageDataPool = new Pool<Dictionary<byte, object>>(() => new Dictionary<byte, object>(), (t) => t.Clear());
            inputSendOpData = new OperationData();
            inputSendOpData.OperationCode = (byte)OperationCode.MultiplayArea;
            inputSendOpData.SubOperationCode = (byte)LevelOpCode.PlayerInput;
            connDict = new Dictionary<int, LevelConn>();
            opDataPool = new Pool<OperationData>(() => new OperationData(), (t) => t.Dispose());
        }
        public bool HasRole(int roleId)
        {
            return connDict.ContainsKey(roleId);
        }
        //========================================
        //玩家进入逻辑规则：
        //1、新玩家进入，发送当前场景中已经存在的其他玩家数据，
        //并将自己添加到场景玩家容器中；
        //2、广播新进入玩家的数据到当前场景中的其他玩家；
        //3、广播完成，将玩家对象广播的接口进行委托监听；
        //========================================
        public bool EnterLevel(int roleId, LevelConn conn)
        {
            if (IsFull)
                return false;
            if (!connDict.ContainsKey(roleId))
            {
                connCache.Add(conn);
                Utility.Debug.LogWarning($"RoleId:{roleId};SessionId:{conn.RoleEntity.SessionId}进入Level :{LevelType} ; {LevelId}");
                SendExists2EnteredS2C(roleId, conn);
                SendEntered2ExistsS2C(roleId, conn);
                connDict.Add(roleId, conn);
                RoleSendMsgHandler += conn.RoleEntity.SendMessage;
                Available = true;
                return true;
            }
            return false;
        }
        public bool ExitLevel(int roleId)
        {
            var result = connDict.Remove(roleId, out var conn);
            if (result)
            {
                connCache.Remove(conn);
                Utility.Debug.LogWarning($"RoleId:{roleId};SessionId:{conn.RoleEntity.SessionId}离开Level :{LevelType} ; {LevelId}");
                RoleSendMsgHandler -= conn.RoleEntity.SendMessage;
                SendExited2ExistedS2C(conn);
                SendExitDoneS2C(conn);
                if (connDict.Count <= 0)
                    Available = false;
            }
            return result;
        }
        /// <summary>
        ///接收到消息后直接存储，不考虑顺序 
        /// </summary>
        public void OnCommandC2S(int roleId, FixTransportData inputJson)
        {
            var result = connDict.TryGetValue(roleId, out var conn);
            if (result)
            {
                conn.InputData = inputJson;
            }
        }
        /// <summary>
        /// 发送消息到当前场景所有玩家；
        /// Send message to all role server to client
        /// </summary>
        public void BroadCast2AllS2C(OperationData data)
        {
            roleSendMsgHandler?.Invoke(data);
        }
        /// <summary>
        /// 乐观锁
        /// </summary>
        public void OnRefresh()
        {
            if (!Available)
                return;
            transportDataCache.Clear();
            var length = connCache.Count;
            for (int i = 0; i < length; i++)
            {
                if (!Utility.Assert.IsNull(connCache[i].InputData))
                {
                    transportDataCache.Add(connCache[i].InputData);
                    connCache[i].InputData = null;
                }
            }
            if (transportDataCache.Count > 0)
            {
                inputSendOpData.DataMessage = Utility.Json.ToJson(transportDataCache);
                roleSendMsgHandler?.Invoke(inputSendOpData);
            }
            currentTick++;
        }
        public void Release()
        {
            //Utility.Debug.LogWarning($"Level:{LevelId}无玩家，Clear");
            this.LevelId = 0;
            this.Available = false;
            roleSendMsgHandler = null;
            currentTick = 0;
            opDataPool.Clear();
            messageDataPool.Clear();
            Capacity = 0;
        }
        public static LevelEntity Create(LevelTypeEnum levelType, int levelId, int capacity)
        {
            LevelEntity se =ReferencePool.Accquire<LevelEntity>();
            se.LevelType = levelType;
            se.Available = true;
            se.Capacity = capacity;
            se.LevelId = levelId;
            return se;
        }
        public static void Release(LevelEntity levelEntity)
        {
            ReferencePool.Release(levelEntity);
        }
        /// <summary>
        /// 将进入的玩家发送到已经在场景中的玩家
        /// </summary>
        void SendEntered2ExistsS2C(int roleId, LevelConn conn)
        {
            var opData = opDataPool.Spawn();
            opData.OperationCode = (byte)OperationCode.MultiplayArea;
            opData.SubOperationCode = (byte)LevelOpCode.PlayerEnter;
            var dataMessage = messageDataPool.Spawn();
            GameEntry.RoleManager.TryGetValue(roleId, out var roleEntity);
            roleEntity.TryGetValue<RoleDTO>(out var roleDto);
            dataMessage.Add((byte)LevelParameterCode.EnteredRole, Utility.Json.ToJson(roleDto));
            opData.DataMessage = Utility.Json.ToJson(dataMessage);
            roleSendMsgHandler?.Invoke(opData);
            opDataPool.Despawn(opData);
            messageDataPool.Despawn(dataMessage);
        }
        /// <summary>
        /// 将已经在场景中的玩家发送到进入的玩家
        /// </summary>
        void SendExists2EnteredS2C(int roleId, LevelConn conn)
        {
            var opData = opDataPool.Spawn();
            var dataMessage = messageDataPool.Spawn();
            opData.OperationCode = (byte)OperationCode.MultiplayArea;
            opData.SubOperationCode = (byte)LevelOpCode.PlayerSYN;
            GameEntry.RoleManager.TryGetValues(connDict.Keys.ToArray(), out var dict);
            var existedRoleDtos = dict.Values.ToList();
            if (existedRoleDtos.Count > 0)
            {
                dataMessage.Add((byte)LevelParameterCode.Existed, Utility.Json.ToJson(existedRoleDtos));
                opData.ReturnCode = (byte)ReturnCode.Success;
            }
            else
            {
                opData.ReturnCode = (byte)ReturnCode.Empty;
            }
            dataMessage.Add((byte)LevelParameterCode.ServerSyncInterval, ApplicationBuilder.MSInterval);
            opData.DataMessage = Utility.MessagePack.SerializeToJson(dataMessage);
            conn.RoleEntity.SendMessage(opData);
            opDataPool.Despawn(opData);
            messageDataPool.Despawn(dataMessage);
        }
        /// <summary>
        /// 将离开的玩家数据广播给已经在level中的其他玩家；
        /// </summary>
        void SendExited2ExistedS2C(LevelConn conn)
        {
            var opData = opDataPool.Spawn();
            opData.OperationCode = (byte)OperationCode.MultiplayArea;
            opData.SubOperationCode = (byte)LevelOpCode.PlayerExit;
            var dataMessage = messageDataPool.Spawn();
            dataMessage.Add((byte)LevelParameterCode.ExitedRole, conn.RoleId);
            opData.DataMessage = Utility.Json.ToJson(dataMessage);
            roleSendMsgHandler?.Invoke(opData);
            opDataPool.Despawn(opData);
            messageDataPool.Despawn(dataMessage);
        }
        /// <summary>
        /// 发送离开完成消息到客户端；
        /// </summary>
        void SendExitDoneS2C(LevelConn conn)
        {
            var opData = opDataPool.Spawn();
            opData.OperationCode = (byte)OperationCode.MultiplayArea;
            opData.SubOperationCode = (byte)LevelOpCode.PlayerFIN;
            opData.ReturnCode = (byte)ReturnCode.Success;
            conn.RoleEntity.SendMessage(opData);
            opDataPool.Despawn(opData);
        }
    }
}


