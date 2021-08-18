using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;

namespace AscensionServer
{
    [Module]
    public partial class SecondaryJobManager : Cosmos. Module,ISecondaryJobManager
    {
        /// <summary>
        /// 配方类型
        /// </summary>
        enum FormulaDrugType: byte
        {
            Alchemy = 1,
            Forge=2,
            Spiritualrunes=3,
            Puppet=4,
            Tacticformation=5
        }
        /// <summary>
        /// 词缀类型
        /// </summary>
        enum AffixType
        {
            None=0,
            HP=1,
            AttackPhysical=2,
            DefendPhysical=3,
            AttackPower=4,
            DefendPower=5,
            AttackSpeed=6,
        }
        enum WeaponType
        {
            /// <summary>
            /// 拳套
            /// </summary>
            BoxingGloves=1,
            /// <summary>
            /// 鏡子
            /// </summary>
            Mirror=2,
            /// <summary>
            /// 書卷
            /// </summary>
            Book=3,
            /// <summary>
            /// 劍
            /// </summary>
            Sword=4,
            /// <summary>
            /// zhang
            /// </summary>
            Cane=5,
            /// <summary>
            /// fuchen
            /// </summary>
            FuChen=6,
            /// <summary>
            /// 旗
            /// </summary>
            Flag=7,
            /// <summary>
            /// 刀
            /// </summary>
            BroadSword=8,
        }
        enum FrogeType
        {
            weapon=1,
            Coat=2,
            UnderWear=3,
            Shoes=4,
            MagicWeapon=9,

        }
        DrollRandom drollRandom = new DrollRandom();
        protected override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((byte)OperationCode.SyncSecondaryJob, ProcessHandlerC2S);
        }

        void ProcessHandlerC2S(int seeionid, OperationData packet)
        {
            SecondaryJobDTO secondaryJob;
            //Utility.Debug.LogInfo("YZQ收到的副职业请求" + packet.DataMessage.ToString());
            //Utility.Debug.LogInfo("YZQ收到的副职业请求"+ (SecondaryJobOpCode)packet.SubOperationCode);
            switch ((SecondaryJobOpCode)packet.SubOperationCode)
            {
                case SecondaryJobOpCode.GetSecondaryJobStatus:
                    #region
                    var role = Utility.Json.ToObject<RoleDTO>(packet.DataMessage.ToString());
                    Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求");
                    GetSecondaryJobStatusS2C(role.RoleID);
                    #endregion
                    break;
                case SecondaryJobOpCode.StudySecondaryJobStatus:
                    #region
                    secondaryJob = Utility.Json.ToObject<SecondaryJobDTO>(packet.DataMessage.ToString());
                    StudyFormula(secondaryJob);
                    #endregion
                    break;
                case SecondaryJobOpCode.CompoundAlchemy:
                    #region
                    secondaryJob = Utility.Json.ToObject<SecondaryJobDTO>(packet.DataMessage.ToString());
                    CompoundAlchemyS2C(secondaryJob.RoleID, secondaryJob.UseItemID);
                    #endregion
                    break;
                case SecondaryJobOpCode.CompoundPuppet:
                    #region
                    secondaryJob = Utility.Json.ToObject<SecondaryJobDTO>(packet.DataMessage.ToString());
                    CompoundPuppetS2C(secondaryJob.RoleID, secondaryJob.UseItemID);
                    #endregion
                    break;
                case SecondaryJobOpCode.CompoundForge:
                    #region
                    secondaryJob = Utility.Json.ToObject<SecondaryJobDTO>(packet.DataMessage.ToString());
                    CompoundForge(secondaryJob.RoleID, secondaryJob.UseItemID);
                    #endregion
                    break;
                case SecondaryJobOpCode.AssemblePuppet:
                    #region
                    secondaryJob = Utility.Json.ToObject<SecondaryJobDTO>(packet.DataMessage.ToString());
                    AssemblePuppetS2C(secondaryJob.RoleID, secondaryJob.UseItemID, secondaryJob.Units);
                    #endregion
                    break;
                case SecondaryJobOpCode.RepairPuppet:
                    #region
                    secondaryJob = Utility.Json.ToObject<SecondaryJobDTO>(packet.DataMessage.ToString());
                    RepairPuppetS2C(secondaryJob.RoleID, secondaryJob.UseItemID);
                    #endregion
                    break;
                case SecondaryJobOpCode.GetPuppetUnit:
                    #region
                    role = Utility.Json.ToObject<RoleDTO>(packet.DataMessage.ToString());
                    GetPuppetUnitS2C(role.RoleID);
                    #endregion
                    break;
                case SecondaryJobOpCode.AddDemonicSoul:
                    #region
                    var demonicDoul = Utility.Json.ToObject<DemonicSoulDTO>(packet.DataMessage.ToString());
                    AddDemonical(demonicDoul.RoleID, demonicDoul.CompoundList[0]);
                    #endregion
                    break;
                case SecondaryJobOpCode.CompoundDemonicSoul:
                    #region
                    demonicDoul = Utility.Json.ToObject<DemonicSoulDTO>(packet.DataMessage.ToString());
                    CompoundDemonical(demonicDoul.CompoundList, demonicDoul.RoleID);
                    #endregion
                    break;
                case SecondaryJobOpCode.GetDemonicSoul:
                    #region
                    demonicDoul = Utility.Json.ToObject<DemonicSoulDTO>(packet.DataMessage.ToString());
                    GetDemonicSoul(demonicDoul.RoleID);
                    #endregion
                    break;
                default:
                    break;
            }
        }

