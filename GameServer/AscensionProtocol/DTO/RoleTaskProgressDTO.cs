using System;
using System.Collections.Generic;
namespace AscensionProtocol.DTO
{
    [Serializable]
    public class RoleTaskProgressDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual Dictionary<string,RoleTaskItemDTO> RoleTaskInfoDic { get; set; }
        public override void Release()
        {
            RoleID = -1;
            RoleTaskInfoDic.Clear();
        }
    }
    [Serializable]
    public class RoleTaskItemDTO
    {
        public virtual string RoleTaskAchieveState { get; set; }
        public virtual string RoleTaskType { get; set; }
        public virtual string RoleTaskAcceptState { get; set; }
        public virtual string RoleTaskAbandonState { get; set; }
        public virtual string RoleTaskKind { get; set; }
    }
}

