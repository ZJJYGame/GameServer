using System.Collections;
using System.Collections.Generic;
using Cosmos;
using System;
using Cosmos.ECS;

namespace AscensionServer
{
    /// <summary>
    ///
    /// </summary>
    public class RoleEntity : Entity, IReference
    {
        public int RoleId { get { return (int)Id; } private set { Id = value; } }
        public int SessionId { get; private set; }
        public int DataCount { get { return dataDict.Count; } }
        Dictionary<Type, object> dataDict = new Dictionary<Type, object>();
        public object[] Find(Predicate<object> handler)
        {
            List<object> dataSet = new List<object>();
            dataSet.AddRange(dataDict.Values);
            return dataSet.FindAll(handler).ToArray();
        }
        public bool ContainsKey(Type key)
        {
            return dataDict.ContainsKey(key);
        }
        public bool TryAdd(Type key, object value)
        {
            return dataDict.TryAdd(key, value);
        }
        public bool TryGetValue(Type key, out object value)
        {
            return dataDict.TryGetValue(key, out value);
        }
        public bool TryGetValue<T>(out T value)
            where T:class
        {
            var result= dataDict.TryGetValue(typeof(T), out var val  );
            value = val as T;
            return result;
        }
        public bool TryRemove(Type key)
        {
            return dataDict.Remove(key);
        }
        public bool TryRemove(Type key, out object value)
        {
            return dataDict.Remove(key, out value);
        }
        public bool TryUpdate(Type key, object newValue, object comparsionValue)
        {
            if (newValue.GetType() != comparsionValue.GetType())
                return false;
            if (newValue.Equals(comparsionValue))
            {
                if (!dataDict.ContainsKey(key))
                    return false;
                dataDict[key] = newValue;
            }
            return false;
        }
        public void SendMessage(OperationData data)
        {
            GameEntry.PeerManager.SendMessage(SessionId, data);
        }
        public void SendMessage(byte opCode, Dictionary<byte, object> userData)
        {
            GameEntry.PeerManager.SendMessage(SessionId, opCode, userData);
        }
        public void SendMessage(byte opCode, short subCode, Dictionary<byte, object> userData)
        {
            GameEntry.PeerManager.SendMessage(SessionId, opCode, subCode, userData);
        }
        public void Release()
        {
            RoleId = 0;
            dataDict.Clear();
        }
        public override bool Equals(object obj)
        {
            var entity = obj as RoleEntity;
            if (entity == null)
                return false;
            return entity.RoleId == this.RoleId;
        }
        public static RoleEntity Create(int roleId, int sessionId, params object[] datas)
        {
            var entity =ReferencePool.Accquire<RoleEntity>();
            entity.RoleId = roleId;
            entity.SessionId = sessionId;
            for (int i = 0; i < datas.Length; i++)
            {
                entity.TryAdd(datas[i].GetType(), datas[i]);
            }
            return entity;
        }
        public static void Release(RoleEntity roleEntity)
        {
            ReferencePool.Release(roleEntity);
        }
    }
}

