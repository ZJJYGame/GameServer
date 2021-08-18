using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Cosmos;
namespace AscensionServer
{
    [Module]
    public class TeamManager :Cosmos. Module,ITeamManager
    {
        /// <summary>
        /// 房间ID长度
        /// </summary>
        readonly byte _IDLenth = 6;
        /// <summary>
        /// 生成房间时最小取值范围
        /// </summary>
        readonly int _MinValue = 1000000;
        /// <summary>
        /// 成功房间时最大取值范围
        /// </summary>
        readonly int _MaxValue = 99999999;
        CancellationToken cancelToken = new CancellationToken();
        ConcurrentDictionary<int, TeamEntity> teamDict = new ConcurrentDictionary<int, TeamEntity>();
        HashSet<int>matchingQueue = new HashSet<int>();
        public TeamEntity CreateTeam(int createrId)
        {
            if (teamDict.ContainsKey(createrId))
                return null;
            var tc =ReferencePool.Accquire<TeamEntity>();
            tc.Oninit(createrId, CreateTeamID());
            return tc;
        }
        public bool LeaveMatchQueue(int roleId)
        {
            return matchingQueue.Remove(roleId);
        }
        /// <summary>
        ///加入随机匹配队列
        /// </summary>
        public bool JoinMatchQueue(int roleId)
        {
            return matchingQueue.Add(roleId);
        }

        /// <summary>
        /// 服务端会一直运行；
        /// 异步随机匹配组队
        /// </summary>
        /// <returns>异步运行</returns>
        public async Task MatchTeam()
        {
            while (true)
            {
                if (!cancelToken.IsCancellationRequested)
                {
                    if (teamDict.Count <= 0 || matchingQueue.Count <= 0)
                        return;
                    HashSet<int> removeIdSet = new HashSet<int>();
                    int teamId = 0;
                    foreach (var team in teamDict.Values)
                    {
                        if (!team.IsFull)
                            teamId = team.TeamID;
                    }
                    await Task.Run(() =>
                    {
                        foreach (var id in matchingQueue)
                        {
                            if (teamId == 0)
                                continue;
                            TeamEntity tc;
                            teamDict.TryGetValue(teamId, out tc);
                            tc.JoinTeam(id);
                            removeIdSet.Add(id);
                        }
                        foreach (var id in removeIdSet)
                        {
                            matchingQueue.Remove(id);
                        }
                    });
                }
            }
        }
        /// <summary>
        /// 加入指定的小队
        /// </summary>
        /// <param name="teamID">小队ID</param>
        /// <param name="roleId">peerID</param>
        /// <returns>是否加入成功</returns>
        public bool JoinTeam(int teamID, int roleId)
        {
            TeamEntity gc;
            if (!teamDict.TryGetValue(teamID, out gc))
                return false;
            else
                return gc.JoinTeam(roleId);
        }
        /// <summary>
        /// 生成队伍ID；
        /// 尾递归检测是否生成了同样的key
        /// </summary>
        /// <returns>生成后的ID</returns>
        int CreateTeamID()
        {
            int id =  Utility.Algorithm.RandomRange(_MinValue, _MaxValue);
            if (!teamDict.ContainsKey(id))
                return id;
            else
                return CreateTeamID();
        }
        [TickRefresh]
        void OnRefresh()
        {
            if (IsPause)
                return;
        }
    }
}


