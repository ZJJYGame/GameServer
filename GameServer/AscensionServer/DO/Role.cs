/*
*Author : xianrenZhang
*Since :	2020-04-28
*Description :  角色表
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class Role:DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual byte RoleFaction { get; set; }
        public virtual bool RoleGender { get; set; }
        public virtual int RoleTalent { get; set; }
        public virtual string RoleRoot { get; set; }
        public virtual string RoleName { get; set; }
        public virtual int RoleLevel { get; set; }
        public virtual int roleExp { get; set; }
        public override void Release()
        {
            RoleID = -1;
            RoleFaction = 0;
            RoleTalent = 0;
            RoleRoot = null;
            RoleName = null;
            RoleLevel = 0;
            roleExp = 0;
        }
    }
}


