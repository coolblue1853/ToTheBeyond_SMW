using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

public class PowerShotSkill : WeaponSkill
{
    // 데미지 & 디버프를 두여하는 탄환 스킬 

    [Header("스킬 커스터마이징")] [SerializeField] private int _ammoCost = 1;
    [SerializeField] private float _additionalSpeedMultiplier = 1.5f;
    [SerializeField] private GameObject _projectilePrefab;


    [Header("디버프 효과들")] [SerializeField] private List<DebuffEffectSO> _debuffEffects;

    [SerializeField] private string _shotAnimStr;

    public override bool Activate()
    {
        if (!(_weapon is RangedWeapon ranged)) return false;
        if (!ranged.IsReady) return false;
        if (!ranged.ConsumeAmmo(_ammoCost))
        {
            return false;
        }

        // 딜레이 시작
        StartCoroutine(DelayRoutine());
        FirePowerShot(ranged);
        ranged.PlayMuzzleVFX();
        return true;
    }


    private void FirePowerShot(RangedWeapon ranged)
    {
        var animController = ranged.playerController.GetComponent<PlayerAnimatorController>();
        if (animController != null && !string.IsNullOrEmpty(_shotAnimStr))
        {
            float attackDuration = ranged.RangedData.fireRate / ranged.playerStat.AttackSpeed;
            animController.PlayUpperBodyAttackWithAutoReset(
                _shotAnimStr,
                ranged.data.upperBodyIdleAnim,
                attackDuration
            );
        }

        GameObject bullet =
            ObjectPooler.SpawnFromPool(_projectilePrefab, ranged.firePoint.position, ranged.firePoint.rotation);
        if (!bullet) return;

        Vector3 direction = ranged.firePoint.right * ranged.FacingDirection;
        float baseSpeed = ranged.RangedData.bulletSpeedMax * ranged.playerStat.BulletSpeed;
        float finalSpeed = baseSpeed * _additionalSpeedMultiplier;

        var rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = direction.normalized * finalSpeed;

            var sr = bullet.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipX = (direction.x < 0);
            var proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                float damage = _skill.baseDamageByLevel;

                proj.SetPayload(
                    new float[] { damage, damage },
                    1.2f,
                    ranged.statProvider.CreatePayloadFrom(true),
                    isSkill: true,
                    isMelee: _isMelee
                );

                // 디버프 설정
                proj.SetDebuffs(_debuffEffects);

                proj.SetReturnReference(_projectilePrefab);
                proj.StartAutoReturn(ranged.RangedData.bulletLifetime);
            }

            //사운드 출력
            MasterAudio.PlaySound($"{ranged.RangedData.sfxWeaponName}_{_skill.skillSfxName}");

            if (_skill.vfxPrefab != null)
            {
                GameObject.Instantiate(_skill.vfxPrefab, ranged.firePoint.position, Quaternion.identity);
            }
        }
    
}
