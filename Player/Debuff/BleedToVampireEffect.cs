using UnityEngine;


public class BleedToVampireEffect : MonoBehaviour, IOnHitEffect
{
    // 출혈을 흡혈로 변경, 스택을 유지  
    [SerializeField] private DebuffEffectSO _bleedEffect;
    [SerializeField] private DebuffEffectSO _vampireEffect;

    public void ApplyEffect(GameObject target)
    {
        ApplyEffect(target, default); // 기본값으로 넘겨도 되지만 데미지 계산은 안 됨
    }

    public void ApplyEffect(GameObject target, DamagePayload payload)
    {
        if (!target.TryGetComponent(out EnemyDebuffHandler debuff)) return;

        int bleedStacks = debuff.GetDebuffStack(DebuffType.Bleed);

        if (bleedStacks > 0)
        {
            debuff.RemoveDebuff(DebuffType.Bleed);
            debuff.ApplyDebuffStacks(_vampireEffect, bleedStacks);
        }
        else
        {
            debuff.ApplyDebuff(_bleedEffect, payload); 
        }
    }
}
