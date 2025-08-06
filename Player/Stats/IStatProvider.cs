using UnityEngine;

public interface IStatProvider
{
    float MoveSpeed { get; }
    float JumpForce { get; }
    float DashSpeed { get; }
    int MaxJumps { get; }   
    int MaxDashCount { get; }  
    float DashChainWindow { get; }
    float DashCooldown { get; }   
    float MaxHealth { get; }
}
public interface IDamageStatProvider
{
    float BaseDamage { get; }
    float PhysicalDamage { get; }
    float MagicalDamage { get; }
    float MeleeDamage { get; }
    float RangedDamage { get; }
    float BasicAttackDamage { get; }
    float SkillDamage { get; }
    float FinalDamageMultiplier { get; }
    float CritChance { get; }
    float CritDamageMultiplier { get; }
}


