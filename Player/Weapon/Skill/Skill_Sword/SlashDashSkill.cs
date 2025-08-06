using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine;

public class SlashDashSkill : WeaponSkill
{
    // 대쉬 공격, 진화시 추가 데미지 구역을 생성 

    [SerializeField] private float _dashDistance = 5f;
    [SerializeField] private float _dashSpeed = 30f;
    [SerializeField] private GameObject _dashHitboxPrefab;
    [SerializeField] private float _hitboxLifetime = 0.3f;
    [SerializeField] private GameObject _damageZonePrefab;
    [SerializeField] private bool _isEvolved = false;
    [SerializeField] private float _zoneDuration = 1f;
    [SerializeField] private float _zoneDelay = 0;
    [SerializeField] private Vector2 _damageZoneOffset = new Vector2(0f, -0.5f);
    [SerializeField] private int _attackCount = 1;
    
    [Header("효과음")] 
    [SerializeField] private string _introSfxName;

    // 애니메이션 관련 변수 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _introAnimStr;
    [SerializeField] private string _IdleAnimStr;
    [SerializeField] private float _playSpeed = 1f;

    public override bool Activate()
    {
        if (_weapon is MeleeWeapon melee && !melee.isDashing)
        {
            _weapon.StartCoroutine(PerformDash(melee));
            return true;
        }
        return false;
    }

    // 대쉬 시작 
    private IEnumerator PerformDash(MeleeWeapon melee)
    {
        melee.isDashing = true;
        _weapon.isUsingSkill = true;
        _weapon.isMovementLocked = true;

        _animController = melee.playerController.GetComponent<PlayerAnimatorController>();
        if (_animController != null)
        {
            _animController.PlayUpperBodyAnimation(_introAnimStr, _delayTime * _playSpeed);
        }

        Transform root = _owner.root;
        Rigidbody2D rb = root.GetComponent<Rigidbody2D>();

        melee.playerController.gravityCtrl.Suppress(); // 중력 제거

        rb.velocity = Vector2.zero;
        Vector2 dir = _owner.right * _weapon.FacingDirection;
        float traveled = 0f;

        MasterAudio.PlaySound(_introSfxName);

        if (_dashHitboxPrefab != null)
        {
            Vector3 spawnPos = root.position + (Vector3)(dir * 0.5f);
            GameObject hitbox = ObjectPooler.SpawnFromPool(_dashHitboxPrefab, spawnPos, Quaternion.identity);
            hitbox.transform.SetParent(root);

            // Sprite 방향 반전
            if (hitbox.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.flipX = _weapon.FacingDirection == -1;
            }

            // 전체 스케일 반전
            Vector3 hitboxScale = hitbox.transform.localScale;
            hitboxScale.x = Mathf.Abs(hitboxScale.x) * _weapon.FacingDirection;
            hitbox.transform.localScale = hitboxScale;

            if (hitbox.TryGetComponent<MeleeAttackTrigger>(out var trigger))
            {
                trigger.SetOwner(_weapon);
                trigger.SetReturnReference(_dashHitboxPrefab);
                var payload = _weapon.statProvider.CreatePayloadFrom();
                trigger.SetPayload(_weapon.data.weaponBaseDamage, _weapon.data.weaponDamageMultiplier, payload, isSkill: false, isMelee: true);
                trigger.SetAutoReturn(_hitboxLifetime);

                if(_attackCount != 1)
                    trigger.SetMultiHit(_attackCount, 0.1f);
            }

        }

        float startTime = Time.time;
        float targetDistance = _dashDistance;

        while (traveled < _dashDistance)
        {
            float delta = _dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + dir * delta);
            traveled += delta;
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(SpawnPrefabs(root, dir));

        melee.playerController.gravityCtrl.Restore(); // 중력 복원

        melee.isDashing = false;

        if (_animController != null && !string.IsNullOrEmpty(_IdleAnimStr))
        {
            _animController.StartResetRoutine(_IdleAnimStr, _delayTime);
        }

        if (_delayTime > 0)
        {
            StartCoroutine(DelayRoutine());
        }
        else
        {
            _weapon.isUsingSkill = false;
            _weapon.isMovementLocked = false;
        }

    }

    // 공격 프리팹 생성 
    IEnumerator SpawnPrefabs(Transform root, Vector2 dir)
    {
        yield return new WaitForSeconds(_zoneDelay);
        
        if (_isEvolved && _damageZonePrefab != null)
        {
            Vector3 offsetPos = root.position + new Vector3(_damageZoneOffset.x * _weapon.FacingDirection, _damageZoneOffset.y, 0f);
            GameObject go = Instantiate(_damageZonePrefab, offsetPos, Quaternion.identity);

            Vector3 zoneScale = go.transform.localScale;
            zoneScale.x = Mathf.Abs(zoneScale.x) * _weapon.FacingDirection;
            go.transform.localScale = zoneScale;

            var dz = go.GetComponent<DamageZoneEffect>();
            var instantEffect = go.GetComponent<InstantDamageEffect>();
            if (dz != null)
            {
                dz.transform.parent = null;
                dz.SetPayload(
                    _weapon.data.weaponBaseDamage,
                    _weapon.data.weaponDamageMultiplier,
                    _weapon.statProvider.CreatePayloadFrom(),
                    isSkill: true,
                    isMelee: true,
                    isEvolved: true,
                    direction: _weapon.FacingDirection,
                    duration: _zoneDuration
                );
            }
            else if (instantEffect != null)
            {
                instantEffect.SetPayload(
                    _weapon.data.weaponBaseDamage,
                    _weapon.data.weaponDamageMultiplier,
                    _weapon.statProvider.CreatePayloadFrom(),
                    isSkill: true,
                    isMelee: true,
                    desroyTime: _zoneDuration
                );
                
                foreach (var effect in go.GetComponents<IOnHitEffect>())
                {
                    instantEffect.RegisterOnHitEffect(effect);
                }
            } 
        }
    }

}
