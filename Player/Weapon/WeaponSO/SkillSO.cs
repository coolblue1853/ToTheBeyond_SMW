using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Active, Passive, Charge, Stack }
public enum SkillInputType { Instant, Hold }

[System.Serializable]
public class StatPerLevel
{
    public StatType type;
    public float[] values = new float[3]; // 1~3 레벨용
    public StatModifier.ModifierMode Mode = StatModifier.ModifierMode.Additive;
}


[CreateAssetMenu(menuName = "Skill/SkillSO")]
public class SkillSO : ScriptableObject
{
    [Header("UI 아이콘")]
    public Sprite iconSprite;   
    public Sprite iconBackSprite;   
    public string skillName;
    public SkillSlot slot;
    public string description;
    public float baseDamageByLevel;
    public SkillInputType inputType;

    [Header("스탯 변화 효과 (레벨별)")]
    public List<StatPerLevel> stats;
    [Header("레벨별 효과 정의")]

    public SkillType type;
    [Header("기타 옵션")]
    public GameObject vfxPrefab;
    public float cooldown;
    public int maxStack;
    public float chargeTime;

    [Header("프리팹 기반 실행 로직")]
    public GameObject skillLogicPrefab; // 프리팹 안에 WeaponSkill이 붙어 있어야 함
    
    [Header("버프 기반 스킬")]
    public float duration;
    public float[] buffPower;
    public GameObject additionalAttackPrefab; 
    
    [Header("사운드 설정")]
    public string skillSfxName;
}
