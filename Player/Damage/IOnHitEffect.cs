using UnityEngine;

public interface IOnHitEffect
{
    void ApplyEffect(GameObject target);
    void ApplyEffect(GameObject target, DamagePayload payload);
}
