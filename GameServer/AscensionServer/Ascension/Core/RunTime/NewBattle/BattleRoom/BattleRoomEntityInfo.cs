using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public struct BattleRoomEntityInfo
    {
        public static BattleRoomEntityInfo Nil { get { return new BattleRoomEntityInfo(-1, 0); } }
        public BattleRoomEntityInfo(int roomId, int playerCount)
        {
            RoomId = roomId;
            PlayerCount = playerCount;
        }
        /// <summary>
        /// 房间Id
        /// </summary>
        public int RoomId { get; private set; }
        /// <summary>
        /// 玩家数量；
        /// </summary>
        public int PlayerCount { get; private set; }


        public void OnComplete(Action<BattleResultInfo[]> battleInfo)
        {
            GameEntry.BattleRoomManager.GetBattleRoomEntity(RoomId).OnBattleEnd += battleInfo;
        }
    }
}
