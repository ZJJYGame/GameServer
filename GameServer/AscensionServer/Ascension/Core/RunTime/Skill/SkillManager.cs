using AscensionProtocol;
using Cosmos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AscensionServer 
{
    /// <summary>
    /// 技能模块；
    /// 注：此模块用于验证客户端角色释放技能时数值的验证、转发等；
    /// </summary>
    [Module]
    public class SkillManager:Cosmos. Module,ISkillManager
    {
        protected override void OnInitialization()
        {
            //CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncRoleAdventureSkill, OnPlayerSkillC2S);
        }
        /// <summary>
        /// 接收来自客户端的技能输入
        /// </summary>
        async void OnPlayerSkillC2S(int sessionId, OperationData opData)
        {
            await Task.Run(() =>
            {
                //var skill = opData.BinParameters as C2SSkillInput;
                //if (skill != null)
                //{
                    //GameManager.CustomeModule<LevelManager>().SendMsg2AllLevelRoleS2C(skill.EntityContainer.EntityContainerId, opData);
                //}
            });
        }
    }
}


