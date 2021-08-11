/*
*Author : xianrenZhang
*Since :	2020-04-28
*Description :  用户名
*/

using System.Collections.Generic;
using System;
namespace AscensionServer.Model
{
    [Serializable]
    public class User:DataObject
    {
        public virtual string Account { get; set; }
        public virtual string Password { get; set; }
        public virtual string UUID { get; set; }
        public override bool Equals(object obj)
        {
            User user = obj as User;
            return user.Account == this.Account && user.Password == this.Password ;
        }
        public override void Release()
        {
            Account = null;
            Password = null;
            UUID = null;
        }
    }

}


