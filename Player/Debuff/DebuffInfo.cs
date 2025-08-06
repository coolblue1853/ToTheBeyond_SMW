using UnityEngine;

public class DebuffInfo
{
    public DebuffType type;
    public float value;
    public float duration;
    public float chance;
    public Sprite icon;
    public RuntimeAnimatorController animatorController;

    public static DebuffInfo FromSO(DebuffEffectSO so)
    {
        return new DebuffInfo
        {
            type = so.type,
            value = so.value,
            duration = so.duration,
            chance = so.applyChance,
            icon = so.icon,
            animatorController = so.animatorController
        };
    }

}