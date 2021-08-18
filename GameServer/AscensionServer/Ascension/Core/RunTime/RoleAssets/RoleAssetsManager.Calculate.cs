using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using AscensionProtocol;
using RedisDotNet;
using Cosmos;
using AscensionServer.Model;

namespace AscensionServer
{
    public partial class RoleAssetsManager
    {
        void GetRoleAssetsS2C(int roleID)
        {
            var result = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._RoleAssetsPerfix,roleID.ToString()).Result;
            if (result)
            {
                var roleAssets= RedisHelper.Hash.HashGetAsync<RoleAssetsDTO>(RedisKeyDefine._RoleAssetsPerfix, roleID.ToString()).Result;
                if (roleAssets != null)
                {
                    RoleStatusSuccessS2C(roleID,RoleAssetsOpCode.GetAssets, roleAssets);
                }
                else
                    GetRoleAssetsMySql(roleID);
            }else
                GetRoleAssetsMySql(roleID);
        }

        void GetRoleAssetsMySql(int roleID)
        {
            NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            var role = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriarole);

            if (role != null)
            {
                RoleStatusSuccessS2C(roleID, RoleAssetsOpCode.GetAssets, role);

            }
            else
                RoleStatusFailS2C(roleID, RoleAssetsOpCode.GetAssets);
        }
    }
}
