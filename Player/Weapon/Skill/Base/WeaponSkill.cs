using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;

public abstract class WeaponSkill : MonoBehaviour
{
    // 무기 스킬 추상 클래스 
    protected SkillSO _skill;
    protected Weapon _weapon;
    protected Transform _owner;

    [SerializeField] protected bool _isMelee;
    [SerializeField] protected float _delayTime = 1.0f;

    public virtual void Initialize(SkillSO skillData, Transform ownerTransform)
    {
        _skill = skillData;
        _owner = ownerTransform;
        _weapon = ownerTransform.GetComponent<Weapon>();
    }

    public SkillSO GetSkillData() => _skill;

    public abstract bool Activate();           // 즉발형

    public virtual void OnSkillHold() { }      // 홀드 시작
    public virtual void OnSkillRelease() { }   // 홀드 끝

    protected IEnumerator DelayRoutine()
    {
        _weapon.isUsingSkill = true;
        yield return new WaitForSeconds(_delayTime);
        _weapon.isUsingSkill = false;
        _weapon.isMovementLocked = false;
        
        if (_weapon is MeleeWeapon melee)
            melee.isDashing = false;
    }
}