using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Cosmos;
using Pipelines.Sockets.Unofficial.Arenas;

namespace AscensionServer
{
    [Implementer]
    public class GateDataProvider : IDataConvertor
    {
        string folderPath = Environment.CurrentDirectory + "/JsonData";
        Dictionary<string, string> jsonDict = new Dictionary<string, string>();
        Dictionary<Type, object> objectDict = new Dictionary<Type, object>();
        public void ConvertData()
        {
            jsonDict.Clear();
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            char[] ch = ".txt".ToCharArray();
            foreach (var f in dir.GetFiles())
            {
                var str = Utility.IO.ReadTextFileContent(folderPath, f.Name);
                var pureStr = f.Name.TrimEnd(ch);
                jsonDict.Add(pureStr, str);
                GameEntry.DataManager.TryAdd(pureStr, str);
#if DEBUG
                //Utility.Debug.LogInfo($"\n{pureStr}\n{str}\n");
#endif
            }
            //GameEntry. DataManager.SetDataDict(jsonDict);
            ParseData();
        }
        void  ParseData()
        {
            objectDict.Clear();
            var datas = Utility.Assembly.GetDerivedTypesByAttribute<ConfigDataAttribute,Data>( true,GetType().Assembly);
            for (int i = 0; i < datas.Length; i++)
            {
                string json;
                //var fullName = Utility.Text.Append(datas[i].GetType().Name, ".txt");
                var fullName = datas[i].Name;
                if (jsonDict.TryGetValue(fullName, out json))
                {
                    try
                    {
                        var obj = Utility.Json.ToObject(json, datas[i]);
                        if (obj != null)
                        {
                            objectDict.TryAdd(datas[i], obj);
                            GameEntry.DataManager.TryAdd(datas[i], obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Utility.Debug.LogError($"IDataProvider ToObject fail . Type :{datas[i].GetType().Name} ;{e}");
                    }
                }
            }
            //GameEntry. DataManager.SetDataDict(objectDict);
        }
    }
}


