using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine;

public class ChargeShotSkill : HoldReleaseWeaponSkill
{
    // 충전해서 발사하는 스킬 관통시 데미지 감소 기능 추가 

    [Header("설정")]
    [SerializeField] private GameObject _projectilePrefab;

    [SerializeField] private float _maxChargeTime = 2f;
    [SerializeField] private AnimationCurve _chargeCurve = AnimationCurve.Linear(0, 0.5f, 1f, 1.5f);
    [SerializeField] private bool _ignoreDamageFalloff = false;

    private float _chargeTimer;

    
    [Header("효과음")] 
    [SerializeField] private string _introSfxName ;
    [SerializeField] private string _outroSfxName ;
    
    // 애니메이션
    private PlayerAnimatorController _animController;
    [SerializeField] private string _introAnimStr;
    [SerializeField] private string _attackAnimStr;

    public override bool Activate()
    {
        return false; // Hold 스킬이므로 즉발 없음
    }

    protected override void BeginSkill()
    {
        if (!(_weapon is RangedWeapon ranged)) return;

        if (!ranged.IsReady || ranged.CurrentAmmo < 1)
        {
            _weapon.isUsingSkill = false;       
            _weapon.isMovementLocked = false;     
            return;
        }
        
        _animController = ranged.playerController.GetComponent<PlayerAnimatorController>();
        if (_animController != null && !string.IsNullOrEmpty(_introAnimStr))
        {
            _animController.PlayUpperBodyAnimation(_introAnimStr);
        }

        ranged.animator.isActing = true;


        MasterAudio.PlaySound(_introSfxName);
        _chargeTimer = 0f;
        StartCoroutine(ChargeRoutine());
    }

    protected override void EndSkill()
    {
        StopAllCoroutines();

        if (!(_weapon is RangedWeapon ranged)) return;
        if (!ranged.ConsumeAmmo(1)) return;

        float chargeRatio = Mathf.Clamp01(_chargeTimer / _maxChargeTime);
        float multiplier = _chargeCurve.Evaluate(chargeRatio);

        FireChargedShot(ranged, multiplier);
        
        
        if (_animController != null && !string.IsNullOrEmpty(_attackAnimStr))
        {
            _animController.PlayUpperBodyAttackWithAutoReset(
                _attackAnimStr,
                _weapon.data.upperBodyIdleAnim,
                _delayTime
            );
        }
        
        StartCoroutine(DelayRoutine());
   
    }

    // 충전 중 
    private IEnumerator ChargeRoutine()
    {
        while (_chargeTimer < _maxChargeTime)
        {
            _chargeTimer += Time.deltaTime;
            yield return null;
        }
    }

    // 충전 완료 이후 탄환 발사 
    private void FireChargedShot(RangedWeapon ranged, float multiplier)
    {
        GameObject bullet = ObjectPooler.SpawnFromPool(_projectilePrefab, ranged.firePoint.position, ranged.firePoint.rotation);
        if (!bullet) return;

        Vector3 dir = ranged.firePoint.right * _weapon.FacingDirection;

        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            float baseSpeed = ranged.RangedData.bulletSpeedMax * ranged.playerStat.BulletSpeed;
            float chargedSpeed = baseSpeed * multiplier; // 충전량에 비례한 속도 증가
            rb.velocity = dir.normalized * chargedSpeed;
        }

        if (bullet.TryGetComponent<SpriteRenderer>(out var sr))
            sr.flipX = (dir.x < 0);

        if (bullet.TryGetComponent<Projectile>(out var proj))
        {
            proj.SetPayload(
                new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel },
                multiplier, // 데미지 배율
                ranged.statProvider.CreatePayloadFrom(isPiercing: true),
                isSkill: true,
                isMelee: _isMelee
            );

            proj.SetReturnReference(_projectilePrefab);
            proj.StartAutoReturn(ranged.RangedData.bulletLifetime);
            proj.SetFalloffDamage(!_ignoreDamageFalloff); // 진화 여부에 따라 적용
        }

        ranged.PlayMuzzleVFX();
        if (_skill.vfxPrefab != null)
            Instantiate(_skill.vfxPrefab, ranged.firePoint.position, Quaternion.identity);

        MasterAudio.PlaySound(_outroSfxName);
        ranged.animator.isActing = false;
        MarkExecuted();
    }
}
