using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionServer.Model;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    /// <summary>
    /// 用于进行战斗中角色的属性数据处理
    /// </summary>
    public class CharacterBattleData : IReference, ICharacterBattleData
    {
        BattleCharacterEntity owner;

        int hp;
        public int Hp { get { return hp + buffCharacterData.Hp; } }
        int maxHp;
        public int MaxHp { get { return maxHp + buffCharacterData.MaxHp; } }
        int mp;
        public int Mp { get { return mp + buffCharacterData.Mp; } }
        int maxMp;
        public int MaxMp { get { return maxMp + buffCharacterData.MaxMp; } }
        int soul;
        public int Soul { get { return soul + buffCharacterData.Soul; } }
        int maxSoul;
        public int MaxSoul { get { return maxSoul + buffCharacterData.MaxSoul; } }
        int bestBlood;
        public int BestBlood { get { return bestBlood + buffCharacterData.BestBlood; } }
        int bestBloodMax;
        public int BestBloodMax { get { return bestBloodMax + buffCharacterData.BestBloodMax; } }
        float attackSpeed;
        public float AttackSpeed { get { return attackSpeed + buffCharacterData.AttackSpeed; } }
        int physicalAtk;
        public int PhysicalAtk { get { return physicalAtk + buffCharacterData.PhysicalAtk; } }
        int physicalDef;
        public int PhysicalDef { get { return physicalDef + buffCharacterData.PhysicalDef; } }
        int powerAtk;
        public int PowerAtk { get { return powerAtk + buffCharacterData.PowerAtk; } }
        int powerDef;
        public int PowerDef { get { return powerDef + buffCharacterData.PowerDef; } }
        float physicalCritProb;
        public float PhysicalCritProb { get { return physicalCritProb + buffCharacterData.PhysicalCritProb; } }
        float magicCritProb;
        public float MagicCritProb { get { return magicCritProb + buffCharacterData.MagicCritProb; } }
        float reduceCritProb;
        public float ReduceCritProb { get { return reduceCritProb + buffCharacterData.ReduceCritProb; } }
        int physicalCritDamage;
        public int PhysicalCritDamage { get { return physicalCritDamage + buffCharacterData.PhysicalCritDamage; } }
        int magicCritDamage;
        public int MagicCritDamage { get { return magicCritDamage + buffCharacterData.MagicCritDamage; } }
        int reduceCritDamage;
        public int ReduceCritDamage { get { return reduceCritDamage + buffCharacterData.ReduceCritDamage; } }
        int ignoreDef;
        public int IgnoreDef { get { return ignoreDef + buffCharacterData.IgnoreDef; } }
        int damageAddition;
        public int DamageAddition { get { return damageAddition + buffCharacterData.DamageAddition; } }
        int damageDeduction;
        public int DamageDeduction { get { return damageDeduction + buffCharacterData.DamageDeduction; } }
        int shield;
        public int Shield { get { return shield + buffCharacterData.Shield; } }
        int dodgeProp;
        public int DodgeProp { get { return dodgeProp + buffCharacterData.DodgeProp; } }
        int physicDodgeProp;
        public int PhysicDodgeProp { get { return physicDodgeProp + buffCharacterData.PhysicDodgeProp; } }
        int powerDodgeProp;
        public int PowerDodgeProp { get { return powerDodgeProp + buffCharacterData.PowerDodgeProp; } }
        int healEffect;
        public int HealEffect { get { return healEffect + buffCharacterData.HealEffect; } }

        ICharacterBattleData buffCharacterData;
        /// <summary>
        /// 获取对应属性
        /// </summary>
        public float GetProperty(BattleSkillNumSourceType battleSkillNumSourceType, BattleDamageData battleDamageData)
        {
            switch (battleSkillNumSourceType)
            {
                case BattleSkillNumSourceType.MaxHealth:
                    return MaxHp;
                case BattleSkillNumSourceType.NowHealth:
                    return Hp;
                case BattleSkillNumSourceType.HasLostHealth:
                    return MaxHp - Hp;
                case BattleSkillNumSourceType.MaxZhenYuan:
                    return MaxMp;
                case BattleSkillNumSourceType.NowZhenYuan:
                    return Mp;
                case BattleSkillNumSourceType.MaxShenHun:
                    return MaxSoul;
                case BattleSkillNumSourceType.NowShenHun:
                    return Soul;
                case BattleSkillNumSourceType.PhysicAttack:
                    return PhysicalAtk;
                case BattleSkillNumSourceType.MagicAttack:
                    return PowerAtk;
                case BattleSkillNumSourceType.PhysicDefense:
                    return PhysicalDef;
                case BattleSkillNumSourceType.MagicDefense:
                    return PowerDef;
                case BattleSkillNumSourceType.AttackSpeed:
                    return AttackSpeed;
                case BattleSkillNumSourceType.TakeDamage:
                    return battleDamageData.damageNum;
                default:
                    return 0;
            }
        }
        public int GetProperty(BattleSkillDamageType battleSkillDamageType)
        {
            switch (battleSkillDamageType)
            {
                case BattleSkillDamageType.Physic:
                    return PhysicalDef;
                case BattleSkillDamageType.Magic:
                    return powerDef;
                case BattleSkillDamageType.ShenHun:
                    return 0;
                case BattleSkillDamageType.Reality:
                    return 0;
                default:
                    return 0;
            }
        }
        public int GetProperty(BattleSkillEventTriggerNumSourceType battleSkillEventTriggerNumSourceType)
        {
            switch (battleSkillEventTriggerNumSourceType)
            {
                case BattleSkillEventTriggerNumSourceType.Health:
                    return Hp;
                case BattleSkillEventTriggerNumSourceType.PhysicDefense:
                    return PhysicalDef;
                case BattleSkillEventTriggerNumSourceType.MagicDefense:
                    return PowerDef;
                case BattleSkillEventTriggerNumSourceType.ShenHun:
                    return Soul;
                case BattleSkillEventTriggerNumSourceType.Shield:
                    return Shield;
                default:
                    return 0;
            }
        }
        public int GetProperty(BuffEvent_Shield_SourceDataType buffEvent_Shield_SourceDataType)
        {
            switch (buffEvent_Shield_SourceDataType)
            {
                case BuffEvent_Shield_SourceDataType.MaxHealth:
                    return MaxHp;
                case BuffEvent_Shield_SourceDataType.MaxZhenYuan:
                    return MaxMp;
                case BuffEvent_Shield_SourceDataType.MaxShenHun:
                    return MaxSoul;
                case BuffEvent_Shield_SourceDataType.TakeDamageNum:
                    //todo 根据伤害值获取暂定
                    return 0;
                case BuffEvent_Shield_SourceDataType.ReceiveDamageNum:
                    //todo 根据造成伤害值获取暂定
                    return 0;
                case BuffEvent_Shield_SourceDataType.PhysicalAttack:
                    return PhysicalAtk;
            }
            return 0;
        }
        public int GetProperty(BuffEvent_DamageOrHeal_SourceDataType buffEvent_DamageOrHeal_SourceDataType)
        {
            switch (buffEvent_DamageOrHeal_SourceDataType)
            {
                case BuffEvent_DamageOrHeal_SourceDataType.MaxHealth:
                    return MaxHp;
                case BuffEvent_DamageOrHeal_SourceDataType.MaxZhenYuan:
                    return MaxMp;
                case BuffEvent_DamageOrHeal_SourceDataType.MaxShenHun:
                    return MaxSoul;
                case BuffEvent_DamageOrHeal_SourceDataType.TakeDamageNum:
                    //todo 根据伤害值获取暂定
                    return 0;
                case BuffEvent_DamageOrHeal_SourceDataType.ReceiveDamageNum:
                    //todo 根据造成伤害值获取暂定
                    return 0;
            }
            return 0;
        }
        public int GetProperty(BattleBuffCondition_SourcePropertyType battleBuffCondition_SourcePropertyType)
        {
            switch (battleBuffCondition_SourcePropertyType)
            {
                case BattleBuffCondition_SourcePropertyType.Health:
                    return Hp;
                case BattleBuffCondition_SourcePropertyType.ZhenYuan:
                    return Mp;
                case BattleBuffCondition_SourcePropertyType.ShenHun:
                    return Soul;
                case BattleBuffCondition_SourcePropertyType.JingXue:
                    return BestBlood;
                default:
                    return 0;
            }
        }
   

        /// <summary>
        /// 获取基础对应属性
        /// </summary>
        public float GetBaseProperty(BuffEvent_RolePropertyChange_SourceDataType buffEvent_RolePropertyChange_SourceDataType, BattleSkillObj battleSkillBase)
        {
            switch (buffEvent_RolePropertyChange_SourceDataType)
            {
                case BuffEvent_RolePropertyChange_SourceDataType.MaxHealth:
                    return maxHp;
                case BuffEvent_RolePropertyChange_SourceDataType.MaxZhenYuan:
                    return maxMp;
                case BuffEvent_RolePropertyChange_SourceDataType.MaxShenHun:
                    return maxSoul;
                case BuffEvent_RolePropertyChange_SourceDataType.PhysicAttack:
                    return physicalAtk;
                case BuffEvent_RolePropertyChange_SourceDataType.MagicAttack:
                    return powerAtk;
                case BuffEvent_RolePropertyChange_SourceDataType.PhysicDefend:
                    return physicalDef;
                case BuffEvent_RolePropertyChange_SourceDataType.MagicDefend:
                    return powerDef;
                case BuffEvent_RolePropertyChange_SourceDataType.TakeDamage:
                    return battleSkillBase.LastDamageValue;
                case BuffEvent_RolePropertyChange_SourceDataType.SkillDamage:
                    return battleSkillBase.LastAttackValue;
                case BuffEvent_RolePropertyChange_SourceDataType.AttackSpeed:
                    return attackSpeed;
                default:
                    return 0;
            }
        }

        public int GetPropertyPercent(BattleSkillEventTriggerNumSourceType battleSkillEventTriggerNumSourceType)
        {
            switch (battleSkillEventTriggerNumSourceType)
            {
                case BattleSkillEventTriggerNumSourceType.Health:
                    Utility.Debug.LogError("获取血量百分比" + Hp + "/" + MaxHp + "/" + Hp * 100 / MaxHp);
                    return Hp * 100 / MaxHp;
                case BattleSkillEventTriggerNumSourceType.ShenHun:
                    return Soul * 100 / MaxSoul;
                case BattleSkillEventTriggerNumSourceType.PhysicDefense:
                case BattleSkillEventTriggerNumSourceType.MagicDefense:
                case BattleSkillEventTriggerNumSourceType.Shield:
                    return 100;
                default:
                    return 0;
            }
        }
        public int GetPropertyPercent(BattleBuffCondition_SourcePropertyType battleBuffCondition_SourcePropertyType)
        {
            switch (battleBuffCondition_SourcePropertyType)
            {
                case BattleBuffCondition_SourcePropertyType.Health:
                    return Hp * 100 / MaxHp;
                case BattleBuffCondition_SourcePropertyType.ZhenYuan:
                    return Mp * 100 / MaxMp;
                case BattleBuffCondition_SourcePropertyType.ShenHun:
                    return Soul * 100 / MaxSoul;
                case BattleBuffCondition_SourcePropertyType.JingXue:
                    return BestBlood * 100 / BestBloodMax;
                default:
                    return 0;
            }
        }


        public void ChangeProperty(BattleSkillDamageTargetProperty baseDamageTargetProperty, int damageNum)
        {
            if (damageNum == 0)
                return;
            switch (baseDamageTargetProperty)
            {
                case BattleSkillDamageTargetProperty.Health:
                    hp += damageNum;
                    hp = hp < 0 ? 0 : hp;
                    hp = hp > MaxHp ? MaxHp : hp;
                    break;
                case BattleSkillDamageTargetProperty.ZhenYuan:
                    mp += damageNum;
                    mp = mp < 0 ? 0 : mp;
                    mp = mp > MaxMp ? MaxMp : mp;
                    break;
                case BattleSkillDamageTargetProperty.ShenHun:
                    soul += damageNum;
                    soul = soul < 0 ? 0 : soul;
                    soul = soul > MaxSoul ? MaxSoul : soul;
                    break;
            }
        }
        public void ChangeProperty(BuffEvent_DamageOrHeal_EffectTargetType buffEvent_DamageOrHeal_EffectTargetType, int damageNum)
        {
            switch (buffEvent_DamageOrHeal_EffectTargetType)
            {
                case BuffEvent_DamageOrHeal_EffectTargetType.Health:
                    hp += damageNum;
                    hp = hp < 0 ? 0 : hp;
                    hp = hp > MaxHp ? MaxHp : hp;
                    break;
                case BuffEvent_DamageOrHeal_EffectTargetType.ShenHun:
                    mp += damageNum;
                    mp = mp < 0 ? 0 : mp;
                    mp = mp > MaxMp ? MaxMp : mp;
                    break;
                case BuffEvent_DamageOrHeal_EffectTargetType.ZhenYuan:
                    soul += damageNum;
                    soul = soul < 0 ? 0 : soul;
                    soul = soul > MaxSoul ? MaxSoul : soul;
                    break;
            }
        }
        public void ChangeProperty(BattleDamageData battleDamageData)
        {
            //死亡前事件触发
            if (battleDamageData.battleSkillActionType == BattleSkillActionType.Damage)
            {
                if (Hp + battleDamageData.damageNum + battleDamageData.extraDamageNum <= 0)
                {
                    owner.BattleBuffController.TriggerBuffEventBeforeRoleDie(null, battleDamageData: battleDamageData);
                }
            }

            //触发伤害前buff事件（目前仅针对护盾）
            owner.BattleBuffController.TriggerBuffEventBeforePropertyChange(owner, battleDamageData: battleDamageData);

            ChangeProperty(battleDamageData.baseDamageTargetProperty, battleDamageData.damageNum);
            ChangeProperty(battleDamageData.extraDamageTargetProperty, battleDamageData.extraDamageNum);

            owner.BattleBuffController.TriggerBuffEventAfterPropertyChange();
        }
        public void ChangeProperty(BattleSkillCostType battleSkillCostType,int costNum)
        {
            switch (battleSkillCostType)
            {
                case BattleSkillCostType.Health:
                    hp += costNum;
                    hp = hp < 0 ? 0 : hp;
                    hp = hp > MaxHp ? MaxHp : hp;
                    break;
                case BattleSkillCostType.ZhenYuan:
                    mp += costNum;
                    mp = mp < 0 ? 0 : mp;
                    mp = mp > MaxMp ? MaxMp : mp;
                    break;
                case BattleSkillCostType.ShenHun:
                    soul += costNum;
                    soul = soul < 0 ? 0 : soul;
                    soul = soul > MaxSoul ? MaxSoul : soul;
                    break;
                case BattleSkillCostType.JingXue:
                    bestBlood += costNum;
                    bestBlood = bestBlood < 0 ? 0 : bestBlood;
                    bestBlood = bestBlood > BestBloodMax ? BestBloodMax : bestBlood;
                    break;
            }
        }
        public void Init(RoleStatus roleStatus, BattleCharacterEntity owner)
        {
            this.owner = owner;
            buffCharacterData = owner.BattleBuffController.BuffCharacterData;

            hp = roleStatus.RoleHP;
            maxHp = roleStatus.RoleMaxHP;
            mp = roleStatus.RoleMP;
            maxMp = roleStatus.RoleMaxMP;
            soul = roleStatus.RoleSoul;
            maxSoul = roleStatus.RoleMaxSoul;
            bestBlood = roleStatus.BestBlood;
            bestBloodMax = roleStatus.BestBloodMax;
            attackSpeed = roleStatus.AttackSpeed;
            physicalAtk = roleStatus.AttackPhysical;
            physicalDef = roleStatus.DefendPhysical;
            powerAtk = roleStatus.AttackPower;
            powerDef = roleStatus.DefendPower;
            physicalCritProb = roleStatus.PhysicalCritProb;
            magicCritProb = roleStatus.MagicCritProb;
            reduceCritProb = roleStatus.ReduceCritProb;
            physicalCritDamage = roleStatus.PhysicalCritDamage;
            magicCritDamage = roleStatus.MagicCritDamage;
            reduceCritDamage = roleStatus.ReduceCritDamage;
            healEffect = 100;
        }
        public void Init(PetStatus petStatus, BattleCharacterEntity owner)
        {
            this.owner = owner;
            buffCharacterData = owner.BattleBuffController.BuffCharacterData;

            hp = petStatus.PetHP;
            maxHp = petStatus.PetMaxHP;
            mp = petStatus.PetMP;
            maxMp = petStatus.PetMaxMP;
            soul = petStatus.PetShenhun;
            maxSoul = petStatus.PetMaxShenhun;
            bestBlood = 0;
            bestBloodMax = 0;
            attackSpeed = petStatus.AttackSpeed;
            physicalAtk = petStatus.AttackPhysical;
            physicalDef = petStatus.DefendPhysical;
            powerAtk = petStatus.AttackPower;
            powerDef = petStatus.DefendPower;
            physicalCritProb = petStatus.PhysicalCritProb;
            magicCritProb = petStatus.MagicCritProb;
            reduceCritProb = petStatus.ReduceCritProb;
            physicalCritDamage = petStatus.PhysicalCritDamage;
            magicCritDamage = petStatus.MagicCritDamage;
            reduceCritDamage = petStatus.ReduceCritDamage;
        }
        public void Init(MonsterDatas monsterDatas, BattleCharacterEntity owner)
        {
            this.owner = owner;
            buffCharacterData = owner.BattleBuffController.BuffCharacterData;

            hp = monsterDatas.Monster_HP;
            maxHp = monsterDatas.Monster_HP;
            mp = monsterDatas.Monster_MP;
            maxMp = monsterDatas.Monster_MP;
            soul = monsterDatas.Monster_Soul;
            maxSoul = monsterDatas.Monster_Soul;
            bestBlood = 0;
            bestBloodMax = 0;
            attackSpeed = monsterDatas.Attact_Speed;
            physicalAtk = monsterDatas.Attact_Physical;
            physicalDef = monsterDatas.Defend_Physical;
            powerAtk = monsterDatas.Attact_Power;
            powerDef = monsterDatas.Defend_Power;
            physicalCritProb = monsterDatas.PhysicalCritProb;
            magicCritProb = monsterDatas.MagicCritProb;
            reduceCritProb = monsterDatas.ReduceCritProb;
            physicalCritDamage = monsterDatas.PhysicalCritDamage;
            magicCritDamage = monsterDatas.MagicCritDamage;
            reduceCritDamage = monsterDatas.ReduceCritProb;
        }


        public void Release()
        {
        }
    }
}
