using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;

namespace AscensionServer
{
    public class GetWeaponSubHandler : SyncWeaponSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string weaponJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.GetWeapon));
            var weaponObj = Utility.Json.ToObject<RoleWeaponDTO>(weaponJson);
            NHCriteria nHCriteriaweapon=ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", weaponObj.RoleID);
            var weapontemp= NHibernateQuerier.CriteriaSelect<RoleWeapon>(nHCriteriaweapon);

            if (weapontemp!=null)
            {
                var WeaponDict = Utility.Json.ToObject<Dictionary<int, List<int>>>(weapontemp.WeaponStatusDict);
                SetResponseParamters(() =>
                {
                    subResponseParameters.Add((byte)ParameterCode.GetWeapon, Utility.Json.ToJson(WeaponDict));
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriaweapon);
            return operationResponse;
        }
    }
}


