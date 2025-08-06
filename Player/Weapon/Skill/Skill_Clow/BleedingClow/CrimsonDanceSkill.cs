using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

public class CrimsonDanceSkill : WeaponSkill
{
    // 공격속도와 출혈 확률를 높이는 버프
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _attackSpeedBonusPreEvolve = 0.3f;
    [SerializeField] private float _attackSpeedBonusPostEvolve = 0.45f;
    [SerializeField] private float _bleedChanceBonusPreEvolve = 0.3f;
    [SerializeField] private float _bleedChanceBonusPostEvolve = 0.8f;
    [SerializeField] private bool _isEvolved;
    [SerializeField] private string _sfxName;

    private readonly List<StatModifier> _activeModifiers = new();
    private Coroutine _activeBuffRoutine;

    // 애니메이션 관련 변수 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _buffAnimStr;
    [SerializeField] private string _idleAnimStr;

    // 사운드 관련 변수 
    [SerializeField] private GameObject _buffSFX;
    [SerializeField] private float _sfxDestroyTime = 1.5f;

    public override bool Activate()
    {
        if (_weapon == null)
            return false;

        // 기존 버프가 있다면 강제로 종료
        if (_activeBuffRoutine != null)
        {
            _weapon.StopCoroutine(_activeBuffRoutine);
            RemoveCurrentBuffs();
        }
        
        if (!(_weapon is MeleeWeapon melee)) return false;
        _animController = melee.playerController.GetComponent<PlayerAnimatorController>();

        _activeBuffRoutine = _weapon.StartCoroutine(ApplyCrimsonDance());

        var vfx = Instantiate(_buffSFX, transform.position, transform.rotation);
        vfx.transform.SetParent(transform.root);
        Destroy(vfx, _sfxDestroyTime);

        _weapon.StartCoroutine(DelayRoutine()); // 쿨타임 즉시 시작
        return true;
    }

    // 버프 적용 
    private IEnumerator ApplyCrimsonDance()
    {
        float atkSpeedBuff = _isEvolved ? _attackSpeedBonusPostEvolve : _attackSpeedBonusPreEvolve;
        float bleedChanceBuff = _isEvolved ? _bleedChanceBonusPostEvolve : _bleedChanceBonusPreEvolve;

        MasterAudio.PlaySound(_sfxName);

        var atkSpeed = new StatModifier(StatType.AttackSpeed, 1 + atkSpeedBuff, StatModifier.ModifierMode.Multiplicative);
        _weapon.playerStat.AddModifier(atkSpeed);
        _activeModifiers.Add(atkSpeed);

        if (_weapon.playerStat is RuntimeStat runtimeStat)
            runtimeStat.TemporaryBleedChanceBonus = bleedChanceBuff;

        if (_animController != null && !string.IsNullOrEmpty(_buffAnimStr))
        {
            _animController.PlayUpperBodyAttackWithAutoReset(
                _buffAnimStr,
                _idleAnimStr,
                _delayTime  / _weapon.playerStat.AttackSpeed
            );
        }
        
        yield return new WaitForSeconds(_duration);

        RemoveCurrentBuffs();
    }

    private void RemoveCurrentBuffs()
    {
        foreach (var mod in _activeModifiers)
            _weapon?.playerStat?.RemoveModifier(mod);
        _activeModifiers.Clear();

        if (_weapon?.playerStat is RuntimeStat runtimeStat)
            runtimeStat.TemporaryBleedChanceBonus = 0f;

        _activeBuffRoutine = null;
    }

    private void OnDestroy()
    {
        if (_activeBuffRoutine != null)
            _weapon.StopCoroutine(_activeBuffRoutine);

        RemoveCurrentBuffs();
    }
}
