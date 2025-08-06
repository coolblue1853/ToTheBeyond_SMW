using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;
public class PressRangedWeapon : RangedWeapon
{
    // 누르고 있으면 발사되는 형태의 무기 
    public override void Attack()
    {
        base.Attack();
        if (_fireRoutine == null)
            _fireRoutine = StartCoroutine(AutoFire());
    }

    public virtual  void StopAttack()
    {
        if (_fireRoutine != null)
        {
            StopCoroutine(_fireRoutine);
            _fireRoutine = null;
        }
    }

    // 버튼 누를 시 자동으로 발사 
    protected virtual IEnumerator AutoFire()
    {
        while (true)
        {
            if (CanFire())
            {
                _lastFireTime = Time.time;
                _currentAmmo--;
                int bulletCount = Mathf.RoundToInt(RangedData.bulletsPerShot + playerStat.BulletsPerShot);
                StartCoroutine(FireBurst(bulletCount));
                

                if (_currentAmmo <= 0)
                    yield return StartCoroutine(ReloadRoutine());
                else
                    yield return new WaitForSeconds(RangedData.fireRate / playerStat.AttackSpeed);
            }
            else if (_currentAmmo <= 0 && !_isReloading)
            {
                yield return StartCoroutine(ReloadRoutine());
            }
            else
            {
                yield return null;
            }
        }
    }
}