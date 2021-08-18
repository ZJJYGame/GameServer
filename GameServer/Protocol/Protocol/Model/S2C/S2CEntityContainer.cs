using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
namespace Protocol
{
    [Serializable]
    [MessagePackObject(true)]
    public class S2CContainer : IDataContract
    {
        public int ContainerId { get; set; }
        public List<SessionRoleIdPair> Players { get; set; }
        public int MSPerTick { get; set; }
    }
}
