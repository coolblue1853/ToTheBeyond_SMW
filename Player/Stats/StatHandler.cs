using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "stats/PlayerStatsSO")]
public class StatHandler : ScriptableObject, IStatProvider, IDamageStatProvider
{
    // ?? ??? 
    [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
    [field: SerializeField] public float JumpForce { get; private set; } = 13f;
    [field: SerializeField] public float DashSpeed { get; private set; } = 20f;
    [field: SerializeField] public int MaxJumps { get; private set; } = 2;
    [field: SerializeField] public int MaxDashCount { get; private set; } = 2;
    [field: SerializeField] public float DashChainWindow { get; private set; } = 0.35f;
    [field: SerializeField] public float DashCooldown { get; private set; } = 5.0f;
    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;

    // ?? ?? ?? ??? 
    [field: SerializeField] public float BaseDamage { get; private set; } = 10f;
    [field: SerializeField] public float DamageMultiplier { get; private set; } = 1f;
    [field: SerializeField] public float PhysicalDamage { get; private set; } = 1f;
    [field: SerializeField] public float MagicalDamage { get; private set; } = 1f;
    [field: SerializeField] public float MeleeDamage { get; private set; } = 1f;
    [field: SerializeField] public float RangedDamage { get; private set; } = 1f;
    [field: SerializeField] public float BasicAttackDamage { get; private set; } = 1f;
    [field: SerializeField] public float SkillDamage { get; private set; } = 1f;
    [field: SerializeField] public float FinalDamageMultiplier { get; private set; } = 1f;
    [field: SerializeField] public float CritChance { get; private set; } = 0.1f;
    [field: SerializeField] public float CritDamageMultiplier { get; private set; } = 2f;
}
