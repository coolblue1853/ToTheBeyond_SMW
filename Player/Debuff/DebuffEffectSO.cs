using UnityEngine;

public enum DebuffType { DamageTakenIncrease, MarkedTarget ,Slow, Poison, Burn, Bleed,VampireAbsorb }

[CreateAssetMenu(menuName = "Debuff/Effect")]
public class DebuffEffectSO : ScriptableObject
{
    public DebuffType type;
    public float value;
    public float duration;
    public float applyChance = 1.0f; // 디버프 적용 확률 1 = 100%
    public Sprite icon;
    public string debuffSfxName;
    public RuntimeAnimatorController animatorController;
}