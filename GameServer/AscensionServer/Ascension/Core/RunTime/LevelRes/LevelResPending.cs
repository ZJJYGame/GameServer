using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    /// <summary>
    /// 角色id与战斗房间id的映射；
    /// </summary>
    public class LevelResPendingDataProxy
    {
        Dictionary<int, BattleRoomEntityInfo> roleIdBattleRoomDict=new Dictionary<int, BattleRoomEntityInfo>();
        public void AddPending(int roleId, BattleRoomEntityInfo battleInfo)
        {
            roleIdBattleRoomDict.Add(roleId, battleInfo);
        }
        public void RemovePending(int roleId,out BattleRoomEntityInfo battleInfo)
        {
            roleIdBattleRoomDict.Remove(roleId, out battleInfo);
        }
        public bool HashPending(int roleId)
        {
            return roleIdBattleRoomDict.ContainsKey(roleId);
        }
    }
}
