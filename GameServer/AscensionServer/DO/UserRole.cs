/* 
 * Time: 2020.4.28
 * Host:xianrenzhang
 * Content:关联uuid外键
 */
using System;
using System.Collections.Generic;
using Cosmos;
namespace AscensionServer.Model
{
    [Serializable]
    public class UserRole:DataObject
    {
        public virtual string UUID { get; set; }
        public virtual string RoleIDArray { get; set; }
        public override bool Equals(object obj)
        {
            if (!(obj is UserRole))
                return false;
            var tmpRole = obj as UserRole;
            if (this.RoleIDArray.Equals(tmpRole.RoleIDArray) && this.UUID.Equals(tmpRole.UUID))
                return true;
            else
                return false;
        }
        public override string ToString()
        {
            return Utility.Text.Format("UUID : " + UUID + ">>" + "Role_Id_Array : " + RoleIDArray + "\n");
        }
        public override void Release()
        {
            UUID = null;
            RoleIDArray = null;
        }
    }
}


