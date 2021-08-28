using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionProtocol;
using AscensionServer.Model;
using RedisDotNet;

namespace AscensionServer
{
    [Module]
    public class LoginManager : Cosmos.Module, ILoginManager
    {
        Type createRoleHelperType;
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.LoginArea, ProcessHandlerC2S);
            createRoleHelperType = Utility.Assembly.GetDerivedTypesByAttribute<ImplementerAttribute, ICreateRoleHelper>(GetType().Assembly)[0];
        }
        /// <summary>
        /// 获取账号下的角色ID；
        /// </summary>
        /// <param name="user">需要查询的用户</param>
        /// <returns>账号对应的角色ID数组</returns>
        public int[] GetAccountRolesId(User user)
        {
            string _uuid = NHibernateQuerier.Query<User>("Account", user.Account).UUID;
            var userRoleObj = NHibernateQuerier.Query<UserRole>("UUID", _uuid);
            string roleIdListJson = userRoleObj.RoleIDArray;
            int length = 0;
            if (!string.IsNullOrEmpty(roleIdListJson))
            {
                var roleIdListStr = Utility.Json.ToObject<List<string>>(roleIdListJson);
                length = roleIdListStr.Count;
                var roleIdListInt = new List<int>();
                for (int i = 0; i < length; i++)
                {
                    roleIdListInt.Add(int.Parse(roleIdListStr[i]));
                }
                return roleIdListInt.ToArray();
            }
            return null;
        }
        void ProcessHandlerC2S(int sessionId, OperationData packet)
        {
            var areaSubCode = (LoginAreaOpCode)packet.SubOperationCode;
            var dict = Utility.Json.ToObject<Dictionary<byte, object>>(Convert.ToString(packet.DataMessage));
            switch (areaSubCode)
            {
                case LoginAreaOpCode.GetAccountRoles:
                    {
                        GetAccountRolesS2C(sessionId, dict);
                    }
                    break;
                case LoginAreaOpCode.CreateRole:
                    {
                        CreateRoleS2C(sessionId, dict);
                    }
                    break;
                case LoginAreaOpCode.LoginRole:
                    {
                        LoginRoleS2C(sessionId, dict);
                    }
                    break;
                case LoginAreaOpCode.LogoffRole:
                    {
                        LogoffRoleS2C(sessionId, dict);
                    }
                    break;
            }
        }
        /// <summary>
        ///客户端从服务器端获取当前账号下拥有角色的信息；
        /// </summary>
        /// <param name="packet">进入的请求数据</param>
        void GetAccountRolesS2C(int sessionId, Dictionary<byte, object> dataMessage)
        {
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.LoginArea,
                SubOperationCode = (short)LoginAreaOpCode.GetAccountRoles
            };
            var messageDict = new Dictionary<byte, object>();
            //验证角色；
            var userObj = Utility.Json.ToObject<User>(Convert.ToString(Utility.GetValue(dataMessage, (byte)ParameterCode.User)));
            var roleIdList = GetAccountRolesId(userObj);
            if (roleIdList == null)
            {
                opData.ReturnCode = (short)ReturnCode.Empty;
            }
            else
            {
                List<Role> roleObjList = new List<Role>();
                for (int i = 0; i < roleIdList.Length; i++)
                {
                    Role tmpRole = NHibernateQuerier.Query<Role>("RoleID", roleIdList[i]);
                    roleObjList.Add(tmpRole);
                }
                messageDict.Add((byte)ParameterCode.RoleSet, Utility.Json.ToJson(roleObjList));
                opData.ReturnCode = (short)ReturnCode.Success;
            }
            opData.DataMessage = Utility.Json.ToJson(messageDict);
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
        void CreateRoleS2C(int sessionId, Dictionary<byte, object> dataMessage)
        {
            var createRoleHelper = ReferencePool.Accquire(createRoleHelperType) as ICreateRoleHelper;
            var opData = createRoleHelper.CreateRole(dataMessage);
            opData.OperationCode = (byte)OperationCode.LoginArea;
            opData.SubOperationCode = (short)LoginAreaOpCode.CreateRole;
            GameEntry.PeerManager.SendMessage(sessionId, opData);
            ReferencePool.Release(createRoleHelper);
        }
        void LoginRoleS2C(int sessionId, Dictionary<byte, object> dataMessage)
        {
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.LoginArea,
                SubOperationCode = (short)LoginAreaOpCode.LoginRole
            };
            var json = Convert.ToString(Utility.GetValue(dataMessage, (byte)ParameterCode.Role));
            var roleObj = Utility.Json.ToObject<RoleDTO>(json);
            var role = RoleEntity.Create(roleObj.RoleID, sessionId, roleObj);
            var roleExist = GameEntry.RoleManager.TryGetValue(roleObj.RoleID, out var remoteRole);
            if (roleExist)
            {
                if (GameEntry.PeerManager.TryGetValue(remoteRole.SessionId, out var pa))
                {
                    pa.SendMessage((byte)OperationCode.LoginArea, (short)LoginAreaOpCode.LogoffRole, null);//从这里发送挤下线消息；
                    GameEntry.RoleManager.TryRemove(roleObj.RoleID);
                }
            }
            IPeerEntity peerAgent;
            var result = GameEntry.PeerManager.TryGetValue(sessionId, out peerAgent);
            if (result)
            {
                var remoteRoleType = typeof(RoleEntity);
                var exist = peerAgent.ContainsKey(remoteRoleType);
                if (!exist)
                {
                    GameEntry.RoleManager.TryAdd(roleObj.RoleID, role);
                    #region 
                    var roleAllianceobj = NHibernateQuerier.Query<RoleAlliance>("RoleID", roleObj.RoleID);
                    if (roleAllianceobj != null)
                    {
                        var allianceStatusobj = NHibernateQuerier.Query<AllianceStatus>("ID", roleAllianceobj.AllianceID);

                        if (allianceStatusobj != null)
                        {
                            allianceStatusobj.OnLineNum += 1;
                            NHibernateQuerier.Update(allianceStatusobj);
                        }
                        roleAllianceobj.Offline = "在线";
                        NHibernateQuerier.Update(roleAllianceobj);
                        RoleAllianceDTO allianceDTO = new RoleAllianceDTO();
                        allianceDTO.AllianceID = roleAllianceobj.AllianceID;
                        allianceDTO.AllianceJob = roleAllianceobj.AllianceJob;
                        allianceDTO.ApplyForAlliance = Utility.Json.ToObject<List<int>>(roleAllianceobj.ApplyForAlliance);
                        allianceDTO.JoinTime = roleAllianceobj.JoinTime;
                        allianceDTO.Offline = roleAllianceobj.Offline;
                        allianceDTO.Reputation = roleAllianceobj.Reputation;
                        allianceDTO.ReputationHistroy = roleAllianceobj.ReputationHistroy;
                        allianceDTO.ReputationMonth = roleAllianceobj.ReputationMonth;
                        allianceDTO.RoleID = roleAllianceobj.RoleID;
                        allianceDTO.RoleName = roleAllianceobj.RoleName;
                        RedisHelper.Hash.HashSet(RedisKeyDefine._RoleAlliancePerfix, roleAllianceobj.RoleID.ToString(), allianceDTO);
                    }
                    #endregion
                    peerAgent.TryAdd(remoteRoleType, role);
                    opData.ReturnCode = (byte)ReturnCode.Success;
                }
                else
                {
                    object legacyRole;
                    peerAgent.TryGetValue(remoteRoleType, out legacyRole);
                    var remoteRoleObj = legacyRole as RoleEntity;
                    var updateResult = peerAgent.TryUpdate(remoteRoleType, role, legacyRole);
                    if (updateResult)
                    {
                        #region 
                        var roleAllianceobj = NHibernateQuerier.Query<RoleAlliance>("RoleID", roleObj.RoleID);
                        if (roleAllianceobj != null)
                        {
                            var allianceStatusobj = NHibernateQuerier.Query<AllianceStatus>("ID", roleAllianceobj.AllianceID);
                            if (allianceStatusobj != null)
                            {
                                NHibernateQuerier.Update(allianceStatusobj);
                            }
                            roleAllianceobj.Offline = "在线";
                            NHibernateQuerier.Update(roleAllianceobj);
                            RoleAllianceDTO allianceDTO = new RoleAllianceDTO();
                            allianceDTO.AllianceID = roleAllianceobj.AllianceID;
                            allianceDTO.AllianceJob = roleAllianceobj.AllianceJob;
                            allianceDTO.ApplyForAlliance = Utility.Json.ToObject<List<int>>(roleAllianceobj.ApplyForAlliance);
                            allianceDTO.JoinTime = roleAllianceobj.JoinTime;
                            allianceDTO.Offline = roleAllianceobj.Offline;
                            allianceDTO.Reputation = roleAllianceobj.Reputation;
                            allianceDTO.ReputationHistroy = roleAllianceobj.ReputationHistroy;
                            allianceDTO.ReputationMonth = roleAllianceobj.ReputationMonth;
                            allianceDTO.RoleID = roleAllianceobj.RoleID;
                            allianceDTO.RoleName = roleAllianceobj.RoleName;
                            RedisHelper.Hash.HashSet(RedisKeyDefine._RoleAlliancePerfix, roleAllianceobj.RoleID.ToString(), allianceDTO);
                        }
                        #endregion
                        GameEntry.RoleManager.TryRemove(roleObj.RoleID);
                        GameEntry.RoleManager.TryAdd(roleObj.RoleID, role);
                        opData.ReturnCode = (byte)ReturnCode.Success;//登录成功
                        //GameEntry.RecordManager.RecordRole(remoteRoleObj as RoleEntity);
                        ReferencePool.Release(remoteRoleObj);//回收这个RemoteRole对象
                    }
                    else
                    {
                        opData.ReturnCode = (byte)ReturnCode.Fail;//登录失败
                    }
                }
            }
            else
                opData.ReturnCode = (byte)ReturnCode.Fail;//登录失败
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
        void LogoffRoleS2C(int sessionId, Dictionary<byte, object> dataMessage)
        {
            var opData = new OperationData()
            {
                OperationCode = (byte)OperationCode.LoginArea,
                SubOperationCode = (short)LoginAreaOpCode.LogoffRole
            };
            IPeerEntity peer;
            GameEntry.PeerManager.TryGetValue(sessionId, out peer);
            var json = Convert.ToString(Utility.GetValue(dataMessage, (byte)ParameterCode.Role));
            var roleObj = Utility.Json.ToObject<RoleDTO>(json);
            IPeerEntity peerAgent;
            var result = GameEntry.PeerManager.TryGetValue(peer.SessionId, out peerAgent);
            if (result)
            {
                var remoteRoleType = typeof(RoleEntity);
                object remoteRoleObj;
                var removeResult = peerAgent.TryRemove(remoteRoleType, out remoteRoleObj);
                if (removeResult)
                {
                    var remoteRole = remoteRoleObj as RoleEntity;
                    ReferencePool.Release(remoteRole);//回收这个RemoteRole对象
                    GameEntry.RoleManager.TryRemove(roleObj.RoleID);
                }
                opData.ReturnCode = (byte)ReturnCode.Success;
                //GameEntry.RecordManager.RecordRole(remoteRoleObj as RoleEntity);
            }
            else
            {
                opData.ReturnCode = (byte)ReturnCode.ItemNotFound;
            }
            GameEntry.PeerManager.SendMessage(sessionId, opData);
        }
    }
}