        void StudyFormula(SecondaryJobDTO secondaryJob)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, FormulaGlobaID>>(out var globaIDDict);
            var tempid = Utility.Converter.RetainInt32(secondaryJob.UseItemID, 5);
            var result = globaIDDict.TryGetValue(tempid,out var drugData);
            if (!result)
            {
                RoleStatusFailS2C(secondaryJob.RoleID,SecondaryJobOpCode.StudySecondaryJobStatus);
                return;
            }

            switch ((FormulaDrugType )drugData.ItemTypeDetail)
            {
                case FormulaDrugType.Alchemy:
                    UpdateAlchemyS2C(secondaryJob.RoleID, secondaryJob.UseItemID);
                    break;
                case FormulaDrugType.Forge:
                    Utility.Debug.LogInfo("YZQ收到的副职业添加配方请求>>>"+Utility.Json.ToJson(secondaryJob));
                    UpdateForgeS2C(secondaryJob.RoleID, secondaryJob.UseItemID);
                    break;
                case FormulaDrugType.Puppet:
                    UpdatePuppetS2C(secondaryJob.RoleID, secondaryJob.UseItemID);
                    break;
                default:
                    break;
            }
        }

        void GetSecondaryJobStatusS2C(int roleID)
        {
            NHCriteria nHCriteriaRole =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", roleID);
            Dictionary<byte, object> dict = new Dictionary<byte, object>();
            #region
            var alchemyExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._AlchemyPerfix, roleID.ToString()).Result;
            if (alchemyExist)
            {
                var alchemyObj = RedisHelper.Hash.HashGetAsync<AlchemyDTO>(RedisKeyDefine._AlchemyPerfix, roleID.ToString()).Result;
                if (alchemyObj == null)
                {
                    var alchemy = NHibernateQuerier.CriteriaSelect<Alchemy>(nHCriteriaRole);
                    if (alchemy != null)
                    {
                        alchemyObj = ChangeDataType(alchemy);
                        dict.Add((byte)ParameterCode.JobAlchemy, alchemyObj);
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, SecondaryJobOpCode.GetSecondaryJobStatus);
                        return;
                    }
                }
                else
                    dict.Add((byte)ParameterCode.JobAlchemy, alchemyObj);
            }
            else
            {
                var alchemy = NHibernateQuerier.CriteriaSelect<Alchemy>(nHCriteriaRole);
                if (alchemy != null)
                {
                    Utility.Debug.LogInfo("1YZQ收到的副职业请求");
                    dict.Add((byte)ParameterCode.JobAlchemy, ChangeDataType(alchemy));
                }
                else
                {
                    RoleStatusFailS2C(roleID, SecondaryJobOpCode.GetSecondaryJobStatus);
                    return;
                }
            }
            #endregion
            #region
            var forgeExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._ForgePerfix, roleID.ToString()).Result;
            if (forgeExist)
            {
                var forgeObj = RedisHelper.Hash.HashGetAsync<ForgeDTO>(RedisKeyDefine._ForgePerfix, roleID.ToString()).Result;
                if (forgeObj == null)
                {
                    var forge = NHibernateQuerier.CriteriaSelect<Forge>(nHCriteriaRole);
                    if (forge != null)
                    {
                        forgeObj = ChangeDataType(forge);
                        dict.Add((byte)ParameterCode.JobForge, forgeObj);
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, SecondaryJobOpCode.GetSecondaryJobStatus);
                        return;
                    }
                }
                else
                    dict.Add((byte)ParameterCode.JobForge, forgeObj);
            }
            else
            {
                var forge = NHibernateQuerier.CriteriaSelect<Forge>(nHCriteriaRole);
                if (forge != null)
                {
                    Utility.Debug.LogInfo("2YZQ收到的副职业请求");
                    dict.Add((byte)ParameterCode.JobForge, ChangeDataType(forge));
                }
                else
                {
                    RoleStatusFailS2C(roleID, SecondaryJobOpCode.GetSecondaryJobStatus);
                    return;
                }
            }
            #endregion
            #region
            var puppetExist = RedisHelper.Hash.HashExistAsync(RedisKeyDefine._PuppetPerfix, roleID.ToString()).Result;
            if (puppetExist)
            {
                var puppetObj = RedisHelper.Hash.HashGetAsync<PuppetDTO>(RedisKeyDefine._PuppetPerfix, roleID.ToString()).Result;
                if (puppetObj == null)
                {
                    var puppet = NHibernateQuerier.CriteriaSelect<Puppet>(nHCriteriaRole);
                    if (puppet != null)
                    {
                        puppetObj = ChangeDataType(puppet);
                        dict.Add((byte)ParameterCode.JobPuppet, puppetObj);
                    }
                    else
                    {
                        RoleStatusFailS2C(roleID, SecondaryJobOpCode.GetSecondaryJobStatus);
                        return;
                    }
                }
                else
                    dict.Add((byte)ParameterCode.JobPuppet, puppetObj);
            }
            else
            {
                var puppet = NHibernateQuerier.CriteriaSelect<Puppet>(nHCriteriaRole);
                if (puppet != null)
                {
                    Utility.Debug.LogInfo("3YZQ收到的副职业请求");
                    dict.Add((byte)ParameterCode.JobPuppet, ChangeDataType(puppet));
                }
                else
                {
                    RoleStatusFailS2C(roleID, SecondaryJobOpCode.GetSecondaryJobStatus);
                    return;
                }
            }
            #endregion

            RoleStatusSuccessS2C(roleID,SecondaryJobOpCode.GetSecondaryJobStatus,dict);
        }

        void RoleStatusSuccessS2C(int roleID, SecondaryJobOpCode oPcode, object data)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncSecondaryJob;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.Success;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);

            Utility.Debug.LogInfo("角色副职业数据发送了" + Utility.Json.ToJson(data));
        }

        void RoleStatusFailS2C(int roleID, SecondaryJobOpCode oPcode,string tips=null)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncSecondaryJob;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.ItemNotFound;
            opData.DataMessage = Utility.Json.ToJson(tips);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
        /// <summary>
        /// 合成失敗
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="oPcode"></param>
        void RoleStatusCompoundFailS2C(int roleID, SecondaryJobOpCode oPcode, object data)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)OperationCode.SyncSecondaryJob;
            opData.SubOperationCode = (byte)oPcode;
            opData.ReturnCode = (byte)ReturnCode.Fail;
            opData.DataMessage = Utility.Json.ToJson(data);
            GameEntry.RoleManager.SendMessage(roleID, opData);
        }
    }
}


