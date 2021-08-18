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
    public class AddSpiritualRunesSubHandler : SyncSpiritualRuneSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Add;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            var dict = operationRequest.Parameters;
            string spiritualRuneJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.JobSpiritualRunes));
            var spiritualRuneObj = Utility.Json.ToObject<SpiritualRunesDTO>(spiritualRuneJson);
            NHCriteria nHCriteriaspiritualRune =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", spiritualRuneObj.RoleID);
            var spiritualRuneTemp = NHibernateQuerier.CriteriaSelect<SpiritualRunes>(nHCriteriaspiritualRune);
            HashSet<int> spiritualRuneHash = new HashSet<int>();
            if (spiritualRuneTemp != null)
            {
                if (string.IsNullOrEmpty(spiritualRuneTemp.Recipe_Array))
                {
                    spiritualRuneTemp.Recipe_Array = Utility.Json.ToJson(spiritualRuneObj.Recipe_Array);
                    NHibernateQuerier.Update(spiritualRuneTemp);
                }
                else
                {
                    spiritualRuneHash = Utility.Json.ToObject<HashSet<int>>(spiritualRuneTemp.Recipe_Array);
                    spiritualRuneHash.Add(spiritualRuneObj.Recipe_Array.First());
                    spiritualRuneTemp.Recipe_Array = Utility.Json.ToJson(spiritualRuneHash);
                    NHibernateQuerier.Update(spiritualRuneTemp);
                }
                SetResponseParamters(() =>
                {
                    spiritualRuneObj = new SpiritualRunesDTO() { RoleID = spiritualRuneTemp.RoleID, JobLevel = spiritualRuneTemp.JobLevel, JobLevelExp = spiritualRuneTemp.JobLevelExp, Recipe_Array = spiritualRuneHash };

                    subResponseParameters.Add((byte)ParameterCode.JobSpiritualRunes, spiritualRuneObj);
                    operationResponse.ReturnCode = (short)ReturnCode.Success;
                });
            }
            else
                operationResponse.ReturnCode = (short)ReturnCode.Fail;
            ReferencePool.Release(nHCriteriaspiritualRune);
            return operationResponse;
        }
    }
}


