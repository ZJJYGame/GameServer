using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
namespace Protocol
{
    [Serializable]
    [MessagePackObject(true)]
    public class SessionRoleIdPair :  IDataContract, IDisposable
    {
        public int SessionId { get; set; }
        public int RoleId { get; set; }
        public SessionRoleIdPair() { }
        public SessionRoleIdPair(int sessionId, int playerId)
        {
            SessionId = sessionId;
            RoleId = playerId;
        }
        public void Dispose()
        {
            SessionId = 0;
            RoleId = 0;
        }
        public override string ToString()
        {
            return $"SessionId:{SessionId} ; RoleId:{RoleId}";
        }
    }
}
