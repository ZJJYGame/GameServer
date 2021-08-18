using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using RedisDotNet;
using AscensionProtocol;

namespace AscensionServer
{
    [Module]
    public partial class PuppetManager : Cosmos.Module, IPuppetManager
    {
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncPuppet, ProcessHandlerC2S);
        }
        void ProcessHandlerC2S(int seeionid, OperationData packet)
        {
            Utility.Debug.LogInfo("YZQ收到的人物傀儡" + packet.DataMessage.ToString());
            Utility.Debug.LogInfo("YZQ收到的人物傀儡" + (PuppetOpCode)packet.SubOperationCode);
            switch ((PuppetOpCode)packet.SubOperationCode)
            {
                case PuppetOpCode.GetPuppetStatus:
                    var role = Utility.Json.ToObject<RoleDTO>(packet.DataMessage.ToString());
                    GetPuppetStatusS2C(role.RoleID);
                    break;
                case PuppetOpCode.SetBattle:
                    var rolepuppet = Utility.Json.ToObject<RolePuppetDTO >(packet.DataMessage.ToString());
                    SetBattleS2C(rolepuppet.RoleID, rolepuppet.IsBattle);
                    break;
                case PuppetOpCode.AbandonPuppet:
                    var dict=Utility.Json.ToObject<Dictionary<byte,object>>(packet.DataMessage.ToString());
                    role = Utility.Json.ToObject<RoleDTO>(dict[(byte)ParameterCode.Role].ToString());
                    var puppetIndividual = Utility.Json.ToObject<PuppetIndividualDTO>(dict[(byte)ParameterCode.GetPuppetIndividual].ToString());
                    AbandonPuppetS2C(role.RoleID, puppetIndividual.ID);
                    break;
                default:
                    break;
            }
        }

        void PuppetManagerSuccessS2C(int roleID, PuppetOpCode oPcode, object data)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPuppet;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.Success;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);

            Utility.Debug.LogInfo("角色副职业数据发送了" + Utility.Json.ToJson(data));
        }

        void PuppetManagerFailS2C(int roleID, PuppetOpCode oPcode)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPuppet;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.ItemNotFound;
            opData.DataMessage = Utility.Json.ToJson(null);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        void PuppetManagerFailS2C(int roleID, PuppetOpCode oPcode,string str)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncPuppet;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.Fail;
            opData.DataMessage = str;
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
    }
}
