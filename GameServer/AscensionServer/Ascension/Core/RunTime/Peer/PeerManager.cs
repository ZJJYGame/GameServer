using Cosmos;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using AscensionProtocol;

namespace AscensionServer
{
    /// <summary>
    /// <see cref="AscensionPeer"/>
    /// photon所有登录的peer都存储在此；
    /// SessionId在具体的Peer中，由服务器分配；
    /// 其余各个模块都是从此通过SessionID取得Peer对象；
    /// </summary>
    [Module]
    public class PeerManager : Cosmos.Module, IPeerManager
    {

        Action<int> onPeerConnected;
        public event Action<int> OnPeerConnected
        {
            add { onPeerConnected += value; }
            remove { OnPeerDisconnected += value; }
        }
        Action<int> onPeerDisconnected;
        public event Action<int> OnPeerDisconnected
        {
            add { onPeerDisconnected += value; }
            remove { onPeerDisconnected -= value; }
        }

        event Action<OperationData> BroadcastMsg1Param
        {
            add { broadcastMsg1Param += value; }
            remove { broadcastMsg1Param -= value; }
        }
        Action<OperationData> broadcastMsg1Param;

        event Action<byte, Dictionary<byte, object>> BroadcastMsg2Params
        {
            add { broadcastMsg2Params += value; }
            remove { broadcastMsg2Params -= value; }
        }
        Action<byte, Dictionary<byte, object>> broadcastMsg2Params;

        event Action<byte, short, Dictionary<byte, object>> BroadcastMsg3Params
        {
            add { broadcastMsg3Params += value; }
            remove { broadcastMsg3Params -= value; }
        }
        Action<byte, short, Dictionary<byte, object>> broadcastMsg3Params;

        ConcurrentDictionary<int, IPeerEntity> peerDict;


        protected override void OnInitialization()
        {
            peerDict = new ConcurrentDictionary<int, IPeerEntity>();
        }
        public bool TryAdd(IPeerEntity peer)
        {
            var result = peerDict.TryAdd(peer.SessionId, peer);
            if (result)
            {
                BroadcastMsg3Params += peer.SendMessage;
                BroadcastMsg2Params += peer.SendMessage;
                BroadcastMsg1Param += peer.SendMessage;
                onPeerConnected?.Invoke(peer.SessionId);
            }
            return result;
        }
        public bool TryAdd(int sessionId, IPeerEntity peer)
        {
            var result = peerDict.TryAdd(sessionId, peer);
            if (result)
            {
                BroadcastMsg3Params += peer.SendMessage;
                BroadcastMsg2Params += peer.SendMessage;
                BroadcastMsg1Param += peer.SendMessage;
                onPeerConnected?.Invoke(sessionId);
            }
            return result;
        }
        public bool TryRemove(int sessionId)
        {
            var result = peerDict.TryRemove(sessionId, out var peer);
            if (result)
            {
                BroadcastMsg3Params -= peer.SendMessage;
                BroadcastMsg2Params -= peer.SendMessage;
                BroadcastMsg1Param -= peer.SendMessage;
                onPeerDisconnected?.Invoke(sessionId);
            }
            return result;
        }
        public bool TryRemove(int sessionId, out IPeerEntity peer)
        {
            var result = peerDict.TryRemove(sessionId, out peer);
            if (result)
            {
                BroadcastMsg3Params -= peer.SendMessage;
                BroadcastMsg2Params -= peer.SendMessage;
                BroadcastMsg1Param -= peer.SendMessage;
                onPeerDisconnected?.Invoke(sessionId);
            }
            return result;
        }
        public bool ContainsKey(int sessionId)
        {
            return peerDict.ContainsKey(sessionId);
        }
        /// <summary>
        /// 将指定键的现有值与指定值进行比较，如果相等，则用第三个值更新该键。
        /// </summary>
        public bool TryUpdate(int sessionId, IPeerEntity newPeer, IPeerEntity comparisonPeer)
        {
            var result = peerDict.TryUpdate(sessionId, newPeer, comparisonPeer);
            if (result)
            {
                BroadcastMsg3Params -= comparisonPeer.SendMessage;
                BroadcastMsg2Params -= comparisonPeer.SendMessage;
                BroadcastMsg1Param -= comparisonPeer.SendMessage;
                BroadcastMsg3Params += newPeer.SendMessage;
                BroadcastMsg2Params += newPeer.SendMessage;
                BroadcastMsg1Param += newPeer.SendMessage;
            }
            return result;
        }
        public bool TryGetValue(int sessionId, out IPeerEntity peer)
        {
            return peerDict.TryGetValue(sessionId, out peer);
        }

