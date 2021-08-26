using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    public enum AllianceOpCode
    {
        /// <summary>
        /// 创建宗门
        /// </summary>
        CreatAlliance=1,
        /// <summary>
        /// 加入宗门
        /// </summary>
        JoinAlliance=2,
        /// <summary>
        /// 获取所有宗门
        /// </summary>
        GetAlliances=3,
        /// <summary>
        /// 获得自身宗门数据
        /// </summary>
        GetAllianceStatus=4,
        /// <summary>
        /// 宗门建设
        /// </summary>
        BuildAlliance=5,
        /// <summary>
        /// 获得宗门成员
        /// </summary>
        GetAllianceMember=6,
        /// <summary>
        /// 退出宗门
        /// </summary>
        QuitAlliance=7,
        /// <summary>
        /// 宗门签到
        /// </summary>
        UpdateAllianceSignin = 8,
        /// <summary>
        /// 拒绝申请
        /// </summary>
        RefuseApply=9,
        /// <summary>
        /// 同意申请
        /// </summary>
        ConsentApply=10,
        /// <summary>
        /// 升级宗门技能
        /// </summary>
        UpdateAllianceSkill=11,
        /// <summary>
        /// 修改宗门名称
        /// </summary>
        ChangeAllianceName=12,
        /// <summary>
        /// 宗门活动
        /// </summary>
        AllianceActivity=13,
        /// <summary>
        /// 修改宗门宗旨
        /// </summary>
        ChangeAlliancePurpose = 14,
        /// <summary>
        /// 职位晋升
        /// </summary>
        CareerAdvancement=15,
        /// <summary>
        /// 搜索宗门
        /// </summary>
        SearchAlliance=16,
        /// <summary>
        /// 兑换丹药
        /// </summary>
        ExchangeElixir=19,
        /// <summary>
        /// 兑换功法
        /// </summary>
        ExchangeScripturesPlatform = 20,
        /// <summary>
        /// 设置兑换物品
        /// </summary>
        SetExchangeGoods = 22,
        /// <summary>
        /// 获取仙盟通告数据
        /// </summary>
        GetAlliancecallboard = 23,
        /// <summary>
        /// 抢占洞府
        /// </summary>
        PreemptDongFu=24,
        /// <summary>
        /// 获得洞府信息
        /// </summary>
        GetDongFuStatus=25,
        /// <summary>
        /// 获得任务宗门信息
        /// </summary>
        GetRoleAlliance=26,
        /// <summary>
        /// 获得宗门兑换表
        /// </summary>
        GetExchangeGoods=27,
        /// <summary>
        /// 获得宗门技能数据
        /// </summary>
        GetAllianceSkill = 28,
        /// <summary>
        ///通知占领挑战
        /// </summary>
        NoticetPreemptChallenge=29,
        /// <summary>
        /// 加入成功后的被动接受
        /// </summary>
        JoinAllianceSuccess = 30,
        /// <summary>
        /// 宗门签到
        /// </summary>
        GetAllianceSignin = 31,
        /// <summary>
        /// 踢出宗门
        /// </summary>
        KickOutAlliance= 32,
        /// <summary>
        /// 解散宗门
        /// </summary>
        DissolveAlliance=33,
        /// <summary>
        /// 取消解散
        /// </summary>
        CancelDissolve = 34,

    }
}
