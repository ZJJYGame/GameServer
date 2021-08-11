using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer.Model
{
    [Serializable]
    public class RoleAssets:DataObject
    {
        public RoleAssets()
        {
            this.SpiritStonesLow = 0;
            this.XianYu = 0;
        }
        public virtual int RoleID { get; set; }
        public virtual long SpiritStonesLow { get; set; }
        public virtual long XianYu { get; set; }
        public override string ToString()
        {
            return Utility.Text.Format("{ RoleID : " + RoleID + " ; " + " SpiritStonesLow : " + SpiritStonesLow 
                +  " ; XianYu"+ XianYu+"}");
        }
        public override void Release()
        {
            RoleID = -1;
            SpiritStonesLow = 0;
            XianYu = 0;
        }
    }
}


