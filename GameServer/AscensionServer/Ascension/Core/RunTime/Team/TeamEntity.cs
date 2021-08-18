using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cosmos;
using AscensionProtocol;

namespace AscensionServer
{
    public class TeamEntity: IReference
    {
        /// <summary>
        /// 队长ID
        /// </summary>
        public int CaptainId{ get; set; }
        /// <summary>
        /// 系统分配的队伍ID
        /// </summary>
        public int TeamID { get; set; }
        /// <summary>
        /// 是否队伍满员
        /// </summary>
        public bool IsFull { get { return peerDict.Count >=_TeamCapacity; } }
        /// <summary>
        /// 广播事件消息 ;
        /// </summary>
        public event Action<byte, object>EventMessage
        {
            add { eventMessage += value; }
            remove
            {
                try { eventMessage -= value; }
                catch (Exception e) { Utility.Debug.LogError($"无法移除发送消息的委托:{e}"); }
            }
        }
        /// <summary>
        /// 广播普通消息;
        /// </summary>
        public event Action<object> Message
        {
            add { message += value; }
            remove
            {
                try { message -= value; }
                catch (Exception e) { Utility.Debug.LogError($"无法移除发送消息的委托:{e}"); }
            }
        }
        readonly ushort _TeamCapacity = 5;
        ConcurrentDictionary<int, RoleEntity> peerDict
            = new ConcurrentDictionary<int, RoleEntity>();
        IPeerEntity captain;

        Action<byte, object> eventMessage;
        Action<object> message;
        /// <summary>
        /// 初始化队伍
        /// </summary>
        /// <param name="createrId">创建者的ID</param>
        public void Oninit(int createrId,int teamID)
        {
            TeamID = teamID;
            CaptainId = createrId;
        }
        /// <summary>
        /// 加入队伍
        /// </summary>
        /// <param name="roleId">peerID</param>
        /// <returns>是否加入成功</returns>
        public bool JoinTeam(int roleId)
        {

            return peerDict.TryAdd(roleId,null);
        }
        /// <summary>
        /// 主动离队
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns>是否离队成功</returns>
        public bool LeaveTeam(int roleId)
        {
            RoleEntity peer;
            return peerDict.TryRemove(roleId, out peer);
        }
        /// <summary>
        /// 被请离队伍；
        /// 值一队长有权限
        /// </summary>
        public bool KickOutOfTeam(int cmdInputterID,int  roleId)
        {
            if (cmdInputterID != CaptainId)
                return false;
            return LeaveTeam(roleId);
        }
        /// <summary>
        /// 解散队伍
        /// </summary>
        public void DissolveTeam()
        {
            eventMessage?.Invoke((byte)OperationCode.SyncRoleTeam, null);
            //foreach (var peer in peerDict.Values)
            //{
            //    //广播解散队伍的消息
            //}
            Release();
        }
        /// <summary>
        /// 提升为队长；
        /// 只有前队长才进行的命令;
        /// </summary>
        public void PreferredToCaptain(uint cmdInputterID,int roleId)
        {
            if (cmdInputterID != CaptainId)
                return;
            CaptainId = roleId;
            // 广播成为队长事件
        }
        public void Release()
        {
            peerDict.Clear();
            CaptainId = 0;
            captain = null;
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        public void BroadcastEvent(byte opCode, object data)
        {
            eventMessage?.Invoke(opCode, data);
        }
        public void BroadcastMessage(byte opCode, object data)
        {
            eventMessage?.Invoke(opCode, data);
        }
    }
}


