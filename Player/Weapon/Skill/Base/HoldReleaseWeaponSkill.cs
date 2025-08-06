using System.Linq;
using UnityEngine;

public abstract class HoldReleaseWeaponSkill : WeaponSkill
{
    // 홀드 - 릴리즈로 발동되는 스킬들 

    protected bool _canExecute = false;
    private bool _hasExecuted = false;
    public bool movementLocked = true;
    private SkillSlot? _cachedSlot;

    public override void Initialize(SkillSO skillData, Transform ownerTransform)
    {
        base.Initialize(skillData, ownerTransform);
        _cachedSlot = FindMySlotFromData();
    }

    // 스킬을 눌렀을때 입력 
    public override void OnSkillHold()
    {
        if (!CanBeginSkill())
        {
            _canExecute = false;
            return;
        }

        _canExecute = true;
        _hasExecuted = false;
        _weapon.isUsingSkill = true;
        _weapon.isMovementLocked = movementLocked;
        BeginSkill();
    }

    // 스킬을 땠을때 입력 
    public override void OnSkillRelease()
    {
        if (!_canExecute) return;

        _weapon.isUsingSkill = false;
        _weapon.isMovementLocked = false;

        EndSkill();

        if (_hasExecuted && _cachedSlot.HasValue)
        {
            _weapon.RegisterSkillCooldown(_cachedSlot.Value, _skill.cooldown);
        }

        _canExecute = false;
    }

    private void Update()
    {
        if (_canExecute && !_hasExecuted)
        {
            UpdateSkill(); // 에임 갱신
        }
    }

    protected void MarkExecuted() => _hasExecuted = true;

    protected virtual bool CanBeginSkill()
    {
        if (_cachedSlot.HasValue && _weapon.IsSkillOnCooldown(_cachedSlot.Value, _skill.cooldown))
        {
            Debug.Log($"쿨타임 중: {_skill.skillName} 스킬 사용 불가");
            return false;
        }

        return true;
    }

    private SkillSlot? FindMySlotFromData()
    {
        return _weapon.data.skills
            .Where(s => s.Skill == _skill)
            .Select(s => (SkillSlot?)s.Slot)
            .FirstOrDefault();
    }

    protected abstract void BeginSkill();
    protected abstract void EndSkill();
    protected virtual void UpdateSkill() { }
}