using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Cosmos;
using NHibernate.Util;

namespace AscensionServer
{
    [Module]
    public class DataManager : Cosmos.Module, IDataManager
    {
        /// <summary>
        /// 间隔秒；
        /// </summary>
        int intervalSec = 3600;
        long latestRefreshTime;
        /// <summary>
        /// 对象字典；
        /// </summary>
          ConcurrentDictionary<Type, object> typeObjectDict = new ConcurrentDictionary<Type, object>();
        /// <summary>
        /// json数据字典；
        /// </summary>
        ConcurrentDictionary<string, string> jsonDict=new ConcurrentDictionary<string, string>();
        List<IDataProvider> providerSet = new List<IDataProvider>();
        protected override void OnActive()
        {
            {
                var objs = Utility.Assembly.GetInstancesByAttribute<ImplementerAttribute, IDataProvider>(GetType().Assembly);
                for (int i = 0; i < objs.Length; i++)
                {
                    objs[i]?.LoadData();
                }
                providerSet.AddRange(objs);
                latestRefreshTime = Utility.Time.SecondNow() + intervalSec;
            }
            {
                var objs = Utility.Assembly.GetInstancesByAttribute<ImplementerAttribute, IDataConvertor>(GetType().Assembly);
                for (int i = 0; i < objs.Length; i++)
                {
                    objs[i].ConvertData();
                }
            }
        }
        /// <summary>
        /// 覆写轮询函数；
        /// 自动更新服务器数据类型；
        /// </summary>
#if SERVER
        [TickRefresh]
        void OnRefresh()
        {
            //if (IsPause)
            //    return;
            //var now = Utility.Time.SecondNow();
            //if (now >= latestRefreshTime)
            //{
            //    RunProvider();
            //    latestRefreshTime = now + intervalSec;
            //}
        }
#endif
        public bool ContainsKey(Type key)
        {
            return typeObjectDict.ContainsKey(key);
        }
        public bool TryAdd(Type key, object value)
        {
            return typeObjectDict.TryAdd(key, value);
        }
        public bool TryGetValue(Type key, out object value)
        {
            return typeObjectDict.TryGetValue(key, out value);
        }
        public bool TryRemove(Type key)
        {
            return typeObjectDict.Remove(key);
        }
        public bool TryRemove(Type key, out object value)
        {
            return typeObjectDict.Remove(key, out value);
        }
        public bool ContainsKey<T>()
            where T : class
        {
            return ContainsKey(typeof(T));
        }
        public bool TryAdd<T>(T value)
            where T : class
        {
            return TryAdd(typeof(T), value);
        }
        public bool TryGetValue<T>(out T value)
            where T : class
        {
            value = default;
            object data;
            var result = TryGetValue(typeof(T), out data);
            if (result)
                value = data as T;
            return result;
        }
        public bool TryRemove<T>()
            where T : class
        {
            return TryRemove(typeof(T));
        }
        public bool TryRemove<T>(out T value)
            where T : class
        {
            value = default;
            object data;
            var result = TryRemove(typeof(T), out data);
            if (result)
                value = data as T;
            return result;
        }
        public bool TryAdd(string key, string value)
        {
            return jsonDict.TryAdd(key, value);
        }

        /// <summary>
        /// 通过类名获取json数据；
        /// typeof(Data).Name可作为key；
        /// </summary>
        /// <param name="key">类名</param>
        /// <param name="value">json数据</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetValue(string key, out string value)
        {
            return jsonDict.TryGetValue(key, out value);
        }
        public bool TryGetObjectValue<T>(string key, out T value)
            where T : class
        {
            value = default;
            string json;
            var result = jsonDict.TryGetValue(key, out json);
            if (result)
            {
                value = Utility.Json.ToObject<T>(json);
            }
            return result;
        }
        public bool TryRemove(string key)
        {
            return jsonDict.Remove(key);
        }
        public bool TryRemove(string key, out string value)
        {
            return jsonDict.Remove(key, out value);
        }
        public bool ContainsKey(string key)
        {
            return jsonDict.ContainsKey(key);
        }
        public void ClearAll()
        {
            jsonDict.Clear();
            typeObjectDict.Clear();
        }
        void RunProvider()
        {
            var length = providerSet.Count;
            for (int i = 0; i < length; i++)
            {
                providerSet[i]?.LoadData();
            }
        }
    }
}


