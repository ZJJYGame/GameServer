using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class Bottleneck : DataObject
    {
        public virtual int RoleID { get; set; }
        public virtual bool IsBottleneck { get; set; }
        public virtual int RoleLevel { get; set; }
        public virtual bool IsThunder { get; set; }
        public virtual bool IsDemon { get; set; }
        public virtual int SpiritualRootVaule { get; set; }
        public virtual int ThunderRound { get; set; }
        public virtual int BreakThroughVauleNow { get; set; }
        public virtual int BreakThroughVauleMax { get; set; }
        public virtual int CraryVaule { get; set; }
        public virtual int DemonID { get; set; }
        public virtual int DrugNum { get; set; }
        public override void Release()
        {
            RoleID = -1;
            IsBottleneck = false;
            RoleLevel = -1;
            IsThunder = false;
            SpiritualRootVaule = -1;
            ThunderRound = -1;
            BreakThroughVauleNow = -1;
            BreakThroughVauleMax = -1;
            CraryVaule = -1;
            DemonID = -1;
            DrugNum = -1;
            IsDemon = false;
        }
        public Bottleneck()
        {

            IsBottleneck = false;
            RoleLevel = 0;
            IsThunder = false;
            SpiritualRootVaule =0;
            ThunderRound = 0;
            BreakThroughVauleNow = 0;
            BreakThroughVauleMax = 0;
            CraryVaule = 0;
            DemonID = 0;
            IsDemon = false;
            DrugNum = 0;
        }

    }
}


