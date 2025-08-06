using System.Collections.Generic;
using UnityEngine;
using System;

public enum StatType
{
    // 이동 관련
    MoveSpeed,
    JumpForce,
    DashSpeed,
    MaxJumps,
    MaxDashCount,
    DashChainWindow,
    DashCooldown,
    MaxHealth,
    
    // 전투 관련
    BaseDamage,
    PhysicalDamage,
    MagicalDamage,
    MeleeDamage,
    RangedDamage,
    BasicAttackDamage,
    SkillDamage,
    FinalDamageMultiplier,
    CritChance,
    CritDamageMultiplier,
    AttackSpeed,
    
    //  원거리 공격 관련
    BulletsPerShot,
    MaxAmmo,
    BulletSpeed,
    ReloadSpeed,
    
    // 상태이상 관련
    BleedChance,
    VampireAbsorb,
    
    // 피해 관련
    DamageTakenIncrease
}

public class StatModifier
{
    public StatType type;
    public float value;
    public ModifierMode mode;
    public string sourceTag;
    
    public enum ModifierMode { Additive, Multiplicative }

    public StatModifier(StatType type, float value, ModifierMode mode = ModifierMode.Additive, string sourceTag = null)
    {
        this.type = type;
        this.value = value;
        this.mode = mode;
        this.sourceTag = sourceTag;
    }

    // 능력치 부여자 체크 
    public override bool Equals(object obj)
    {
        if (obj is not StatModifier other) return false;
        return type == other.type && Mathf.Approximately(value, other.value) && mode == other.mode && sourceTag == other.sourceTag;
    }

    public override int GetHashCode()
    {
        return type.GetHashCode() ^ value.GetHashCode() ^ mode.GetHashCode();
    }
    
}


public class RuntimeStat : IStatProvider, IDamageStatProvider
{
    // 실시간으로 사용되고 변경되는 런타임 스탯

    private readonly StatHandler baseStats; // SO로 가져와지는 기본 스탯 
    private readonly List<StatModifier> modifiers = new();
    public event Action<StatType> OnStatChanged;

    public RuntimeStat(StatHandler baseStats)
    {
        this.baseStats = baseStats;
    }

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        OnStatChanged?.Invoke(mod.type);
    }

    public void RemoveModifier(StatModifier mod)
    {
        if (modifiers.Remove(mod))
        {
            OnStatChanged?.Invoke(mod.type);
        }
    }
    public bool RemoveAllModifiersOfTypeWithTag(StatType type, string tag)
    {
        int removed = modifiers.RemoveAll(m => m.type == type && m.sourceTag == tag);
    
        if (removed > 0)
            OnStatChanged?.Invoke(type);

        return removed > 0;
    }

    public void ClearModifiers() => modifiers.Clear();

    private float GetValue(StatType type, float baseValue)
    {
        float result = baseValue;
        foreach (var mod in modifiers)
        {
            if (mod.type != type) continue;
            switch (mod.mode)
            {
                case StatModifier.ModifierMode.Additive: result += mod.value; break;
                case StatModifier.ModifierMode.Multiplicative: result *= mod.value; break;
            }
        }
        return result;
    }

    // 스탯을 가져오는 프로퍼티
    // 기본 능력치 
    public float MoveSpeed => GetValue(StatType.MoveSpeed, baseStats.MoveSpeed);
    public float JumpForce => GetValue(StatType.JumpForce, baseStats.JumpForce);
    public float DashSpeed => GetValue(StatType.DashSpeed, baseStats.DashSpeed);
    public int MaxJumps => Mathf.RoundToInt(GetValue(StatType.MaxJumps, baseStats.MaxJumps));
    public int MaxDashCount => Mathf.RoundToInt(GetValue(StatType.MaxDashCount, baseStats.MaxDashCount));
    public float DashChainWindow => GetValue(StatType.DashChainWindow, baseStats.DashChainWindow);
    public float DashCooldown => GetValue(StatType.DashCooldown, baseStats.DashCooldown);
    public float MaxHealth => GetValue(StatType.MaxHealth, baseStats.MaxHealth);

    // 데미지 관련
    public float BaseDamage => GetValue(StatType.BaseDamage, baseStats.BaseDamage);
    public float PhysicalDamage => GetValue(StatType.PhysicalDamage, baseStats.PhysicalDamage);
    public float MagicalDamage => GetValue(StatType.MagicalDamage, baseStats.MagicalDamage);
    public float MeleeDamage => GetValue(StatType.MeleeDamage, baseStats.MeleeDamage);
    public float RangedDamage => GetValue(StatType.RangedDamage, baseStats.RangedDamage);
    public float BasicAttackDamage => GetValue(StatType.BasicAttackDamage, baseStats.BasicAttackDamage);
    public float SkillDamage => GetValue(StatType.SkillDamage, baseStats.SkillDamage);
    public float FinalDamageMultiplier => GetValue(StatType.FinalDamageMultiplier, baseStats.FinalDamageMultiplier);
    public float CritChance => GetValue(StatType.CritChance, baseStats.CritChance);
    public float CritDamageMultiplier => GetValue(StatType.CritDamageMultiplier, baseStats.CritDamageMultiplier);

    // 원거리 무기 관련
    public float MaxAmmo => (GetValue(StatType.MaxAmmo, 1f)); // 기본 1배
    public float AttackSpeed => GetValue(StatType.AttackSpeed, 1f);
    public float BulletSpeed => GetValue(StatType.BulletSpeed, 1f);
    public int BulletsPerShot => Mathf.RoundToInt(GetValue(StatType.BulletsPerShot, 0f));
    public float ReloadSpeed => GetValue(StatType.ReloadSpeed, 1f);
    
    // 상태이상
    public float BleedChance => GetValue(StatType.BleedChance, 0f);
    public float TemporaryBleedChanceBonus { get; set; } = 0f;
    
    
    [field: SerializeField] public DebuffEffectSO bleedDebuffSO { get; private set; }

    // 피해 관련
    public float DamageTakenIncrease => GetValue(StatType.DamageTakenIncrease, 0f); // 기본값 0%

    

}
