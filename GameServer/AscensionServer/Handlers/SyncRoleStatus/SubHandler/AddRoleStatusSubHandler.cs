using AscensionProtocol;
using AscensionServer.Model;
using Cosmos;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    public class AddRoleStatusSubHandler : SyncRoleStatusSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var RoleStatusData = Utility.GetValue(operationRequest.Parameters, (byte)ParameterCode.RoleStatus) as string;
            Utility.Debug.LogInfo(">>>>>添加自己的一些属性" + RoleStatusData + ">>>>>>>>>>>>>");
            var roleObj = Utility.Json.ToObject<RoleStatus>(RoleStatusData);
            NHCriteria nHCriteriaRoleId =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleObj.RoleID);
            bool exist = NHibernateQuerier.Verify<RoleStatus>(nHCriteriaRoleId);
            if (exist)
            {
                var  roleStatusSever =  NHibernateQuerier.CriteriaSelect<RoleStatus>(nHCriteriaRoleId);
                
                if (roleObj.RoleMaxHP != 0 )
                {
                    roleStatusSever.RoleMaxHP += roleObj.RoleMaxHP;
                }

                if (roleObj.RoleMP != 0)
                {
                    roleStatusSever.RoleMP += roleObj.RoleMP;
                }

                if (roleObj.PhysicalCritDamage != 0 )
                {
                    roleStatusSever.PhysicalCritDamage += roleObj.PhysicalCritDamage;
                }


                if (roleObj.DefendPhysical != 0)
                {
                    roleStatusSever.DefendPhysical += roleObj.DefendPhysical;
                }

                if (roleObj.AttackPower != 0)
                {
                    roleStatusSever.AttackPower += roleObj.AttackPower;
                }

                if (roleObj.DefendPower != 0 )
                {
                    roleStatusSever.DefendPower += roleObj.DefendPower;
                }

                if (roleObj.AttackSpeed != 0)
                {
                    roleStatusSever.AttackSpeed += roleObj.AttackSpeed;
                }


                NHibernateQuerier.Update(new RoleStatus()
                {
                    RoleID = roleObj.RoleID,
                    RoleHP = roleStatusSever.RoleHP,
                    RoleMaxMP = roleStatusSever.RoleMP,
                    RoleMaxHP = roleStatusSever.RoleMaxHP,            
                    RoleMP = roleStatusSever.RoleMP,                   
                });
                operationResponse.Parameters = subResponseParameters;
                operationResponse.ReturnCode = (short)ReturnCode.Success;
            }else operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriaRoleId);
            return operationResponse;
        }
    }
}


