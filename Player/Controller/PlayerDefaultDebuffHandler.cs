using UnityEngine;

public class PlayerDefaultDebuffHandler : MonoBehaviour
{
    // 기본공격에 의한 디버프 부여 SO 
    public DebuffEffectSO bleedSO;
    public DebuffEffectSO poisonSO;
    public DebuffEffectSO burnSO;

    public DebuffEffectSO GetDebuff(DebuffType type, float applyChanceBonus = 0f)
    {
        DebuffEffectSO source = type switch
        {
            DebuffType.Bleed => bleedSO,
            DebuffType.Poison => poisonSO,
            DebuffType.Burn => burnSO,
            _ => null
        };

        if (source == null) return null;

        var clone = ScriptableObject.CreateInstance<DebuffEffectSO>();
        clone.type = source.type;
        clone.value = source.value;
        clone.duration = source.duration;
        clone.icon = source.icon;
        clone.debuffSfxName = source.debuffSfxName;
        clone.applyChance = Mathf.Clamp01(source.applyChance + applyChanceBonus);

        return clone;
    }
}
