using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    private MeleeWeaponDataSO meleeData => data as MeleeWeaponDataSO;

    // 공격 관련 변수 
    private int _comboIndex = 0;
    public bool isDashing = false;
    private float _lastAttackTime;
    private GameObject _additionalAttackVfx;
    public bool IsAdditionalAttackActive { get; set; } = false;

    [SerializeField] private bool _isSetParent = false;
    [SerializeField] private Transform[] _attackPivots;
    [SerializeField] private string[] _attackAnimationNames; 
    [SerializeField] private Vector2 _additionalAttackOffset;
    
    private float _duration = 0.3f;
    private float _speed = 10f;

    public void SetAdditionalAttackVfx(GameObject vfxPrefab, float duration, float speed)
    {
        _additionalAttackVfx = vfxPrefab;
        _duration = duration;
        _speed = speed;
    }

    // 추가공격이 있을 경우 
    public void TrySpawnAdditionalAttack()
    {
        if (!IsAdditionalAttackActive || _additionalAttackVfx == null) return;

        Vector2 direction = Vector2.right * FacingDirection;
        Vector3 spawnPosition = transform.position + (Vector3)(new Vector2(_additionalAttackOffset.x * FacingDirection, _additionalAttackOffset.y));

        GameObject slash = ObjectPooler.SpawnFromPool(_additionalAttackVfx, spawnPosition, Quaternion.identity);

        // 회전 제거
        slash.transform.rotation = Quaternion.identity;

        var proj = slash.GetComponent<Projectile>();
        if (proj != null)
        {
            var payload = statProvider.CreatePayloadFrom(); 
            payload.isPiercing = true;

            proj.SetPayload(data.weaponBaseDamage, data.weaponDamageMultiplier, payload, isSkill: true, isMelee: true);
            proj.SetReturnReference(_additionalAttackVfx);
            proj.StartAutoReturn(_duration);

            var rb = slash.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction.normalized * _speed;
            }

            var sr = slash.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipX = (FacingDirection < 0);
        }
    }

    // 공격 활성화 
    public override void Attack()
    {
        if (!CanAttack() || meleeData.comboSequence.Count == 0) return;

        var entry = meleeData.comboSequence[_comboIndex];

        float comboAllowTime = entry.delay + meleeData.ComboResetDelay; // 다음 연속공격으로 넘어갈 수 있는지 
        if (Time.time - _lastAttackTime > comboAllowTime)
            _comboIndex = 0;

        entry = meleeData.comboSequence[_comboIndex];

        _lastAttackTime = Time.time;
        isUsingSkill = true;
        isMovementLocked = entry.lockMovement;

        if (animator != null && // 애니메이션 출력 
            entry.pivotIndex >= 0 &&
            entry.pivotIndex < _attackAnimationNames.Length)
        {
            string animName = _attackAnimationNames[entry.pivotIndex];
            animator.PlayUpperBodyAttackWithAutoReset(
                animName,
                data.upperBodyIdleAnim,
                entry.delay / playerStat.AttackSpeed
            );
        }

        MasterAudio.PlaySound($"{meleeData.sfxWeaponName}_Attack");
        StartCoroutine(AttackRoutine(entry));
        _comboIndex = (_comboIndex + 1) % meleeData.comboSequence.Count;
    }

    // 공격 루, 데미지 프리팹 생성 및 데미지 페이로드 주입 
    private IEnumerator AttackRoutine(MeleeWeaponDataSO.MeleeAttackEntry entry)
    {
        float spawnDelay = entry.attackSpawnDelay / playerStat.AttackSpeed;
        yield return new WaitForSeconds(spawnDelay);

        Transform pivot = (entry.pivotIndex >= 0 && entry.pivotIndex < _attackPivots.Length) ? _attackPivots[entry.pivotIndex] : transform;
        Vector3 spawnPosition = pivot.position;

        GameObject instance;
        if (_isSetParent && _ownerTransform != null)
        {
            instance = ObjectPooler.SpawnFromPool(entry.attackPrefab, Vector3.zero, Quaternion.identity, _ownerTransform);
            instance.transform.localPosition = pivot.localPosition;
        }
        else
        {
            instance = ObjectPooler.SpawnFromPool(entry.attackPrefab, spawnPosition, Quaternion.identity);
        }

        TrySpawnAdditionalAttack(); 

        var trigger = instance.GetComponent<MeleeAttackTrigger>();
        if (trigger != null)
        {
            trigger.SetOwner(this);
            trigger.SetReturnReference(entry.attackPrefab);

            var payload = statProvider.CreatePayloadFrom();
            trigger.SetPayload(data.weaponBaseDamage, data.weaponDamageMultiplier, payload, isSkill: false, isMelee: true);
            trigger.SetDebuffs(data.basicAttackDebuffs);
            trigger.SetAutoReturn(entry.lifeTime / playerStat.AttackSpeed);
        }

        float comboEndDelay = (entry.delay - entry.attackSpawnDelay) / playerStat.AttackSpeed;
        if (comboEndDelay > 0f)
            yield return new WaitForSeconds(comboEndDelay);

        isUsingSkill = false;
        isMovementLocked = false;
    }
}
