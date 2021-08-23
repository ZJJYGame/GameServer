using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    public enum PracticeOpcode
    {
        None = 0,
        GetRoleGongfa = 1,//获得角色功法
        AddGongFa = 2,//添加功法
        GetRoleMiShu = 3,//获得角色秘术
        AddMiShu = 4,//添加秘术
        SwitchPracticeType=5,//切换修炼秘书功法
        UploadingExp = 7,//上传经验
        GetOffLineExp = 8,//获得离线经验
        TriggerBottleneck = 9,//触发瓶颈
        UseBottleneckElixir = 10,//使用丹药突破瓶颈
        UpdateBottleneck = 11,//更新瓶颈状态
        DemonicBattle= 12,//触发心魔战斗
        ThunderRoundBattle = 13//触发天劫战斗
    }
}
