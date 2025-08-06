using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

public class BarrageShotSkill : WeaponSkill
{
    // 남은 탄환을 모두 발사하는 스킬 

    [Header("스킬 커스터마이징")]
    [SerializeField] private float _fireInterval = 0.1f;
    [SerializeField] private float _bulletSpeed = 15f;
    [SerializeField] private float _spreadAngle = 10f;
    [SerializeField] private bool _useFixedSpread = false;
    [SerializeField] private bool _reloadBeforeFire = false;     // 재장전효과

    [Header("발사체 설정")]
    [SerializeField] private GameObject _projectilePrefab;

    [Header("디버프 효과")]
    [SerializeField] private List<DebuffEffectSO> _debuffEffects;

    [SerializeField] private string _shotAnimStr;

    public override bool Activate()
    {
        if (!(_weapon is RangedWeapon ranged)) return false;

        if (_reloadBeforeFire)
        {
            ranged.ForceReloadToMax(); // 무조건 탄환을 채움
        }
        else
        {
            if (!ranged.IsReady) return false; // 일반적인 조건 검사
        }

        int currentAmmo = ranged.CurrentAmmo;
        if (currentAmmo <= 0) return false;
        _weapon.isUsingSkill = true;
        _weapon.isMovementLocked = true;
        ranged.StartCoroutine(FireAllShots(ranged, currentAmmo));
        return true;
    }

    // 잔여 탄환 모두 발사 스킬 
    private IEnumerator FireAllShots(RangedWeapon ranged, int shotCount)
    {
        for (int i = 0; i < shotCount; i++)
        {
            if (!ranged.ConsumeAmmo(1)) break;

            GameObject bullet = ObjectPooler.SpawnFromPool(_projectilePrefab, ranged.firePoint.position, ranged.firePoint.rotation);
            if (!bullet) continue;

            float angleOffset = _useFixedSpread
                ? -_spreadAngle / 2f + (_spreadAngle / Mathf.Max(1, shotCount - 1)) * i
                : Random.Range(-_spreadAngle / 2f, _spreadAngle / 2f);

    

            var animController = ranged.playerController.GetComponent<PlayerAnimatorController>(); // 공격 애니메이션 출력 
            if (animController != null && !string.IsNullOrEmpty(_shotAnimStr))
            {
                animController.PlayUpperBodyAttackWithAutoReset(
                    _shotAnimStr,
                    ranged.data.upperBodyIdleAnim,
                    _fireInterval
                );
            }
            ranged.PlayMuzzleVFX();
            Vector3 dir = Quaternion.Euler(0, 0, angleOffset) * (ranged.firePoint.right * ranged.FacingDirection);
        
            var sr = bullet.GetComponent<SpriteRenderer>(); // 탄환 방향 전환 
            if (sr != null)
                sr.flipX = (dir.x < 0);

            var rb = bullet.GetComponent<Rigidbody2D>();  // 탄환 속도 지정 
            if (rb != null) rb.velocity = dir.normalized * _bulletSpeed;

            var proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetPayload(
                    new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel },
                    1f,
                    ranged.statProvider.CreatePayloadFrom(true),
                    isSkill: true,
                    isMelee: _isMelee
                );

                proj.SetDebuffs(_debuffEffects);
                proj.SetReturnReference(_projectilePrefab);
                proj.StartAutoReturn(ranged.RangedData.bulletLifetime);
            }

            MasterAudio.PlaySound($"{ranged.RangedData.sfxWeaponName}_{_skill.skillSfxName}");
            
            if (_skill.vfxPrefab != null)
                GameObject.Instantiate(_skill.vfxPrefab, ranged.firePoint.position, Quaternion.identity);

            yield return new WaitForSeconds(_fireInterval);
        }
        _weapon.isMovementLocked = false;
        // 딜레이 시작
        StartCoroutine(DelayRoutine());
    }
}
