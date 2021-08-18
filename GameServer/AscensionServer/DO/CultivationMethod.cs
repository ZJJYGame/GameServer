/*
*Author : yingduan
*Since :	2020-05-14
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
    public class CultivationMethod : DataObject
    {
        public CultivationMethod()
        {
            CultivationMethodID = 0;
            CultivationMethodExp = 0;
            CultivationMethodLevel = 1;
            CultivationMethodLevelSkillArray="{}";
        }
        public virtual int ID { get; set; }
        public virtual int CultivationMethodID { get; set; }
        public virtual int CultivationMethodExp { get; set; }
        public virtual short CultivationMethodLevel { get; set; }
        public virtual string CultivationMethodLevelSkillArray { get; set; }

        public override void Release()
        {
            ID = -1;
            CultivationMethodID = 0;
            CultivationMethodExp = 0;
            CultivationMethodLevel = 0;
            CultivationMethodLevelSkillArray = null;
        }
    }
}



