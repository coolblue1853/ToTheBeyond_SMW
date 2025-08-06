using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class BladeOverdriveSkill : WeaponSkill
{
    // 추가 공격과 공격 속도를 올리는 버프 스킬 

    private float _duration;
    private float _attackSpeedBuff;
    private float _moveSpeedBuff;
    private GameObject _additionalAttackPrefab;

    private readonly List<StatModifier> _activeModifiers = new();
    private Coroutine _activeRoutine;

    [SerializeField] private float _attackDuration = 0.3f;
    [SerializeField] private float _attackSpeed = 10f;

    [Header("효과음")] 
    [SerializeField] private string _introSfxName;
    [SerializeField] private GameObject _buffSFX;
    [SerializeField] private float _sfxDestroyTime = 1.5f;

    // 애니메이션 관련  변수
    private PlayerAnimatorController _animController;
    [SerializeField] private string _introAnimStr;


    public override void Initialize(SkillSO skillData, Transform ownerTransform)
    {
        base.Initialize(skillData, ownerTransform);

        _duration = skillData.duration;
        _attackSpeedBuff = (skillData.buffPower.Length > 0) ? skillData.buffPower[0] : 1f;
        _moveSpeedBuff = (skillData.buffPower.Length > 1) ? skillData.buffPower[1] : 1f;
        _additionalAttackPrefab = skillData.additionalAttackPrefab;
    }

    public override bool Activate()
    {
        if (_weapon is not MeleeWeapon melee)
            return false;
        
        _animController = melee.playerController.GetComponent<PlayerAnimatorController>();
        if (_animController != null && !string.IsNullOrEmpty(_introAnimStr))
        {
            _animController.PlayUpperBodyReloadWithAutoReset(
                _introAnimStr,
                melee.data.upperBodyIdleAnim,
                _delayTime  / melee.playerStat.AttackSpeed,
                1f
            );
        }
        
        // 기존 버프가 있으면 중단
        if (_activeRoutine != null)
        {
            _weapon.StopCoroutine(_activeRoutine);
            RemoveBuffs(melee);
        }

       var vfx = Instantiate(_buffSFX,transform.position, transform.rotation);
        Destroy(vfx, _sfxDestroyTime);

        MasterAudio.PlaySound(_introSfxName);
        _activeRoutine = _weapon.StartCoroutine(ApplyOverdrive(melee));
        _weapon.StartCoroutine(DelayRoutine());
        return true;
    }

    // 버프 발동 
    private IEnumerator ApplyOverdrive(MeleeWeapon melee)
    {
        var stat = _weapon.playerStat;

        StatModifier atkSpeed = new(StatType.AttackSpeed, _attackSpeedBuff, StatModifier.ModifierMode.Multiplicative);
        StatModifier moveSpeed = new(StatType.MoveSpeed, _moveSpeedBuff, StatModifier.ModifierMode.Multiplicative);
        stat.AddModifier(atkSpeed);
        stat.AddModifier(moveSpeed);
        _activeModifiers.Add(atkSpeed);
        _activeModifiers.Add(moveSpeed);

        melee.SetAdditionalAttackVfx(_additionalAttackPrefab, _attackDuration, _attackSpeed);
        melee.IsAdditionalAttackActive = true;

        if (_skill?.vfxPrefab != null)
            Instantiate(_skill.vfxPrefab, _owner.position, Quaternion.identity);

        yield return new WaitForSeconds(_duration);

        RemoveBuffs(melee);
    }

    private void RemoveBuffs(MeleeWeapon melee)
    {
        var stat = _weapon?.playerStat;
        foreach (var mod in _activeModifiers)
            stat?.RemoveModifier(mod);
        _activeModifiers.Clear();

        melee.SetAdditionalAttackVfx(null, 0, 0);
        melee.IsAdditionalAttackActive = false;
        _activeRoutine = null;
    }

    private void OnDestroy()
    {
        if (_activeRoutine != null)
            _weapon?.StopCoroutine(_activeRoutine);

        if (_weapon is MeleeWeapon melee)
            RemoveBuffs(melee);
    }
}