        /// <summary>
        ///发送消息到具体的SessionId 
        /// </summary>
        public bool SendMessage(int sessionId, OperationData message)
        {
            var result = TryGetValue(sessionId, out var peer);
            if (result)
                peer.SendMessage(message);
            return result;
        }
        /// <summary>
        /// 同步方法；
        /// 通过广播消息(MSG)；
        ///若不存在session对象，则不发送，并返回false；
        /// </summary>
        public bool SendMessage(int sessionId, byte opCode, Dictionary<byte, object> userData)
        {
            var result = TryGetValue(sessionId, out var peer);
            if (result)
                peer.SendMessage(opCode, userData);
            return result;
        }
        /// <summary>
        /// 同步方法；
        /// 通过广播消息(MSG)；
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>是否能够发送</returns>
        public bool SendMessage(int sessionId, byte opCode,short subCode, Dictionary<byte, object> userData)
        {
            var result = TryGetValue(sessionId, out var peer);
            if (result)
                peer.SendMessage(opCode,subCode, userData);
            return result;
        }


        /// <summary>
        /// 同步方法；
        /// 通过广播消息(MSG)；
        /// </summary>
        /// <param name="message">普通消息</param>
        public void BroadcastMessageToAll(OperationData message)
        {
            broadcastMsg1Param?.Invoke(message);
        }
        /// <summary>
        /// 通过广播消息(MSG)；
        /// 此方法会对所有在线且Available的peer对象进行消息广播；
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="userData">用户自定义数据</param>
        public void BroadcastMessageToAll(byte opCode, Dictionary<byte, object> userData)
        {
            broadcastMsg2Params?.Invoke(opCode, userData);
        }
        /// <summary>
        /// 同步方法；
        /// 通过广播消息(MSG)；
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="userData">用户自定义数据</param>
        public void BroadcastMessageToAll(byte opCode,short subCode,Dictionary<byte, object> userData)
        {
            broadcastMsg3Params?.Invoke(opCode, subCode,userData);
        }


        /// <summary>
        /// 异步广播消息(MSG)；
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="message">普通消息</param>
        /// <returns>线程Task</returns>
        public async Task<bool> SendMessageAsync(int sessionId, OperationData message)
        {
            return await Task.Run(() => { return SendMessage(sessionId, message); });
        }
        /// <summary>
        ///异步广播消息到具体的sessionId 
        /// </summary>
        public async Task<bool> SendMessageAsync(int sessionId, byte opCode, Dictionary<byte, object> userData)
        {
            return await Task.Run(() => { return SendMessage(sessionId, opCode, userData); });
        }
        /// <summary>
        /// 异步广播消息(MSG)；
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>线程Task</returns>
        public async Task<bool> SendMessageAsync(int sessionId, byte opCode,short subCode, Dictionary<byte, object> userData)
        {
            return await Task.Run(() => { return SendMessage(sessionId, opCode,subCode, userData); });
        }


        /// <summary>
        /// 异步广播消息(MSG)；
        /// </summary>
        /// <param name="message">普通消息</param>
        /// <param name="callback">消息广播完成后的回调</param>
        /// <returns>线程Task</returns>
        public async Task BroadcastMessageToAllAsync(OperationData message, Action callback = null)
        {
            await Task.Run(() => { broadcastMsg1Param?.Invoke(message); });
            callback?.Invoke();
        }
        /// <summary>
        /// 异步广播消息(MSG)；
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="callback">广播结束后的回调</param>
        /// <returns>线程Task</returns>
        public async Task BroadcastMessageToAllAsync(byte opCode, Dictionary<byte, object> userData, Action callback = null)
        {
            await Task.Run(() => { broadcastMsg2Params?.Invoke(opCode, userData); });
            callback?.Invoke();
        }
        /// <summary>
        /// 异步广播消息(MSG)；
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="callback">广播结束后的回调</param>
        /// <returns>线程Task</returns>
        public async Task BroadcastMessageToAllAsync(byte opCode,short subCode, Dictionary<byte, object> userData, Action callback = null)
        {
            await Task.Run(() => { broadcastMsg3Params?.Invoke(opCode, subCode,userData); });
            callback?.Invoke();
        }

    }
}


