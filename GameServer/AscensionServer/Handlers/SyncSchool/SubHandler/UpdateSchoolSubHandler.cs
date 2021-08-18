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
using RedisDotNet;

namespace AscensionServer
{
    public class UpdateSchoolSubHandler : SyncSchoolSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string schoolJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.School));
            var schoolObj = Utility.Json.ToObject<School>(schoolJson);
            NHCriteria  nHCriteriaschool=ReferencePool.Accquire<NHCriteria>().SetValue("ID", schoolObj.ID);
            var schooltemp= NHibernateQuerier.CriteriaSelect<School>(nHCriteriaschool);
            var content= RedisData.ReidsDataProcessing.GetData("School"+schooltemp.ID);
            if (schooltemp!=null)
            {
                if (string.IsNullOrEmpty(content))
                {
                    if (schoolObj.GetContributions >= 0)
                    {
                        schooltemp.ContributionNow += schoolObj.GetContributions;
                        schooltemp.IsSignin = schoolObj.IsSignin;
                        NHibernateQuerier.Update<School>(schooltemp);
                        SetResponseParamters(() =>
                        {
                            subResponseParameters.Add((byte)ParameterCode.School, Utility.Json.ToJson(schooltemp));
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
            }
            Utility.Debug.LogInfo("更新后的宗门信息" + Utility.Json.ToJson(schooltemp));
            ReferencePool.Release(nHCriteriaschool);
            return operationResponse;
        }
    }
}


