using System.Collections.Generic;
using UnityEngine;

public class PassiveArmorEffect : MonoBehaviour
{
    private SkillSO _skill;
    private RuntimeStat _stat;
    private List<StatModifier> _appliedModifiers = new();

    public void Initialize(SkillSO skill, RuntimeStat stat, int level)
    {
        _skill = skill;
        _stat = stat;
        Apply(level);
    }

    public void Reinitialize(int newLevel)
    {
        foreach (var mod in _appliedModifiers)
            _stat.RemoveModifier(mod);
        _appliedModifiers.Clear();
        Apply(newLevel);
    }

    // 패시브 능력치 적용
    private void Apply(int level)
    {
        foreach (var statInfo in _skill.stats)
        {
            float value = statInfo.values[Mathf.Clamp(level - 1, 0, statInfo.values.Length - 1)];
            var modifier = new StatModifier(statInfo.type, value, statInfo.Mode);
            _stat.AddModifier(modifier);
            _appliedModifiers.Add(modifier);
        }
    }

    // 적용된 능력치를 런타임 스탯에서 제거 
    public void RemoveAllModifiers()
    {
        foreach (var mod in _appliedModifiers)
        {
            _stat.RemoveModifier(mod);
        }
        _appliedModifiers.Clear();
    }
}