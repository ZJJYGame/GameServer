using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    /// <summary>
    /// 角色id
    /// 是否瓶颈
    /// 角色等级
    /// 是否渡天劫
    /// 是否渡心劫
    /// 灵根数
    /// 雷劫回合数
    /// 瓶颈突破值
    /// 煞气值
    /// 心魔ID
    /// </summary>
    [Serializable]
    public class BottleneckDTO : DataTransferObject
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
        public virtual int DrugPercent { get; set; }
        public virtual int DrugID { get; set; }

        public BottleneckDTO()
            {
            IsBottleneck = false;
            RoleLevel = 0;
            IsThunder = false;
            SpiritualRootVaule = 0;
            ThunderRound = 0;
            BreakThroughVauleNow = 0;
            BreakThroughVauleMax = 0;
            CraryVaule = 0;
            DemonID = 0;
            DrugID = 0;
            IsDemon = false;
            DrugPercent = 0;
        }

        public override void Release()
        {
            RoleID = -1;
            IsBottleneck = false;
            RoleLevel = 0;
            IsThunder = false;
            SpiritualRootVaule = 0;
            ThunderRound = 0;
            BreakThroughVauleNow = 0;
            BreakThroughVauleMax = 0;
            CraryVaule = 0;
            DemonID = 0;
            DrugID =0;
            IsDemon = false;
            DrugPercent = 0;
        }
    }
}
