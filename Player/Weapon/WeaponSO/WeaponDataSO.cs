using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SkillPerLevel
{
    public SkillSlot Slot;
    public int LevelRequired;
    public SkillSO Skill;
}
public abstract class WeaponDataSO : ScriptableObject
{
    public WeaponElementType  elementType;
    public WeaponType weaponType;

    public Sprite weaponIcon;
    public string weaponName;
    public string weaponDetail;
    public WeaponKind wKind;
    public DamageType damageType;
    public WeaponCategory category;
    public List<SkillPerLevel> skills = new();
    public float weaponDamageMultiplier = 1.0f;
    public float[] weaponBaseDamage = new float[2] { 10, 15 };
    
    [Header("Attack Debuffs")] 
    public List<DebuffEffectSO> basicAttackDebuffs;
    
    [Header("Animation")]
    public string upperBodyIdleAnim;

    public Vector2 weaponPivot;
}