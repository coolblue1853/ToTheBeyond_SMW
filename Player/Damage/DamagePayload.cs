using UnityEngine;

[System.Serializable]
public struct DamagePayload
{
    // 데미지 관련 값 
    public float baseDamage;
    public float physicalDamage;
    public float magicalDamage;
    public float meleeDamage;
    public float rangedDamage;
    public float basicAttackDamage;
    public float skillDamage;
    public float finalDamageMultiplier;
    public float critChance;
    public float critDamageMultiplier;
    public bool isPiercing;

    public IStatProvider statProvider;


    // 최종 데미지 계산
    // 기본 데미지 * 무기 계수 * 그 외 공격 계수 * 무기타입 * 크리티컬 
    public float GetFinalDamage(float[] weaponBaseDamage, float _weaponDamageMultiplier, bool isSkill = false, bool isMelee = false, bool isCrit = false)
    {
        float typeBonus = (isMelee ? meleeDamage : rangedDamage) - 1f;
        float methodBonus = (isSkill ? skillDamage : basicAttackDamage) - 1f;
        float sourceBonus = (physicalDamage > magicalDamage ? physicalDamage : magicalDamage) - 1f;

        float weaponDamage = Random.Range(weaponBaseDamage[0], weaponBaseDamage[1] + 1);

        float result = (baseDamage + weaponDamage)
                       * _weaponDamageMultiplier
                       * (1f + sourceBonus + typeBonus + methodBonus)
                       * finalDamageMultiplier;

        if (isCrit)
            result *= critDamageMultiplier;

        return Mathf.FloorToInt(result);
    }

    public float GetBleedDamage(float baseBleedValue)
    {
        float result = baseBleedValue * basicAttackDamage * finalDamageMultiplier;
        return Mathf.FloorToInt(result);
    }
}

