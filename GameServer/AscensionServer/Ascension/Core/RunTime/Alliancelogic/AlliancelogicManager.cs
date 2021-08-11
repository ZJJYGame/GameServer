using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventData = Photon.SocketServer.EventData;
using Cosmos;
using AscensionProtocol.DTO;
using AscensionServer.Model;

namespace AscensionServer
{
    public   class AlliancelogicManager:ConcurrentSingleton<AlliancelogicManager>
    {
        /// <summary>
        /// 储存登录盟主的字典，用于派发申请消息
        /// </summary>
        Dictionary<int, AscensionPeer> alliancePoolDict = new Dictionary<int, AscensionPeer>();
        /// <summary>
        /// 用于查询数据库数据的整合的函数
        /// </summary>
        /// <typeparam name="T">所需查询的类型</typeparam>
        /// <param name="keyname">映射的变量名</param>
        /// <param name="key">对应的id</param>
        /// <returns></returns>
        public T GetNHCriteria<T>(  string  keyname,int key)
        {
            NHCriteria nHCriteria  =ReferencePool.Accquire<NHCriteria>().SetValue(keyname, key);
            var dataObjectTemp = NHibernateQuerier.CriteriaSelectAsync<T>(nHCriteria).Result;

                ReferencePool.Release(nHCriteria);
                return dataObjectTemp;
        }
        /// <summary>
        /// 仙盟的申请整合方法
        /// </summary>
        /// <param name="role"></param>
        /// <param name="schoolDTO"></param>
        /// <returns></returns>
        public ApplyForAllianceDTO JointDate( Role role  , School schoolDTO)
        {
            ApplyForAllianceDTO applyForAllianceDTO = new ApplyForAllianceDTO() { RoleID = role.RoleID,MemberName=role.RoleName,Level= role .RoleLevel};
            return applyForAllianceDTO;
        }
    }
}


