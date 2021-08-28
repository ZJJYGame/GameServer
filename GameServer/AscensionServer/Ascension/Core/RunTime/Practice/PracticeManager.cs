using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionProtocol;
using AscensionServer.Model;

namespace AscensionServer
{
    [Module]
    public partial  class PracticeManager: Cosmos.Module, IPracticeManager
    {
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncPractice, ProcessHandlerC2S);
        }

        void ProcessHandlerC2S(int seeionid ,OperationData packet)
        {
            var dict = Utility.Json.ToObject<Dictionary<byte, object>>(Convert.ToString(packet.DataMessage));
            Utility.Debug.LogInfo("YZQjueseid为" + Convert.ToString(packet.DataMessage));
            RoleDTO role;
            OnOffLineDTO onOffLine;
            BottleneckDTO bottleneckDTO;
            foreach (var item in dict)
            {
                switch ((PracticeOpcode)item.Key)
                {
                    case PracticeOpcode.GetRoleGongfa:
                        role = Utility.Json.ToObject<RoleDTO>(item.Value.ToString());
                        GetRoleGongFaS2C(role.RoleID);
                        break;
                    case PracticeOpcode.AddGongFa:
                        var secondary = Utility.Json.ToObject<SecondaryJobDTO>(item.Value.ToString());
                        AddGongFaS2C(secondary.RoleID, secondary.UseItemID);
                        break;
                    case PracticeOpcode.GetRoleMiShu:
                        role = Utility.Json.ToObject<RoleDTO>(item.Value.ToString());
                        GetRoleMiShuS2C(role.RoleID);
                        break;
                    case PracticeOpcode.AddMiShu:
                         secondary = Utility.Json.ToObject<SecondaryJobDTO>(item.Value.ToString());
                        AddMiShuS2C(secondary.RoleID, secondary.UseItemID);
                        break;
                    case PracticeOpcode.SwitchPracticeType:
                        onOffLine = Utility.Json.ToObject<OnOffLineDTO>(item.Value.ToString());
                        SwitchPracticeTypeS2C(onOffLine);
                        break;
                    case PracticeOpcode.UploadingExp:
                        var obj = Utility.Json.ToObject<OnOffLineDTO>(item.Value.ToString());
                        Utility.Debug.LogInfo("YZQ更新挂机经验收到请求了"+Utility.Json.ToJson(obj));
                        UploadingExpS2C(obj);
                        break;
                    case PracticeOpcode.GetOffLineExp:
                        role = Utility.Json.ToObject<RoleDTO>(item.Value.ToString());
                        GetOffLineExpS2C(role.RoleID);
                        break;
                    case PracticeOpcode.UseBottleneckElixir:
                        bottleneckDTO=Utility.Json.ToObject<BottleneckDTO>(item.Value.ToString());
                        UseBottleneckElixir(bottleneckDTO.RoleID, bottleneckDTO.DrugID);
                        break;
                    case PracticeOpcode.UpdateBottleneck://突破使用
                        role = Utility.Json.ToObject<RoleDTO>(item.Value.ToString());
                        UpdateBottleneckAsync(role);
                        break;
                    case PracticeOpcode.DemonicBattle:
                        role = Utility.Json.ToObject<RoleDTO>(item.Value.ToString());
                        DemonicBattle(role);
                        break;
                    case PracticeOpcode.ThunderRoundBattle:
                        role = Utility.Json.ToObject<RoleDTO>(item.Value.ToString());
                        ThunderRoundBattle(role);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 失败返回
        /// </summary>
        void ResultFailS2C(int roleID, PracticeOpcode opcode,string tips=null)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPractice;
            opData.ReturnCode = (short)ReturnCode.Fail;
            opData.SubOperationCode = (byte)opcode;
            opData.DataMessage = tips;
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="opcode"></param>
        /// <param name="tips"></param>
        void ResultEmptyS2C(int roleID, PracticeOpcode opcode, string tips = null)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPractice;
            opData.ReturnCode = (short)ReturnCode.Empty;
            opData.SubOperationCode = (byte)opcode;
            opData.DataMessage = tips;
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        /// <summary>
        /// 数据未找到返回
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="opcode"></param>
        /// <param name="tips"></param>
        void ResultNotFoundS2C(int roleID, PracticeOpcode opcode, string tips = null)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPractice;
            opData.ReturnCode = (short)ReturnCode.ItemNotFound;
            opData.SubOperationCode = (byte)opcode;
            opData.DataMessage = tips;
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        /// <summary>
        /// 结果成功返回
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="opcode"></param>
        /// <param name="data"></param>
        void ResultSuccseS2C(int roleID, PracticeOpcode opcode,object data)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPractice;
            opData.ReturnCode = (short)ReturnCode.Success;
            opData.SubOperationCode = (byte)opcode;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);
            Utility.Debug.LogInfo("yzqjueseid发送成功" + Utility.Json.ToJson(opData));
        }
    
    }
}
