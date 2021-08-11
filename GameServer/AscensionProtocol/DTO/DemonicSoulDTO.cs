using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class DemonicSoulDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual  Dictionary<int, DemonicSoulEntity> DemonicSouls { get; set; }
        public virtual DemonicSoulOperateType OperateType { get; set; }
        public virtual List<int> CompoundList { get; set; }
        public override void Release()
        {
            RoleID = -1;
            DemonicSouls.Clear();
            CompoundList.Clear();
            OperateType = DemonicSoulOperateType.None;
        }
    }

    [Serializable]
    public class DemonicSoulEntity : DataTransferObject
    {
        public virtual int GlobalID { get; set; }
        public virtual int UniqueID { get; set; }
        public virtual List<int> Skills { get; set; }

        public override void Release()
        {
            GlobalID = 0;
            UniqueID = 0;
            Skills.Clear();
        }
    }

    public enum DemonicSoulOperateType
    {
        None=0,
        Add=1,
        Compound=2,
        Get=3,
    }
}
