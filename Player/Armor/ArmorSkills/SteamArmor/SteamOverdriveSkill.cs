using UnityEngine;
using System.Collections;

public class SteamOverdriveSkill : ArmorSkill
{
    // 증기를 사용한 능력치 버프 스킬
    private SteamArmor _steamArmor;
    private RuntimeStat _stat;

    private float _requiredHeat;
    private float _duration;
    private float _maxDmgMult;
    private float _maxAtkSpeedMult;

    private Coroutine _buffCoroutine;

    private StatModifier _dmgBuff;
    private StatModifier _atkSpdBuff;

    public override void Initialize(SkillSO skillData, Transform owner, Armor armor, int level)
    {
        base.Initialize(skillData, owner, armor, level);

        _steamArmor = armor as SteamArmor;
        _stat = armor.playerStat;

        var so = skillData as SteamOverdriveSO;
        _requiredHeat = so.requiredHeat;

        _duration = so.buffDuration;
        _maxDmgMult = so.maxDamageMultiplier;
        _maxAtkSpeedMult = so.maxAttackSpeedMultiplier;
    }


    // 증기수치가 요구치만큼 있다면 사용 후 능력치 증가
    public override bool Activate()
    {
        if (_steamArmor == null || _stat == null) return false;

        if (_steamArmor.IsOverheated)
        {
            return false;
        }

        float currentHeat = _steamArmor.CurrentHeat;
        if (currentHeat < _requiredHeat)
        {
            return false;
        }

        float heatRatio = Mathf.Clamp01(currentHeat / _steamArmor.MaxHeat);
        _steamArmor.ReduceHeat(currentHeat); // 전체 소모

        float dmgMult = Mathf.Lerp(1f, _maxDmgMult, heatRatio);
        float atkSpdMult = Mathf.Lerp(1f, _maxAtkSpeedMult, heatRatio);

        // 기존 버프 종료(중첩 방지)
        if (_buffCoroutine != null)
        {
            _stat.RemoveModifier(_dmgBuff);
            _stat.RemoveModifier(_atkSpdBuff);
            StopCoroutine(_buffCoroutine);
        }

        _buffCoroutine = StartCoroutine(OverdriveRoutine(dmgMult, atkSpdMult));
        return true;
    }


    private IEnumerator OverdriveRoutine(float dmgMult, float atkSpdMult)
    {
        _dmgBuff = new StatModifier(StatType.BaseDamage, dmgMult, StatModifier.ModifierMode.Multiplicative);
        _atkSpdBuff = new StatModifier(StatType.AttackSpeed, atkSpdMult, StatModifier.ModifierMode.Multiplicative);

        _stat.AddModifier(_dmgBuff);
        _stat.AddModifier(_atkSpdBuff);

        yield return new WaitForSeconds(_duration);

        _stat.RemoveModifier(_dmgBuff);
        _stat.RemoveModifier(_atkSpdBuff);

        _buffCoroutine = null;
    }
}
