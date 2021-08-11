using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public  class RoleMiShuDTO: DataTransferObject
    {
        public virtual int RoleID { get; set; }
        public virtual Dictionary<int, MiShuDTO> MiShuIDDict { get; set; }
        public RoleMiShuDTO()
        {
            RoleID = -1;
            MiShuIDDict = new Dictionary<int, MiShuDTO>();
        }

        public override void Release()
        {
            RoleID = -1;
            MiShuIDDict = new Dictionary<int, MiShuDTO>();
        }

    }
    [Serializable]
    public class MiShuDTO : DataTransferObject
    {
        public virtual int MiShuID { get; set; }
        public virtual int MiShuExp { get; set; }
        public virtual short MiShuLevel { get; set; }
        public virtual List<int> MiShuSkillArry { get; set; }
        public virtual List<int> MiShuAdventureSkill { get; set; }

        public MiShuDTO()
        {

            MiShuID = 0;
            MiShuExp = 0;
            MiShuLevel = 0;
            MiShuSkillArry = new List<int>();
            MiShuAdventureSkill = new List<int>();
        }

        public override void Release()
        {

            MiShuID = 0;
            MiShuExp = 0;
            MiShuLevel = 0;
            MiShuSkillArry.Clear();
        }
    }
}
