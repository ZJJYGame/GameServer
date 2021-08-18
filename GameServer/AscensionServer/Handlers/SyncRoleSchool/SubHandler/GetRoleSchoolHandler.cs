using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class GetRoleSchoolHandler : SyncRoleSchoolSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Get;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string roleListJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.RoleSchool));
            var roleobj = Utility.Json.ToObject<List<int>>(roleListJson);
            Dictionary<int, School> schoolDict = new Dictionary<int, School>();
            if (roleobj.Count>0)
            {          
                foreach (var roleId in roleobj)
                {
                    NHCriteria nHCriteriarole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleId);
                    var roleschoolObj =NHibernateQuerier.CriteriaSelect<RoleSchool>(nHCriteriarole);
                    var verify= NHibernateQuerier.Verify<RoleSchool>(nHCriteriarole);
                    if (verify)
                    {
                        if (roleschoolObj.RoleJoiningSchool!=null)
                        {
                            NHCriteria nHCriteriaSchool =ReferencePool.Accquire<NHCriteria>().SetValue("ID", roleschoolObj.RoleJoiningSchool);
                            var schoolObj = NHibernateQuerier.CriteriaSelect<School>(nHCriteriaSchool);
                            schoolDict.Add(roleId, schoolObj);

                            ReferencePool.Release(nHCriteriaSchool);
                        }else
                            schoolDict.Clear();

                    }
                    else
                    {
                        SetResponseParamters(() =>
                        {
                         operationResponse.ReturnCode = (byte)ReturnCode.Fail;
                        });
                    }
                    ReferencePool.Release(nHCriteriarole);
                }
             
                if (schoolDict.Count == roleobj.Count)
                {
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.RoleSchool, Utility.Json.ToJson(schoolDict));
                        operationResponse.ReturnCode = (byte)ReturnCode.Success;
                    });
                }
                else
                {
                    SetResponseParamters(() =>
                    {
                        operationResponse.ReturnCode = (byte)ReturnCode.Fail;
                    });
                }
            }
            Utility.Debug.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>..发送加入的宗门" + Utility.Json.ToJson(schoolDict));
            return operationResponse;
        }
    }
}


