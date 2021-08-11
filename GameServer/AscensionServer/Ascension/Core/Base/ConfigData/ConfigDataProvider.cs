using Cosmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Implementer]
    public class ConfigDataProvider : IDataProvider
    {
        string folderPath = Environment.CurrentDirectory + "/ConfigData";
        Dictionary<Type, object> objectDict = new Dictionary<Type, object>();
        Dictionary<string, string> jsonDict = new Dictionary<string, string>();
        public void LoadData()
        {
            jsonDict.Clear();
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            foreach (var f in dir.GetFiles())
            {
                var str = Utility.IO.ReadTextFileContent(folderPath, f.Name);
                jsonDict.Add(f.Name, str);
            }
            ParseData();
        }
        void ParseData()
        {
            var datSet = Utility.Assembly.GetDerivedTypesByAttribute<ConfigDataAttribute, Data>(true,GetType().Assembly);
            for (int i = 0; i < datSet.Length; i++)
            {
                string json;
                var fullName = Utility.Text.Append(datSet[i].Name, ".json");
                if (jsonDict.TryGetValue(fullName, out json))
                {
                    try
                    {
                        var obj = Utility.Json.ToObject(json, datSet[i]);
                        if (obj != null)
                        {
                            objectDict.TryAdd(datSet[i].GetType(), obj);
                            //Utility.Debug.LogWarning($"{obj.GetType()};\n{obj}");
                            GameEntry.DataManager.TryAdd(datSet[i], obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Utility.Debug.LogError($"IDataProvider ToObject fail . Type :{datSet[i].GetType().Name} ;{e}");
                    }
                }
            }
        }
    }
}
