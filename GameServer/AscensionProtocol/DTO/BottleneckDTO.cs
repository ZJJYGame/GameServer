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
        public virtual int  RoleLevel { get; set; }
        public virtual bool IsThunder{ get; set; }
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
    }
}
