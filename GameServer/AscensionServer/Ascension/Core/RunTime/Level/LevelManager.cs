using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 场景管理器，管理野外高同步的数据；
    /// </summary>
    [Module]
    public partial class LevelManager : Module, ILevelManager
    {
        //========================================
        //注意：
        //1、历练的场景设计是按照棋盘格切块的，表现效果参照Unity
        //插件WorldSteamer。
        //2、假设有100*100的单位地图，均匀切成5*5的方块。则一个单位
        //为20*20。在同步时需要同步此单位周边9宫格的地块信息。
        //□□□
        //□■□
        //□□□
        //实心块即玩家所在块，空心块为玩家周边地块。
        //========================================
        /// <summary>
        /// 历练单个对象场景能够容纳的玩家数量；
        /// </summary>
        public const int AdventureSingleLevelCapacity = 128;
        /// <summary>
        /// 秘境单个场景能够容纳的玩家数量；
        /// </summary>
        public const int SecretSingleLevelCapacity = 6;

        LevelEntity adventureLevel;
        /// <summary>
        /// 历练场景字典;
        /// </summary>
        //ConcurrentDictionary<int, LevelEntity> adventureLevelEntityDict;
        int adventureLevelIndex = 100;

        /// <summary>
        /// 秘境场景字典;
        /// </summary>
        ConcurrentDictionary<long, LevelEntity> secretAreaLevelEntityDict;
        int secretAreaLevelIndex = 100;

        /// <summary>
        /// Key : SessionId---Value : LevelConn
        /// </summary>
        ConcurrentDictionary<int, LevelConn> connDict;
        long latestTime = 0;
        int updateInterval = ApplicationBuilder.MSInterval;

        Action sceneRefreshHandler;
        event Action SceneRefreshHandler
        {
            add { sceneRefreshHandler += value; }
            remove { sceneRefreshHandler -= value; }
        }

        Action<LevelTypeEnum, int, int> onRoleEnterLevel;
        /// <summary>
        /// 角色进入场景事件
        /// </summary>
        public event Action<LevelTypeEnum, int, int> OnRoleEnterLevel
        {
            add { onRoleEnterLevel += value; }
            remove { onRoleEnterLevel -= value; }
        }

        Action<LevelTypeEnum, int, int> onRoleExitLevel;
        /// <summary>
        /// 角色离开场景事件
        /// </summary>
        public event Action<LevelTypeEnum, int, int> OnRoleExitLevel
        {
            add { onRoleExitLevel += value; }
            remove { onRoleExitLevel -= value; }
        }
        Pool<LevelConn> connPool;

        protected override void OnPreparatory()
        {
            adventureLevel = LevelEntity.Create(LevelTypeEnum.Adventure, 701, Int32.MaxValue);
            SceneRefreshHandler += adventureLevel.OnRefresh;
            connPool = new Pool<LevelConn>(() => new LevelConn(), (t) => t.Dispose());
            //adventureLevelEntityDict = new ConcurrentDictionary<int, LevelEntity>();
            secretAreaLevelEntityDict = new ConcurrentDictionary<long, LevelEntity>();
            connDict = new ConcurrentDictionary<int, LevelConn>();

            GameEntry.PeerManager.OnPeerDisconnected += OnPeerDisconnectHandler;
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.MultiplayArea, ProcessMultiplayHandlerS2C);
        }
        //========================================
        //同步注意：
        //1、乐观模式。定时派发，Level内部有自己的空帧处理规则；
        //========================================
        [TickRefresh]
         void OnRefresh()
        {
            if (IsPause)
                return;
            var now = Utility.Time.MillisecondNow();
            if (latestTime <= now)
            {
                latestTime = now + updateInterval;
                sceneRefreshHandler?.Invoke();
            }
        }
        /// <summary>
        ///场景是否包含有角色； 
        /// </summary>
        public bool LevelHasRole(LevelTypeEnum levelType, int levelId, int roleId)
        {
            switch (levelType)
            {
                case LevelTypeEnum.Adventure:
                    {
                        //if (adventureLevelEntityDict.TryGetValue(levelId, out var levelEntity))
                        //{
                        //    return levelEntity.HasRole(roleId);
                        //}
                        return adventureLevel.HasRole(roleId);
                    }
                    break;
                case LevelTypeEnum.SecretArea:
                    {
                        if (secretAreaLevelEntityDict.TryGetValue(levelId, out var levelEntity))
                        {
                            return levelEntity.HasRole(roleId);
                        }
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        ///广播消息到指定场景，若场景不存在，则不执行； 
        /// </summary>
        public void SendMessageToLevelS2C(LevelTypeEnum levelType, int levelId, OperationData opData)
        {
            switch (levelType)
            {
                case LevelTypeEnum.Adventure:
                    {
                        adventureLevel.BroadCast2AllS2C(opData);
                    }
                    break;
                case LevelTypeEnum.SecretArea:
                    {
                        if (secretAreaLevelEntityDict.TryGetValue(levelId, out var levelEntity))
                            levelEntity.BroadCast2AllS2C(opData);
                    }
                    break;
            }
        }
        void OnPeerDisconnectHandler(int sessionId)
        {
            if (connDict.TryRemove(sessionId, out var conn))
            {
                var levelEntity = GetLevelEntity(conn);
                if (Utility.Assert.IsNull(levelEntity))
                    return;
                levelEntity.ExitLevel(conn.RoleId);
                onRoleExitLevel?.Invoke(levelEntity.LevelType, (int)levelEntity.LevelId, conn.RoleId);
                if (levelEntity.Empty)
                {
                    switch (levelEntity.LevelType)
                    {
                        case LevelTypeEnum.Adventure:
                            //adventureLevelEntityDict.TryRemove((int)levelEntity.LevelId, out _);
                            break;
                        case LevelTypeEnum.SecretArea:
                            {
                                //SceneRefreshHandler -= levelEntity.OnRefresh;
                                //LevelEntity.Release(levelEntity);
                                //secretAreaLevelEntityDict.TryRemove((int)levelEntity.LevelId, out _);
                            }
                            break;
                    }

                }
            }
        }
        void ProcessMultiplayHandlerS2C(int sessionId, OperationData packet)
        {
            var subCode = (LevelOpCode)packet.SubOperationCode;
            switch (subCode)
            {
                case LevelOpCode.PlayerSYN:
                    OnPlayerEnterS2C(sessionId, packet);
                    break;
                case LevelOpCode.PlayerFIN:
                    OnPlayerExitS2C(sessionId, packet);
                    break;
                case LevelOpCode.PlayerInput:
                    OnPlayerInputS2C(sessionId, packet);
                    break;
            }
        }
        void OnPlayerEnterS2C(int sessionId, OperationData packet)
        {
            if (!connDict.ContainsKey(sessionId))
            {
                var json = Convert.ToString(packet.DataMessage);
                var obj = Utility.MessagePack.DeserializeJson<Dictionary<object, object>>(json);
                var levelType = Convert.ToByte(Utility.GetValue(obj, ((byte)LevelParameterCode.LevelType).ToString()));
                var roleId = Convert.ToInt32(Utility.GetValue(obj, ((byte)LevelParameterCode.EnteredRole).ToString()));
                var conn = connPool.Spawn();
                conn.LevelType = levelType;
                conn.RoleId = roleId;
                GameEntry.RoleManager.TryGetValue(roleId, out var roleEntity);
                conn.RoleEntity = roleEntity;
                var leveltypeEnum = (LevelTypeEnum)levelType;
                LevelEntity levelEntity = null;
                switch (leveltypeEnum)
                {
                    case LevelTypeEnum.Adventure:
                        {
                            levelEntity = adventureLevel;
                        }
                        break;
                    case LevelTypeEnum.SecretArea:
                        {
                            //需要分单人与多人！！！
                            //levelEntity = LevelEntity.Create(LevelTypeEnum.SecretArea, 0,SecretSingleLevelCapacity);
                        }
                        break;
                }
                levelEntity.EnterLevel(conn.RoleId, conn);
                onRoleEnterLevel?.Invoke(levelEntity.LevelType, (int)levelEntity.LevelId, conn.RoleId);
                conn.LevelId = (int)levelEntity.LevelId;
                connDict.TryAdd(sessionId, conn);
            }
        }
        void OnPlayerExitS2C(int sessionId, OperationData packet)
        {
            if (connDict.TryRemove(sessionId, out var conn))
            {
                var levelEntity = GetLevelEntity(conn);
                if (Utility.Assert.IsNull(levelEntity))
                    return;
                levelEntity.ExitLevel(conn.RoleId);
                onRoleExitLevel?.Invoke(levelEntity.LevelType, (int)levelEntity.LevelId, conn.RoleId);
                if (levelEntity.Empty)
                {
                    switch (levelEntity.LevelType)
                    {
                        case LevelTypeEnum.Adventure:
                            //adventureLevelEntityDict.TryRemove((int)levelEntity.LevelId, out _);
                            break;
                        case LevelTypeEnum.SecretArea:
                            {
                                //SceneRefreshHandler -= levelEntity.OnRefresh;
                                //LevelEntity.Release(levelEntity);
                                //secretAreaLevelEntityDict.TryRemove((int)levelEntity.LevelId, out _);
                            }
                            break;
                    }

                }
            }
        }
        void OnPlayerInputS2C(int sessionId, OperationData packet)
        {
            if (connDict.TryGetValue(sessionId, out var conn))
            {
                var json = Convert.ToString(packet.DataMessage);
                var inputDataObj = Utility.Json.ToObject<FixTransportData>(json);
                var levelEntity = GetLevelEntity(conn);
                levelEntity.OnCommandC2S(conn.RoleId, inputDataObj);
            }
        }
        LevelEntity GetLevelEntity(LevelConn conn)
        {
            var levelType = (LevelTypeEnum)conn.LevelType;
            LevelEntity levelEntity = null;
            switch (levelType)
            {
                case LevelTypeEnum.Adventure:
                    levelEntity = adventureLevel;
                    //adventureLevelEntityDict.TryGetValue(conn.LevelId, out levelEntity);
                    break;
                case LevelTypeEnum.SecretArea:
                    secretAreaLevelEntityDict.TryGetValue(conn.LevelId, out levelEntity);
                    break;
            }
            return levelEntity;
        }
    }
}


