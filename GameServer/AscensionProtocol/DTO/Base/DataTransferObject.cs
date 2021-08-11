using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos;
namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 数据传输通信协议DTO基类
    /// </summary>
    [Serializable]
    public abstract class DataTransferObject : IReference
    {
        public abstract void Release();
    }
}
