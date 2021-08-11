using AscensionProtocol;
using AscensionProtocol.DTO;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Cosmos;
using System;
using System.Text;
using System.ServiceModel.Configuration;

namespace AscensionServer
{
    public class AscensionPeer : ClientPeer, IPeerEntity
    {
        #region Properties
        public int SessionId { get; private set; }
        public bool Available { get; private set; }
        public object Handle { get; private set; }
        SendParameters sendParam = new SendParameters();
        EventData eventData = new EventData();
        Dictionary<Type, object> dataDict = new Dictionary<Type, object>();
        OperationData operationData = new OperationData();
        #endregion
        #region Methods
        public AscensionPeer(InitRequest initRequest) : base(initRequest)
        {
            Handle = this; this.SessionId = ConnectionId;
            GameEntry.PeerManager.TryAdd(this);
            Utility.Debug.LogInfo($"Photon SessionId : {SessionId} Available . RemoteAdress:{initRequest.RemoteIP}");
        }
        public bool TryGetValue(Type key, out object value)
        {
            return dataDict.TryGetValue(key, out value);
        }
        public bool ContainsKey(Type key)
        {
            return dataDict.ContainsKey(key);
        }
        public bool TryRemove(Type Key)
        {
            return dataDict.Remove(Key, out _);
        }
        public bool TryRemove(Type key, out object value)
        {
            return dataDict.Remove(key, out value);
        }
        public bool TryAdd(Type key, object Value)
        {
            return dataDict.TryAdd(key, Value);
        }
        public bool TryUpdate(Type key, object newValue, object comparsionValue)
        {
            if (dataDict.ContainsKey(key))
            {
                var equal = dataDict[key].Equals(comparsionValue);
                if (equal)
                    dataDict[key] = newValue;
                return equal;
            }
            return false;
        }
        /// <summary>
        /// 发送消息到remotePeer
        /// </summary>
        public void SendMessage(OperationData opData)
        {
            var data = Utility.MessagePack.Serialize(opData);
            base.SendMessage(data, sendParam);
        }
        public void SendMessage(byte opCode, Dictionary<byte, object> userData)
        {
            operationData.Dispose();
            operationData.OperationCode = opCode;
            operationData.DataMessage = Utility.Json.ToJson(userData);
            var data = Utility.MessagePack.Serialize(operationData);
            base.SendMessage(data, sendParam);
        }
        public void SendMessage(byte opCode,short subCode, Dictionary<byte, object> userData)
        {
            operationData.Dispose();
            operationData.OperationCode = opCode;
            operationData.SubOperationCode = subCode;
            operationData.DataMessage = Utility.Json.ToJson(userData);
            var data = Utility.MessagePack.Serialize(operationData);
            base.SendMessage(data, sendParam);
        }
        /// <summary>
        /// 发送事件消息;
        /// 传输的数据类型限定为Dictionary<byte,object>类型；
        /// </summary>
        /// <param name="data">用户自定义数据</param>
        public void SendEventMsg(byte opCode, Dictionary<byte, object> data)
        {
            eventData.Code = opCode;
            eventData.Parameters = data;
            base.SendEvent(eventData, sendParam);
        }
        public void Release()
        {
            SessionId = 0;
            Available = false;
            dataDict.Clear();
        }
        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            GameEntry.PeerManager.TryRemove(SessionId);
            Utility.Debug.LogError($"Photon SessionId : {SessionId} Unavailable . RemoteAdress:{RemoteIPAddress}");
        }
        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            operationRequest.Parameters.Add((byte)ParameterCode.ClientPeer, this);
            object responseData = GameEntry.NetworkManager.EncodeMessage(operationRequest);
            var op = responseData as OperationResponse;
            op.OperationCode = operationRequest.OperationCode;
            SendOperationResponse(op, sendParameters);
        }
        /// <summary>
        /// 接收到客户端消息；
        /// </summary>
        protected override void OnMessage(object message, SendParameters sendParameters)
        {
            //接收到客户端消息后，进行委托广播；
            var opData = Utility.MessagePack.Deserialize<OperationData>(message as byte[]);
            CommandEventCore.Instance.Dispatch(opData.OperationCode,SessionId, opData);
        }
        #endregion
    }
}

