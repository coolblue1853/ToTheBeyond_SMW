using UnityEngine;
public class StandardRangedWeapon : RangedWeapon
{
    // 기본 원거리 무기

    public override void Attack()
    {
        if (!CanAttack()) return;
        if (!CanFire()) return;
        
        if (!string.IsNullOrEmpty(RangedData.upperBodyAttackAnim))
        {
            float attackInterval = RangedData.fireRate / playerStat.AttackSpeed;
            playerController.GetComponent<PlayerAnimatorController>()
                ?.PlayUpperBodyAttackWithAutoReset(RangedData.upperBodyAttackAnim, data.upperBodyIdleAnim, attackInterval);
        }

        _lastFireTime = Time.time;
        _currentAmmo--;
        int bulletCount = Mathf.RoundToInt(RangedData.bulletsPerShot + playerStat.BulletsPerShot);
        StartCoroutine(FireBurst(bulletCount));

        if (_currentAmmo <= 0)
            Reload();
    }
}
