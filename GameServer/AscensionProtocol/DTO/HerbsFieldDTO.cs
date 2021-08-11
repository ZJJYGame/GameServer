using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class HerbsFieldDTO : DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual int jobLevel { get; set; }
        //public virtual string  AllHerbs { get; set; }//list下标为灵田下标
        public virtual List<HerbFieldStatus> AllHerbs { get; set; }//list下标为灵田下标

        public override void Release()
        {
            RoleID = -1;
            jobLevel = 0;
            AllHerbs=null;
        }
    }
    [Serializable]
    public class HerbFieldStatus
    {
        public virtual int ArrayID { get; set; }//list下标记录
        public virtual bool IsStratPlant { get; set; }//用於判斷開始種植記錄種植時間
        public virtual int HerbsID { get; set; }
        public virtual int HerbsGrowthValue { get; set; }//成长值
        public virtual int FieldLevel { get; set; }
        public virtual int plantingTime { get; set; }//记录灵草成熟剩余时间
    }

}
