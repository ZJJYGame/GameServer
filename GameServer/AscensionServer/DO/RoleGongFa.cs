/*
*Author : xianrenZhang
*Since :	2020-04-28
*Description :  功法
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class RoleGongFa:DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual string GongFaIDDict { get; set; }
        public override void Release()
        {
            RoleID = -1;
            GongFaIDDict = null;
        }
    }
}


