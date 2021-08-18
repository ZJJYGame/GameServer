using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cosmos;
namespace AscensionServer
{
    public class RoomEntity : IReference, IKeyValue<int, RoleEntity>
    {
        #region Properties
        public int RoomId { get; private set; }
        /// <summary>
        /// 当前房间对象是否可用
        /// </summary>
        public bool Available { get; private set; }
        /// <summary>
        /// 广播消息事件委托；
        /// </summary>
        public event Action<byte, Dictionary<byte,object>> BroadcastBattleEvent
        {
            add { broadcastBattleEvent += value; }
            remove{broadcastBattleEvent -= value;}
        }
        /// <summary>
        /// 倒计时秒
        /// </summary>
        protected int countDownSec = 15;
        /// <summary>
        /// 是否可收集指令
        /// </summary>
        protected bool canCacheCmd = false;
        /// <summary>
        /// 当前房间内战斗的回合数
        /// </summary>
        protected int roundCount = 0;
        protected ConcurrentDictionary<int, RoleEntity> roleDict 
            = new ConcurrentDictionary<int, RoleEntity>();
        protected Action<byte, Dictionary<byte,object>> broadcastBattleEvent;
        protected object battleResultdata;
        #endregion

        #region Methods
        /// <summary>
        /// 初始化房间；
        /// 分配ID给当前房间
        /// </summary>
        /// <param name="roomId">分配的房间ID</param>
        public virtual void OnInit(int roomId)
        {
            Available = true;
            this.RoomId = roomId;
        }
        public virtual void Release()
        {
            roleDict.Clear();
            canCacheCmd = true;
            Available = false;
            broadcastBattleEvent = null;
        }
        public bool TryGetValue(int roleId, out RoleEntity role )
        {
            return roleDict.TryGetValue(roleId, out role);
        }
        public bool ContainsKey(int roleId)
        {
            return roleDict.ContainsKey(roleId);
        }
        public bool TryRemove(int roleId)
        {
            var result = roleDict.TryRemove(roleId, out var role);
            if (result)
                BroadcastBattleEvent -= role.SendMessage;
            return result;
        }
        public bool TryAdd(int roleId, RoleEntity role)
        {
            if (role == null)
               throw new ArgumentNullException("PeerEntity is invaild ! ");
            var result = roleDict.TryAdd(roleId, role);
            if (result)
                BroadcastBattleEvent += role.SendMessage;
            return result;
        }
        public bool TryRemove(int roleId, out RoleEntity role)
        {
            var result = roleDict.TryRemove(roleId, out role);
            if (result)
                BroadcastBattleEvent -= role.SendMessage;
            return result;
        }
        public bool TryUpdate(int roleId, RoleEntity newRole, RoleEntity comparsionRole)
        {
            var result = roleDict.TryUpdate(roleId, newRole, comparsionRole);
            if (result)
            {
                BroadcastBattleEvent -= comparsionRole.SendMessage;
                BroadcastBattleEvent += newRole.SendMessage;
            }
            return result;
        }
        public async void CountDown()
        {
            await Task.Delay(new TimeSpan(0, 0, 15));
            Utility.Debug.LogInfo("15秒倒计时结束，开始广播战斗计算结果");
            //await BroadcastMessageAsync(null);
        }
        /// <summary>
        /// 通过peer实体生成房间实体；
        /// </summary>
        /// <param name="roles">peer的数组</param>
        /// <returns>生成的房间实体</returns>
        public static RoomEntity Create(params RoleEntity [] roles)
        {
            var length = roles.Length;
            var re =ReferencePool.Accquire<RoomEntity>();
            for (int i = 0; i < length; i++)
            {
                re.TryAdd(roles[i].RoleId, roles[i]);
            }
            return re;
        }
        /// <summary>
        /// 通过sessionId生成roomEntity;
        /// 若传入的任意sessionId无效，则房间实体生成失败，返回空；
        /// </summary>
        /// <param name="roleIds">用户会话Id数组</param>
        /// <returns>生成的房间实体</returns>
        public static RoomEntity Create(params  int[] roleIds)
        {
            List<RoleEntity> roleSet = new List<RoleEntity>();
            var length = roleIds.Length;
            for (int i = 0; i < length; i++)
            {
                var result = GameEntry. RoleManager.TryGetValue(roleIds[i], out var  role);
                if (result)
                    roleSet.Add(role);
                else
                    return null;
            }
           return Create(roleSet.ToArray());
        }
        /// <summary>
        /// 开始回合；
        /// 收集指令；
        /// </summary>
        protected void StartRound()
        {
            roundCount++;
            canCacheCmd = true;
            CountDown();
        }
        protected virtual void  BroadcastEvent(byte opCode, Dictionary<byte,object> data)
        {
            broadcastBattleEvent?.Invoke(opCode, data);
        }
        protected virtual async Task BroadcastEventAsync(byte opCode, Dictionary<byte,object> data)
        {
            await Task.Run(() => broadcastBattleEvent?.Invoke(opCode, data));
        }
        #endregion
    }
}


