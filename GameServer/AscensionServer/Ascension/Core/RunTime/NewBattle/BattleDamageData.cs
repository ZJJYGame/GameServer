using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer
{
    /// <summary>
    /// 战斗伤害数据
    /// </summary>
    public class BattleDamageData
    {
        //伤害目标ID
        public int TargetID;
        //攻击段数
        public int attackSection;
        //行为类型
        public BattleSkillActionType battleSkillActionType;
        //伤害类型
        public BattleSkillDamageType damageType;
        //基础伤害是否暴击,是=>暴击，否=>不暴击
        public bool isCrit;
        /// <summary>
        /// 基础伤害的目标属性（血量，真元，神魂）
        /// </summary>
        public BattleSkillDamageTargetProperty baseDamageTargetProperty;
        //伤害数字
        public int damageNum;
        /// <summary>
        /// 额外伤害的目标属性（血量，真元，神魂）
        /// </summary>
        public BattleSkillDamageTargetProperty extraDamageTargetProperty;
        //额外伤害
        public int extraDamageNum;
        //对护盾造成的伤害
        public int shieldDamage;
        //添加buff的id列表
        public List<int> addBuffList;
        //初始攻击值
        public int initialAttackValue;
        public int initialDefendValue;
        List<ISkillAdditionData> actorSkillAdditionDataList;
        List<ISkillAdditionData> targetSkillAdditionDataList;
        public CharacterBattleData selfCharacterBattleData;
        public CharacterBattleData targetCharacterBattleData;
        public void GetDamage()
        {
            int attackValue = initialAttackValue;
            int defendValue = 0;
            if (battleSkillActionType == BattleSkillActionType.Damage)
            {
                defendValue = initialDefendValue * (100 - selfCharacterBattleData.IgnoreDef - actorSkillAdditionDataList.Sum(p => p.IgnoreDefensive) - targetSkillAdditionDataList.Sum(p => p.IgnoreDefensive)) / 100;
                defendValue = defendValue < 0 ? 0 : defendValue;
            }
            int damageValue = 0;
            if (damageType == BattleSkillDamageType.Physic || damageType == BattleSkillDamageType.Magic)
            {
                Utility.Debug.LogError($"对{TargetID}伤害加成{actorSkillAdditionDataList.Sum(p => p.DamgeAddition)}--{targetSkillAdditionDataList.Sum(p => p.DamgeAddition)}");
                damageValue = (attackValue - defendValue) * (100 + selfCharacterBattleData.DamageAddition + actorSkillAdditionDataList.Sum(p => p.DamgeAddition) + targetSkillAdditionDataList.Sum(p => p.DamgeAddition) - targetCharacterBattleData.DamageDeduction) / 100;
            }
            else if (damageType == BattleSkillDamageType.Reality || damageType == BattleSkillDamageType.ShenHun)
                damageValue = attackValue;
            //计算暴击伤害
            if (isCrit)
            {
                int finalCritDamage = 0;
                if (damageType == BattleSkillDamageType.Physic)
                    finalCritDamage = selfCharacterBattleData.PhysicalCritDamage - targetCharacterBattleData.ReduceCritDamage;
                else if (damageType == BattleSkillDamageType.Magic)
                    finalCritDamage = selfCharacterBattleData.MagicCritDamage - targetCharacterBattleData.ReduceCritDamage;
                damageValue = damageValue * (200 + finalCritDamage + actorSkillAdditionDataList.Sum(p => p.CritDamage) + targetSkillAdditionDataList.Sum(p => p.CritDamage)) / 100;
            }
            damageValue = damageValue <= 0 ? 1 : damageValue;
            damageNum = battleSkillActionType == BattleSkillActionType.Damage ? -damageValue : damageValue;
        }
        public void AddActorSkillAddition(params ISkillAdditionData[] skillAdditionDatas)
        {
            actorSkillAdditionDataList.AddRange(skillAdditionDatas);
        }
        public void AddtargetSkillAddition(params ISkillAdditionData[] skillAdditionDatas)
        {
            targetSkillAdditionDataList.AddRange(skillAdditionDatas);
        }
        public  BattleDamageData()
        {
            actorSkillAdditionDataList = new List<ISkillAdditionData>();
            targetSkillAdditionDataList = new List<ISkillAdditionData>();
        }
    }
}
