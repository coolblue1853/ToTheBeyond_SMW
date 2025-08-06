using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SteamOverdrive")]
public class SteamOverdriveSO : SkillSO
{
    public float requiredHeat = 40f;
    public float buffDuration = 5f;

    public float maxDamageMultiplier = 2.0f;        // 열기 100일 때 배율
    public float maxAttackSpeedMultiplier = 1.5f;   // 열기 100일 때 배율
}
