using AscensionProtocol;
using AscensionProtocol.DTO;
using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Module]
    public partial   class RoleAssetsManager : Cosmos.Module, IRoleAssetsManager
    {
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncRoleAssets, ProcessHandlerC2S);
        }
        void ProcessHandlerC2S(int seeionid, OperationData packet)
        {
            Utility.Debug.LogInfo("角色资产收到了"+ packet.DataMessage.ToString());
            var pointObj = Utility.Json.ToObject<RoleAssetsDTO>(packet.DataMessage.ToString());
            switch ((RoleAssetsOpCode)packet.SubOperationCode)
            {
                case RoleAssetsOpCode.AddAssets:

                    break;
                case RoleAssetsOpCode.GetAssets:
                    GetRoleAssetsS2C(pointObj.RoleID);
                    break;
                case RoleAssetsOpCode.ReduceAssets:

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 处理角色资产成功发送
        /// </summary>
        void RoleStatusSuccessS2C(int roleID, RoleAssetsOpCode oPcode, object data)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncRoleAssets;
            opData.SubOperationCode = (byte)oPcode;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        /// <summary>
        /// 处理角色资产失败发送
        /// </summary>
        void RoleStatusFailS2C(int roleID, RoleAssetsOpCode oPcode)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncRoleAssets;
            opData.SubOperationCode = (byte)oPcode;
            opData.DataMessage = Utility.Json.ToJson(null);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
    }
}
