using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 战斗角色技能控制器,每个角色实体持有，用来管理角色技能
    /// </summary>
    public class BattleSkillController
    {
        //控制器拥有者
        BattleCharacterEntity owner;
        //角色技能对象缓存,key=>技能id,value=>技能对象
        Dictionary<int, BattleSkillBase> skillDict;
        //角色拥有的技能id集合
        HashSet<int> roleHasSkillHash;
        //用于记录当前使用的技能id
        public int nowUseSkillId;

        public bool HasSkill(int skillID)
        {
            return roleHasSkillHash.Contains(skillID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="targetIdList"></param>
        /// <param name="isCDLimit">是否被CD限制，直接使用需考虑cd，触发无需考虑</param>
        public void UseSkill(int skillID,List<int> targetIdList,bool isCDLimit)
        {
            nowUseSkillId = skillID;

            if (!skillDict.ContainsKey(skillID))
                skillDict[skillID] = new BattleSkillBase(skillID, owner);
            BattleSkillBase battleSkill = skillDict[skillID];
            //冷却是否满足以及冷却的处理
            if (isCDLimit)//如果有CD限制
            {
                if (battleSkill.NowCold != 0)
                {
                    Utility.Debug.LogError($"技能{battleSkill.SkillID}正在冷却，无法使用,剩余冷却{battleSkill.MaxCold-battleSkill.NowCold}");
                    return;
                }
                else
                {
                    battleSkill.EnterCold();
                }
            }
            //todo释放条件是否满足
            //todo消耗的判断与扣除
            if (!battleSkill.SkillCost(out var actionCost))
            {
                Utility.Debug.LogError($"技能{battleSkill.SkillID}消耗不足，无法释放");
                return;
            }
            //获取技能释放目标的实体
            List<BattleCharacterEntity> targetCharacterList = new List<BattleCharacterEntity>();
            for (int i = 0; i < targetIdList.Count; i++)
            {
                targetCharacterList.Add(GameEntry.BattleCharacterManager.GetCharacterEntity(targetIdList[i]));
            }
            //生成传输对象，防止没有攻击对象时没有生成传输对象
            GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).SpawnBattleTransfer(owner.UniqueID,battleSkill.SkillID,BattleCmd.SkillInstruction);

            //使用技能前buff事件处理
            ISkillAdditionData buffBeforeUseSkillAddition = owner.BattleBuffController.TriggerBuffEventBerforeUseSkill(null);

            #region 所有目标的伤害一次性打完
            if (battleSkill.AttackProcess_Type == AttackProcess_Type.SingleUse)//所有目标的伤害一次性打完
            {
                for (int i = 0; i < battleSkill.AttackSectionNumber; i++)
                {
                   //释放上一次行为信息
                    GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).ReleaseBattleTransfer();
                    BattleTransferDTO battleTransferDTO = GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).SpawnBattleTransfer(owner.UniqueID,battleSkill.SkillID,BattleCmd.SkillInstruction);
                    //判断所有目标是可以成为技能目标
                    bool flag = false;
                    for (int j = 0; j < targetCharacterList.Count; j++)
                    {
                        flag = battleSkill.CanUseSkill(targetCharacterList[j]) || flag;
                    }
                    if (!flag)
                        continue;

                    if (actionCost != null)
                    {
                        battleTransferDTO.ActionCost = actionCost;
                        actionCost = null;
                    }

                    List<BattleDamageData> battleDamageDataList = new List<BattleDamageData>();
                    //生成该次行动战斗数据记录对象
                    
                    //每段攻击前buff事件处理
                    ISkillAdditionData buffBeforeAtkAddition= owner.BattleBuffController.TriggerBuffEventBeforeAttack();
                    for (int j = 0; j < targetCharacterList.Count; j++)
                    {
                        if (!battleSkill.CanUseSkill(targetCharacterList[j]))
                        {
                            owner.TakeBattleDamageData = null;
                            targetCharacterList[j].ReceiveBattleDamageData = null;
                            continue;
                        }

                        //暴击判断
                        BattleDamageData battleDamageData = battleSkill.IsCrit(i, targetCharacterList[j]);
                        owner.TakeBattleDamageData = battleDamageData;
                        targetCharacterList[j].ReceiveBattleDamageData = battleDamageData;
                        //攻击前技能事件触发
                        ISkillAdditionData skillBeforeAtkAddition = battleSkill.TriggerSkillEventBeforeAttack( battleDamageData);
                        battleDamageData.AddActorSkillAddition(buffBeforeUseSkillAddition, buffBeforeAtkAddition, skillBeforeAtkAddition);
                        //获得伤害数据
                        battleDamageData = battleSkill.GetDamageData(i, j, targetCharacterList[j], battleDamageData);
                        battleDamageDataList.Add(battleDamageData);
                    }

                    //受击前buff事件处理
                    for (int j = 0; j < targetCharacterList.Count; j++)
                    {
                        ISkillAdditionData buffBeforOnHitAddition= targetCharacterList[j].BattleBuffController.TriggerBuffEventBeforeOnHit(owner, battleDamageDataList[j]);
                        if (targetCharacterList[j].ReceiveBattleDamageData != null)
                        {
                            targetCharacterList[j].ReceiveBattleDamageData.AddtargetSkillAddition(buffBeforOnHitAddition);
                            battleSkill.GetDamageData(i, j, targetCharacterList[j], targetCharacterList[j].ReceiveBattleDamageData);
                        }
                    }

                    //todo伤害结算
                    for (int j = 0; j < battleDamageDataList.Count; j++)
                    {
                        GameEntry.BattleCharacterManager.GetCharacterEntity(battleDamageDataList[j].TargetID).OnActionEffect(battleDamageDataList[j]);
                    }


                    List<TargetInfoDTO> targetInfoDTOs= GetTargetInfoDTOList(battleDamageDataList);
                    if (battleTransferDTO.TargetInfos != null)
                        battleTransferDTO.TargetInfos.AddRange(targetInfoDTOs);
                    else
                        battleTransferDTO.TargetInfos = targetInfoDTOs;

                    //受击后buff事件处理
                    for (int j = 0; j < targetCharacterList.Count; j++)
                    {
                        targetCharacterList[j].BattleBuffController.TriggerBuffEventBehindOnHit(owner, battleDamageDataList[j]);
                    }

                    //每段攻击后buff事件结算
                    owner.BattleBuffController.TriggerBuffEventBehindAttack(targetCharacterList[0], battleDamageDataList[0]);

                    for (int j = 0; j < battleDamageDataList.Count; j++)
                    {
                        //攻击后技能事件结算
                        battleSkill.TriggerSkillEventBehindAttack(battleDamageDataList[j]);
                    }


                }
                //添加buff
                battleSkill.AddAllBuff(targetCharacterList);


                //使用技能后buff事件处理     
                if (targetCharacterList.Count > 0)
                    owner.BattleBuffController.TriggerBuffEventBehindUseSkill(targetCharacterList[0]);
                else
                    owner.BattleBuffController.TriggerBuffEventBehindUseSkill(null);

                //释放正在使用的战斗数据记录对象
                GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).ReleaseBattleTransfer();
                return;
            }
            #endregion
            #region 所有目标的伤害分阶段打完
            else//所有目标的伤害分阶段打完
            {
                for (int i = 0; i < targetCharacterList.Count; i++)
                {
                    List<BattleDamageData> battleDamageDataList = new List<BattleDamageData>();
                   
                    for (int j = 0; j < battleSkill.AttackSectionNumber; j++)
                    {
                        if (!battleSkill.CanUseSkill(targetCharacterList[i]))
                            continue;
            
                        GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).ReleaseBattleTransfer();
                        BattleTransferDTO battleTransferDTO = GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).SpawnBattleTransfer(owner.UniqueID, battleSkill.SkillID, BattleCmd.SkillInstruction);
                        //每段攻击前buff事件处理
                        ISkillAdditionData buffBeforeAtkAddition= owner.BattleBuffController.TriggerBuffEventBeforeAttack(targetCharacterList[i]);

                        BattleDamageData battleDamageData = battleSkill.IsCrit(j, targetCharacterList[i]);
                        owner.TakeBattleDamageData = battleDamageData;
                        targetCharacterList[i].ReceiveBattleDamageData = battleDamageData;
                        ISkillAdditionData skillBeforeAtkAddition= battleSkill.TriggerSkillEventBeforeAttack(battleDamageData);
                        battleDamageData.AddActorSkillAddition(buffBeforeUseSkillAddition, buffBeforeAtkAddition, skillBeforeAtkAddition);
                        battleDamageData = battleSkill.GetDamageData(j, i, targetCharacterList[i], battleDamageData);
                        //if (battleDamageData == null)//根本没有打，移除传输事件
                        //{
                        //    battleTransferDTOList.Remove(battleTransferDTO);
                        //    continue;
                        //}
                        //受击前buff事件处理
                        ISkillAdditionData buffBeforOnHitAddition= targetCharacterList[i].BattleBuffController.TriggerBuffEventBeforeOnHit(owner, battleDamageData);
                        if (targetCharacterList[j].ReceiveBattleDamageData != null)
                        {
                            targetCharacterList[j].ReceiveBattleDamageData.AddtargetSkillAddition(buffBeforOnHitAddition);
                        }


                        battleDamageDataList.Add(battleDamageData);
                        GameEntry.BattleCharacterManager.GetCharacterEntity(battleDamageData.TargetID).OnActionEffect(battleDamageData);


                        List<TargetInfoDTO> targetInfoDTOs= GetTargetInfoDTOList(new List<BattleDamageData>() { battleDamageData });
                        if (battleTransferDTO.TargetInfos != null)
                            battleTransferDTO.TargetInfos.AddRange(targetInfoDTOs);
                        else
                            battleTransferDTO.TargetInfos = targetInfoDTOs;



                        //受击后buff事件处理
                        targetCharacterList[i].BattleBuffController.TriggerBuffEventBehindOnHit(owner, battleDamageData);

                        //每段攻击后buff事件结算
                        owner.BattleBuffController.TriggerBuffEventBehindAttack(targetCharacterList[i], battleDamageData);

                        battleSkill.TriggerSkillEventBehindAttack(battleDamageData);

                    }
                    //添加buff
                    battleSkill.AddBuff(i,targetCharacterList[i]);

                }

               

                //使用技能后buff事件处理
                owner.BattleBuffController.TriggerBuffEventBehindUseSkill();

                //释放正在使用的战斗数据记录对象
                GameEntry.BattleRoomManager.GetBattleRoomEntity(owner.RoomID).ReleaseBattleTransfer();

                return;
            }
            #endregion
        }

        //将伤害数据转换为发送数据
        List<TargetInfoDTO> GetTargetInfoDTOList(List<BattleDamageData> battleDamageDataList)
        {
            List<TargetInfoDTO> targetInfoDTOList = new List<TargetInfoDTO>();
            TargetInfoDTO targetInfoDTO;
            for (int i = 0; i < battleDamageDataList.Count; i++)
            {
                if (battleDamageDataList[i] == null)
                    continue;
                targetInfoDTO = new TargetInfoDTO();
                targetInfoDTO.TargetID = battleDamageDataList[i].TargetID;
                switch (battleDamageDataList[i].baseDamageTargetProperty)
                {
                    case BattleSkillDamageTargetProperty.Health:
                        targetInfoDTO.TargetHPDamage = battleDamageDataList[i].damageNum;
                        break;
                    case BattleSkillDamageTargetProperty.ZhenYuan:
                        targetInfoDTO.TargetMPDamage = battleDamageDataList[i].damageNum;
                        break;
                    case BattleSkillDamageTargetProperty.ShenHun:
                        targetInfoDTO.TargetShenHunDamage = battleDamageDataList[i].damageNum;
                        break;
                }
                targetInfoDTOList.Add(targetInfoDTO);
            }
            if (targetInfoDTOList.Count != 0)
                return targetInfoDTOList;
            else return null;
        }

        public BattleSkillController(BattleCharacterEntity owner)
        {
            this.owner = owner;
            skillDict = new Dictionary<int, BattleSkillBase>();
            roleHasSkillHash = new HashSet<int>() { 21001};
        }
    }
}
