using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleSkillObj: ISkillAdditionData
    {
        //角色的属性
        CharacterBattleData CharacterBattleData { get { return OwnerEntity.CharacterBattleData; } }
        public BattleCharacterEntity OwnerEntity { get; protected set; }


        public int SkillID { get { return battleSkillData.id; } }

        //当前冷却
        public int NowCold { get; private set; }
        //最大冷却
        public int MaxCold { get { return battleSkillData.cd; } }

        public int DamgeAddition { get { return battleSkillData.damageAddition; } set { } }
        public int CritProp { get { return battleSkillData.critProp; } set { } }
        public int CritDamage { get { return battleSkillData.critDamage; } set { } }
        public int IgnoreDefensive { get { return battleSkillData.ignoreDefensive; } set { } }
        public int DamagDeduction { get; set; }
        public int DodgeProp { get; set; }

        //技能攻击段数
        public int AttackSectionNumber { get { return battleSkillData.battleSkillDamageNumDataList.Count; } }
        public AttackProcess_Type AttackProcess_Type { get { return battleSkillData.attackProcessType; } }
        //最后一个攻击值
        public int LastAttackValue { get; protected set; }
        //最后一个伤害值
        public int LastDamageValue { get; protected set; }
       

        //技能对应的json数据
        BattleSkillData battleSkillData;

        List<BattleSkillEventBase> battleSkillEventBaseList;
        //攻击前技能触发事件
        Action<BattleDamageData,ISkillAdditionData> skillEventBeforeAttack;
        public event Action<BattleDamageData,ISkillAdditionData> SkillEventBeforeAttack
        {
            add { skillEventBeforeAttack += value; }
            remove { skillEventBeforeAttack -= value; }
        }
        //攻击后技能触发事件
        Action<BattleDamageData, ISkillAdditionData> skillEventBehindAttack;
        public event Action<BattleDamageData, ISkillAdditionData> SkillEventBehindAttack
        {
            add { skillEventBehindAttack += value; }
            remove { skillEventBehindAttack -= value; }
        }
        //进入冷却
        public void EnterCold()
        {
            NowCold = MaxCold;
        }
        //减少冷却
        public void ReduceCold()
        {
            if (NowCold > 0)
                NowCold--;
        }
        public bool CanUseSkill(BattleCharacterEntity target)
        {
            if (target.HasDie)
            {
                Utility.Debug.LogError("目标已死亡");
                if (battleSkillData.battleSkillActionType == BattleSkillActionType.Damage || battleSkillData.battleSkillActionType == BattleSkillActionType.Heal)
                {
                    return false;
                }
                else return true;
            }
            else
            {
                if (battleSkillData.battleSkillActionType == BattleSkillActionType.Damage || battleSkillData.battleSkillActionType == BattleSkillActionType.Heal)
                {
                    return true;
                }
                else return false;
            }
        }
        public bool SkillCost(out ActionCost actionCost)
        {
            BattleSkillCostData battleSkillCostData = battleSkillData.battleSkillCostData;
            int nowData = 0; ;
            int needData = 0;
            switch (battleSkillCostData.battleSkillCostType)
            {
                case BattleSkillCostType.Health:
                    nowData = CharacterBattleData.Hp;
                    needData = CharacterBattleData.MaxHp * battleSkillCostData.percentValue/100 + battleSkillCostData.fixedValue;
                    break;
                case BattleSkillCostType.ZhenYuan:
                    nowData = CharacterBattleData.Mp;
                    needData = CharacterBattleData.MaxMp * battleSkillCostData.percentValue/100 + battleSkillCostData.fixedValue;
                    break;
                case BattleSkillCostType.ShenHun:
                    nowData = CharacterBattleData.Soul;
                    needData = CharacterBattleData.MaxSoul * battleSkillCostData.percentValue/100 + battleSkillCostData.fixedValue;
                    break;
                case BattleSkillCostType.JingXue:
                    nowData = CharacterBattleData.BestBlood;
                    needData = CharacterBattleData.BestBloodMax * battleSkillCostData.percentValue/100 + battleSkillCostData.fixedValue;
                    break;
            }
            if (nowData >= needData)
            {
                CharacterBattleData.ChangeProperty(battleSkillCostData.battleSkillCostType, -needData);
                actionCost = new ActionCost()
                {
                    BattleSkillCostType = (byte)battleSkillCostData.battleSkillCostType,
                    CostNum = -needData,
                    Cold = NowCold
                };
                return true;
            }
            else
            {
                actionCost = null;
                return false;
            }
        }
        /// <summary>
        /// 获取该技能的伤害
        /// </summary>
        /// <param name="index">第几段伤害</param>
        /// <param name="targetIndex">第几个目标</param>
        public BattleDamageData GetDamageData(int index, int targetIndex, BattleCharacterEntity target, BattleDamageData battleDamageData)
        {
            if (target.HasDie)//todo需判断是否是复活技能
                return null;
            BattleSkillDamageNumData battleSkillDamageNumData = battleSkillData.battleSkillDamageNumDataList[index];
            battleDamageData.battleSkillActionType = battleSkillData.battleSkillActionType;
            battleDamageData.damageType = battleSkillDamageNumData.battleSkillDamageType;
            battleDamageData.baseDamageTargetProperty = battleSkillDamageNumData.baseNumSourceDataList[targetIndex].battleSkillDamageTargetProperty;
            battleDamageData.selfCharacterBattleData = CharacterBattleData;
            battleDamageData.targetCharacterBattleData = target.CharacterBattleData;
            //计算基础伤害初始值
            int attackValue = 0;
            if (battleSkillDamageNumData.baseDamageAdditionSourceTarget)//根据目标数值
                attackValue = (int)target.CharacterBattleData.GetProperty(battleSkillDamageNumData.baseNumSourceDataList[targetIndex].battleSkillNumSourceType, battleDamageData) * battleSkillDamageNumData.baseNumSourceDataList[targetIndex].mulitity/100 + battleSkillDamageNumData.fixedNum;
            else//根据自身数值
                attackValue = (int)CharacterBattleData.GetProperty(battleSkillDamageNumData.baseNumSourceDataList[targetIndex].battleSkillNumSourceType, battleDamageData) * battleSkillDamageNumData.baseNumSourceDataList[targetIndex].mulitity/100 + battleSkillDamageNumData.fixedNum;
            battleDamageData.initialAttackValue = attackValue;
            battleDamageData.initialDefendValue= target.CharacterBattleData.GetProperty(battleDamageData.damageType);

            battleDamageData.GetDamage();

            //计算额外伤害初始值
            int extraAttackValue = 0;
            if (battleSkillDamageNumData.extraNumSourceData.Count > 0)
            {
                if (battleSkillDamageNumData.extraDamageAdditionSourceTarget)//根据目标数值
                    extraAttackValue = (int)target.CharacterBattleData.GetProperty(battleSkillDamageNumData.extraNumSourceData[targetIndex].battleSkillNumSourceType, battleDamageData) * battleSkillDamageNumData.extraNumSourceData[targetIndex].mulitity;
                else//根据自身数值
                    extraAttackValue = (int)CharacterBattleData.GetProperty(battleSkillDamageNumData.extraNumSourceData[targetIndex].battleSkillNumSourceType, battleDamageData) * battleSkillDamageNumData.extraNumSourceData[targetIndex].mulitity;
                battleDamageData.extraDamageNum = battleSkillData.battleSkillActionType == BattleSkillActionType.Damage ? -extraAttackValue : extraAttackValue; ;
            }

            //todo 最后攻击值和最后伤害值设置位置有待修改
            LastAttackValue = attackValue;
            LastDamageValue = Math.Abs(battleDamageData.damageNum);
            return battleDamageData;
        }


        public BattleDamageData IsCrit(int index, BattleCharacterEntity target,params ISkillAdditionData[] skillAdditionDatas)
        {
            BattleDamageData battleDamageData = new BattleDamageData();
            BattleSkillDamageNumData battleSkillDamageNumData = battleSkillData.battleSkillDamageNumDataList[index];
            bool isCrit = false;
            if (battleSkillDamageNumData.battleSkillDamageType == BattleSkillDamageType.Physic || battleSkillDamageNumData.battleSkillDamageType == BattleSkillDamageType.Magic)
            {
                float critRange = 0;
                if (battleSkillDamageNumData.battleSkillDamageType == BattleSkillDamageType.Physic)
                    critRange = (CharacterBattleData.PhysicalCritProb - target.CharacterBattleData.ReduceCritProb) * 100;
                else if (battleSkillDamageNumData.battleSkillDamageType == BattleSkillDamageType.Magic)
                    critRange = (CharacterBattleData.MagicCritProb - target.CharacterBattleData.ReduceCritProb) * 100;
                int additionCritProp = skillAdditionDatas.Sum(p => p.CritProp);
                critRange += (CritProp + additionCritProp) *100;
                int randomValue = Utility.Algorithm.RandomRange(0, 10000 + 1);
                if (randomValue <= critRange)
                    isCrit = true;
            }
            battleDamageData.TargetID = target.UniqueID;
            battleDamageData.isCrit = isCrit;
            battleDamageData.attackSection = index;
            return battleDamageData;
        }
        
        public void AddBuff(int targetIndex,BattleCharacterEntity battleCharacterEntity,BattleTransferDTO battleTransferDTO=null)
        {
            if (battleTransferDTO == null)
                battleTransferDTO = GameEntry.BattleRoomManager.GetBattleRoomEntity(OwnerEntity.RoomID).GetBattleTransfer();
            float baseProp;
            float selfProp;
            float targetProp;
            if (battleTransferDTO.AddBuffDTOList == null)
                battleTransferDTO.AddBuffDTOList = new List<AddBuffDTO>();
            for (int i = 0; i < battleSkillData.battleSkillAddBuffList.Count; i++)
            {
                BattleSkillAddBuffData battleSkillAddBuffData = battleSkillData.battleSkillAddBuffList[i];
                if (battleSkillAddBuffData.TargetType)//受击方
                {
                        if (CanAddBuff(targetIndex, battleSkillAddBuffData, CharacterBattleData, battleCharacterEntity.CharacterBattleData))
                        {
                            BattleBuffObj addBuffObj = battleCharacterEntity.BattleBuffController.AddBuff(battleSkillAddBuffData, this);
                            //客户端无需知道buff是否被覆盖，只需要同buff替换即可
                            if (addBuffObj != null)
                                battleTransferDTO.AddBuffDTOList.Add(new AddBuffDTO()
                                {
                                    TargetId = addBuffObj.Owner.UniqueID,
                                    BuffId = addBuffObj.BuffId,
                                    Round = addBuffObj.NowRound
                                });
                        }
                }
                else//给自己加
                {
                    baseProp = battleSkillAddBuffData.basePropList[0];
                    selfProp = CharacterBattleData.GetProperty(battleSkillAddBuffData.selfAddBuffProbability.battleSkillBuffProbSource, null);
                    selfProp = selfProp * battleSkillAddBuffData.selfAddBuffProbability.multiplyPropValue / 10000 + battleSkillAddBuffData.selfAddBuffProbability.fixedPropValue;
                    selfProp = battleSkillAddBuffData.selfAddBuffProbability.addOrReduce ? selfProp : -selfProp;
                    if (CanAddBuff(0, battleSkillAddBuffData, CharacterBattleData, CharacterBattleData))
                    {
                        BattleBuffObj addBuffObj = OwnerEntity.BattleBuffController.AddBuff(battleSkillAddBuffData, this);
                        if (addBuffObj != null)
                            battleTransferDTO.AddBuffDTOList.Add(new AddBuffDTO()
                            {
                                TargetId = addBuffObj.Owner.UniqueID,
                                BuffId = addBuffObj.BuffId,
                                Round = addBuffObj.NowRound
                            });
                    }
                }
            }
        }
        //为所有目标添加buff
        public void AddAllBuff(List<BattleCharacterEntity> battleCharacterEntitiyList)
        {
            BattleTransferDTO battleTransferDTO = GameEntry.BattleRoomManager.GetBattleRoomEntity(OwnerEntity.RoomID).GetBattleTransfer();
            //if (battleTransferDTO == null)
            //    return;
            float baseProp;
            float selfProp;
            float targetProp;
            battleTransferDTO.AddBuffDTOList = new List<AddBuffDTO>();
            for (int i = 0; i < battleSkillData.battleSkillAddBuffList.Count; i++)
            {
                BattleSkillAddBuffData battleSkillAddBuffData = battleSkillData.battleSkillAddBuffList[i];
                if (battleSkillAddBuffData.TargetType)//受击方
                {
                    for (int j = 0; j < battleCharacterEntitiyList.Count; j++)
                    {
                        if (CanAddBuff(j, battleSkillAddBuffData, CharacterBattleData, battleCharacterEntitiyList[j].CharacterBattleData))
                        {
                            BattleBuffObj addBuffObj = battleCharacterEntitiyList[j].BattleBuffController.AddBuff(battleSkillAddBuffData, this);
                            //客户端无需知道buff是否被覆盖，只需要同buff替换即可
                            if (addBuffObj != null)
                                battleTransferDTO.AddBuffDTOList.Add(new AddBuffDTO()
                                {
                                    TargetId = addBuffObj.Owner.UniqueID,
                                    BuffId = addBuffObj.BuffId,
                                    Round = addBuffObj.NowRound
                                });
                        }
                    }
                }
                else//给自己加
                {
                    baseProp = battleSkillAddBuffData.basePropList[0];
                    selfProp = CharacterBattleData.GetProperty(battleSkillAddBuffData.selfAddBuffProbability.battleSkillBuffProbSource, null);
                    selfProp = selfProp * battleSkillAddBuffData.selfAddBuffProbability.multiplyPropValue / 10000 + battleSkillAddBuffData.selfAddBuffProbability.fixedPropValue;
                    selfProp = battleSkillAddBuffData.selfAddBuffProbability.addOrReduce ? selfProp : -selfProp;
                    if (CanAddBuff(0, battleSkillAddBuffData, CharacterBattleData, CharacterBattleData))
                    {
                        BattleBuffObj addBuffObj = OwnerEntity.BattleBuffController.AddBuff(battleSkillAddBuffData, this);
                        if(addBuffObj!=null)
                            battleTransferDTO.AddBuffDTOList.Add(new AddBuffDTO()
                            {
                                TargetId = addBuffObj.Owner.UniqueID,
                                BuffId = addBuffObj.BuffId,
                                Round = addBuffObj.NowRound
                            });
                    }
                }
            }
        }
        bool CanAddBuff(int targetIndex, BattleSkillAddBuffData battleSkillAddBuffData,CharacterBattleData selfData,CharacterBattleData targetData)
        {
            float baseProp = battleSkillAddBuffData.basePropList[targetIndex];
            float selfProp = selfData.GetProperty(battleSkillAddBuffData.selfAddBuffProbability.battleSkillBuffProbSource, null);
            selfProp = selfProp * battleSkillAddBuffData.selfAddBuffProbability.multiplyPropValue / 10000 + battleSkillAddBuffData.selfAddBuffProbability.fixedPropValue;
            selfProp = battleSkillAddBuffData.selfAddBuffProbability.addOrReduce ? selfProp : -selfProp;
            float targetProp = targetData.GetProperty(battleSkillAddBuffData.targetAddBuffProbability.battleSkillBuffProbSource, null);
            targetProp = targetProp * battleSkillAddBuffData.targetAddBuffProbability.multiplyPropValue / 10000 + battleSkillAddBuffData.targetAddBuffProbability.fixedPropValue;
            targetProp = battleSkillAddBuffData.targetAddBuffProbability.addOrReduce ? selfProp : -selfProp;
            float finalProp = baseProp + selfProp + targetProp;
            int randomValue = Utility.Algorithm.RandomRange(0, 100);
            if (randomValue < finalProp)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 攻击前事件触发
        /// </summary>
        public ISkillAdditionData TriggerSkillEventBeforeAttack(BattleDamageData battleDamageData)
        {
            ISkillAdditionData skillAdditionData = new SkillAdditionData();
            skillEventBeforeAttack?.Invoke( battleDamageData, skillAdditionData);
            return skillAdditionData;
        }
        /// <summary>
        /// 攻击后事件触发
        /// </summary>
        public ISkillAdditionData TriggerSkillEventBehindAttack(BattleDamageData battleDamageData)
        {
            ISkillAdditionData skillAdditionData = new SkillAdditionData();
            skillEventBehindAttack?.Invoke(battleDamageData, skillAdditionData);
            return skillAdditionData;
        }


        public BattleSkillObj(int skillID,BattleCharacterEntity battleCharacterEntity)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, BattleSkillData>>(out var battleSkillDict);
            battleSkillData = battleSkillDict[skillID];
            OwnerEntity = battleCharacterEntity;
            NowCold = 0;

            //添加技能事件
            battleSkillEventBaseList = new List<BattleSkillEventBase>();
            for (int i = 0; i < battleSkillData.battleSkillEventDataList.Count; i++)
            {
                switch (battleSkillData.battleSkillEventDataList[i].battleSkillTriggerEventType)
                {
                    case BattleSkillTriggerEventType.Skill:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_Skill(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                    case BattleSkillTriggerEventType.Heal:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_Heal(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                    case BattleSkillTriggerEventType.SuckBlood:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_SuckBlood(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                    case BattleSkillTriggerEventType.AddCritProp:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_AddCritProp(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                    case BattleSkillTriggerEventType.AddDamage:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_AddDamage(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                    case BattleSkillTriggerEventType.AddPierce:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_AddIgnoreDefence(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                    case BattleSkillTriggerEventType.AddCritDamage:
                        battleSkillEventBaseList.Add(new BattleSkillEvent_AddCritDamage(this, battleSkillData.battleSkillEventDataList[i]));
                        break;
                }
            }

            //添加冷却刷新事件
            BattleController battleController= GameEntry.BattleRoomManager.GetBattleRoomEntity(OwnerEntity.RoomID).BattleController;
            battleController.RoundFinishEvent += ReduceCold;
        }
    }
}
