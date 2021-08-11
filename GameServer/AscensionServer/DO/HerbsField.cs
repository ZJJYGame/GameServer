using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class HerbsField : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual int jobLevel { get; set; }
        public virtual string AllHerbs { get; set; }//以灵田下标为key值
        public HerbsField()
        {
            RoleID = 0;
            jobLevel = 4;
            AllHerbs = null;
        }


        public override void Release()
        {
            RoleID = -1;
            jobLevel = 0;
            AllHerbs=null;
        }
    }
 
}


