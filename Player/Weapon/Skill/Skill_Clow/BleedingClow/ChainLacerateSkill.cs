using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;

public class ChainLacerateSkill : WeaponSkill
{
    // 적에게 돌진하여 연속공격하는 스킬 

    [Header("스킬 설정")]
    [SerializeField] private float _jumpSpeed = 25f;
    [SerializeField] private float _attackDelay = 0.1f;
    [SerializeField] private int _hitCount = 6;
    [SerializeField] private float _damageMultiplierOnBleed = 1.3f;
    [SerializeField] private float _explodeMultiplier;
    [SerializeField] private bool _isEvolved;
    [SerializeField] private GameObject _vfxSlash;
    [SerializeField] private GameObject _vfxExplosion;
    [SerializeField] private string _slashSfx = "Slash";
    private Transform _playerTransform;

    // 애니메이션 관련 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _rightAttackAnimStr;
    [SerializeField] private string _leftAttackAnimStr;
    [SerializeField] private string _idleAnimStr;

    // 사운드 관련 
    [SerializeField] private GameObject _rightSFX;
    [SerializeField] private GameObject _leftSFX;
    [SerializeField] private float _sfxDestroyTime = 1.5f;

    public override bool Activate()
    {
        var target = FindClosestEnemyInCamera();
        if (target == null) return false;

        StartCoroutine(ExecuteChainLacerate(target));

        if (!(_weapon is MeleeWeapon melee)) return false;
        _animController = melee.playerController.GetComponent<PlayerAnimatorController>();

        return true;
    }

    // 돌진 관련 처리
    private IEnumerator ExecuteChainLacerate(Transform target)
    {
        _weapon.isUsingSkill = true;
        _weapon.isMovementLocked = true;

        _playerTransform = _owner.root.GetComponent<Transform>();
        Vector3 direction = (target.position - _playerTransform.position).normalized;

        if (direction.x != 0)
        {
            var playerController = _playerTransform.GetComponent<PlayerController>();
            playerController.movement.FacingDirection = direction.x > 0 ? 1 : -1;
        }

        // 순간이동 처리
        if (target == null) yield break;

        _playerTransform.position = target.position;

        var rb = _playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;

        yield return null;

        var enemyHealth = target.GetComponent<EnemyHealth>();
        var enemyDebuff = target.GetComponent<EnemyDebuffHandler>();

        if (enemyHealth == null || enemyHealth.CurrentHealth <= 0f)
        {
            yield break; // 적이 이미 죽었으면 중단
        }

        bool isBleeding = enemyDebuff != null && enemyDebuff.HasDebuff(DebuffType.Bleed);

        for (int i = 0; i < _hitCount; i++)
        {
            if (enemyHealth == null || enemyHealth.CurrentHealth <= 0f)
                break; // 루프 중간에서도 죽었는지 확인

            if (i % 2 == 0)
            {
                var vfx = Instantiate(_rightSFX, transform.position, Quaternion.identity);
                Destroy(vfx, _sfxDestroyTime);
                if (_animController != null && !string.IsNullOrEmpty(_rightAttackAnimStr))
                {
                    _animController.PlayUpperBodyAttack(
                        _rightAttackAnimStr,
                        _attackDelay / _weapon.playerStat.AttackSpeed
                    );
                }
            }
            else
            {
                var vfx = Instantiate(_leftSFX, transform.position, Quaternion.identity);
                Destroy(vfx, _sfxDestroyTime);
                if (_animController != null && !string.IsNullOrEmpty(_leftAttackAnimStr))
                {
                    _animController.PlayUpperBodyAttack(
                        _leftAttackAnimStr,
                        _attackDelay / _weapon.playerStat.AttackSpeed
                    );
                }
            }

            float[] weaponBase = new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel };
            var payload = _weapon.statProvider.CreatePayloadFrom(true);
            float damage = payload.GetFinalDamage(
                weaponBase,
                1f,
                isSkill: true,
                isMelee: true,
                isCrit: false
            );

            if (isBleeding)
                damage *= _damageMultiplierOnBleed;

            enemyHealth.TakeDamage(damage, false);
            MasterAudio.PlaySound(_slashSfx);

            if (_vfxSlash != null)
                Instantiate(_vfxSlash, target.position, Quaternion.identity);

            yield return new WaitForSeconds(_attackDelay);
        }

        if (_isEvolved && isBleeding && enemyHealth != null && enemyHealth.CurrentHealth > 0f) // 진화상태라면 출혈시 폭팔 데미지 
        {
            int bleedStack = enemyDebuff.GetDebuffStack(DebuffType.Bleed);
            float bleedDamage = _skill.baseDamageByLevel * (1 + (_explodeMultiplier * bleedStack));
            Debug.Log(bleedDamage);

            enemyHealth.TakeDamage(bleedDamage, false);
            enemyDebuff.RemoveDebuff(DebuffType.Bleed);
        }

        if (_animController != null)
        {
            _animController.PlayUpperBodyAnimation(
                _idleAnimStr,
                _delayTime
            );
        }

        StartCoroutine(DelayRoutine());
    }

    // 출혈 여부 포함 및 가장 가까운 적 반환 
    private Transform FindClosestEnemyInCamera()
    {
        Camera cam = Camera.main;
        Vector3 camMin = cam.ViewportToWorldPoint(Vector3.zero);
        Vector3 camMax = cam.ViewportToWorldPoint(Vector3.one);

        Collider2D[] hits = Physics2D.OverlapAreaAll(camMin, camMax);
        Transform closestBleeding = null;
        Transform closestAny = null;
        float closestBleedingDist = Mathf.Infinity;
        float closestAnyDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                if (enemyHealth.CurrentHealth <= 0f)
                    continue;

                Transform enemyTransform = enemyHealth.transform;
                float dist = Vector3.Distance(_owner.position, enemyTransform.position);

                if (hit.TryGetComponent<EnemyDebuffHandler>(out var debuffHandler) &&
                    debuffHandler.HasDebuff(DebuffType.Bleed))
                {
                    if (dist < closestBleedingDist)
                    {
                        closestBleeding = enemyTransform;
                        closestBleedingDist = dist;
                    }
                }

                if (dist < closestAnyDist)
                {
                    closestAny = enemyTransform;
                    closestAnyDist = dist;
                }
            }
        }

        return closestBleeding != null ? closestBleeding : closestAny;
    }
}
