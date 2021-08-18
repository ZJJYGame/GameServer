//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Photon.SocketServer;
//using AscensionProtocol;
//using AscensionServer.Model;
//using NHibernate.Linq.Clauses;
//using AscensionProtocol.DTO;
//using Renci.SshNet.Security;
//using Cosmos;

//namespace AscensionServer
//{
//    public class AddWeaponSubHandler : SyncWeaponSubHandler
//    { 
//        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

//        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
//        {
//            var dict = operationRequest.Parameters;
//            string weaponJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.GetWeapon));
//            var weaponObj = Utility.Json.ToObject<RoleWeaponDTO>(weaponJson);
//            NHCriteria nHCriteriaweapon =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", weaponObj.RoleID);
//            var weaponTemp = NHibernateQuerier.CriteriaSelect<Weapon>(nHCriteriaweapon);

//            int index=1;
//            Dictionary<int, int> indexDict = new Dictionary<int, int>();
//            Dictionary<int, List<int>> WeaponDict = new Dictionary<int, List<int>>();
//            if (weaponTemp != null)
//            {
//                if (!string.IsNullOrEmpty(weaponTemp.Weaponindex)&& !weaponTemp.Weaponindex.Equals("{}"))
//                {

//                    Utility.Debug.LogInfo("添加后的武器数值为" + weaponTemp.Weaponindex);
//                    indexDict = Utility.Json.ToObject<Dictionary<int, int>>(weaponTemp.Weaponindex);
//                    WeaponDict= Utility.Json.ToObject<Dictionary<int, List<int>>>(weaponTemp.WeaponStatusDict);
//                    Utility.Debug.LogInfo("添加后的武器数值为" + indexDict.Count);
//                    if (indexDict.TryGetValue(weaponObj.WeaponID, out index))
//                    {
//                        Utility.Debug.LogInfo("1添加后的武器数值为" + index);
//                        WeaponDict.Add(Convert.ToInt32(weaponObj.WeaponID + "" + (index+1)), weaponObj.WeaponStatusDict[weaponObj.WeaponID]);
//                        index = Convert.ToInt32(weaponObj.WeaponID + "" + (index + 1));
//                        weaponTemp.WeaponStatusDict = Utility.Json.ToJson(WeaponDict);
//                        indexDict[weaponObj.WeaponID] += 1;
//                        weaponTemp.Weaponindex = Utility.Json.ToJson(indexDict);
//                        NHibernateQuerier.Update(weaponTemp);
//                    }
//                    else
//                    {
//                        Utility.Debug.LogInfo("2添加后的武器数值为" + weaponTemp.RoleID);
//                        WeaponDict.Add(Convert.ToInt32(weaponObj.WeaponID + "" + 1), weaponObj.WeaponStatusDict[weaponObj.WeaponID]);
//                        index = Convert.ToInt32(weaponObj.WeaponID + "" + 1);
//                        weaponTemp.WeaponStatusDict = Utility.Json.ToJson(WeaponDict);
//                        indexDict.Add(weaponObj.WeaponID, 1);
//                        weaponTemp.Weaponindex = Utility.Json.ToJson(indexDict);
//                        NHibernateQuerier.Update(weaponTemp);
//                    }
//                }
//                else
//                {
//                    Utility.Debug.LogInfo("3添加后的武器数值为" + weaponTemp.RoleID);
//                    WeaponDict.Add(Convert.ToInt32(weaponObj.WeaponID + "" + 1), weaponObj.WeaponStatusDict[weaponObj.WeaponID]);
//                    index = Convert.ToInt32(weaponObj.WeaponID + "" + 1);
//                    weaponTemp.WeaponStatusDict = Utility.Json.ToJson(WeaponDict);
//                    indexDict.Add(weaponObj.WeaponID, 1);
//                    weaponTemp.Weaponindex = Utility.Json.ToJson(indexDict);
//                    NHibernateQuerier.Update(weaponTemp);
//            }
//            SetResponseParamters(() =>
//                {
//                    subResponseParameters.Add((byte)ParameterCode.GetWeapon, weaponTemp.WeaponStatusDict);
//                    subResponseParameters.Add((byte)ParameterCode.GetWeaponindex, Utility.Json.ToJson(index));
//                    operationResponse.ReturnCode = (short)ReturnCode.Success;
//                });
//            }
//            else
//                operationResponse.ReturnCode = (short)ReturnCode.Fail;
//            ReferencePool.Release(nHCriteriaweapon);
//            return operationResponse;
//        }
//    }
//}


