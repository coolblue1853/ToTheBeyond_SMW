using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlatBuff
{
    public StatType type;
    public float value;
    public StatModifier.ModifierMode mode = StatModifier.ModifierMode.Additive;
}

public class StatBuffSkill : ArmorSkill
{
    [SerializeField] private float _duration = 5f; // 지속시간
    [SerializeField] private List<FlatBuff> _buffs; // 적용할 버프

    private readonly List<StatModifier> _activeModifiers = new();


    // 종료시간이 있는 버프스킬 발동
    public override bool Activate()
    {
        if (_stat == null || _buffs == null || _buffs.Count == 0) return false;

        foreach (var buff in _buffs)
        {
            var mod = new StatModifier(buff.type, buff.value, buff.mode);
            _stat.AddModifier(mod);
            _activeModifiers.Add(mod);
        }

        if (_skill?.vfxPrefab)
            Instantiate(_skill.vfxPrefab, _owner.position, Quaternion.identity);

        StartCoroutine(RemoveAfterDelay());
        return true;
    }

    // 일정 시간 후 버프 해제 
    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(_duration);

        foreach (var mod in _activeModifiers)
            _stat.RemoveModifier(mod);

        _activeModifiers.Clear();
    }


    // 파괴될때 자동으로 버프 해제
    private void OnDestroy()
    {
        foreach (var mod in _activeModifiers)
            _stat?.RemoveModifier(mod);

        _activeModifiers.Clear();
    }
}
