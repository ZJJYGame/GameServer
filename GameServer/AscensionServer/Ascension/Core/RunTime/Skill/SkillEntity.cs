using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using AscensionProtocol;
using Cosmos.ECS;

namespace AscensionServer 
{
    /// <summary>
    /// 技能实体对象；
    /// 技能的CD、持续时间等使用此作为容器进行运算；
    /// 此实体持有技能数据，
    /// </summary>
    public class SkillEntity : Entity, IReference,IRefreshable
    {
        //public IDataContract DataContract { get; set; }
        ConcurrentDictionary<int, SkillVariable> skillDict = 
            new ConcurrentDictionary<int, SkillVariable>();
        IDataVerifyHelper dataVerifier=new SkillDataVerifyHelper();
        protected SkillEntity(){}
        protected SkillEntity(int id) { base.Id = id; }
        //public virtual void OnCastSkill(IDataContract data)
        //{
        //    var skill = data as C2SSkillInput;
        //    var hasSkillVar= skillDict.TryGetValue(skill.SkillId, out var skillVar);
        //    if (hasSkillVar)
        //    {
        //        var verified= dataVerifier.VerifyData(skill);
        //        if (verified)
        //        {
        //            var opData = new OperationData((byte)OperationCode.SyncRoleAdventureSkill);
        //            //opData.BinParameters = skill;
        //            //GameManager.CustomeModule<LevelManager>()
        //            //    .SendMsg2AllLevelRoleS2C(skill.EntityContainer.EntityContainerId,opData);
        //        }
        //    }
        //}
        //public virtual void OnInit(IDataContract data)
        //{
        //    player= data as SessionRoleIdPair;

        //}
        public virtual void Release(){}
        /// <summary>
        /// 空虚函数;
        /// </summary>
        public virtual void OnRefresh() { }
    }
}


