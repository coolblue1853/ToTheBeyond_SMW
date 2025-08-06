using System.Collections;
using UnityEngine;

public class ChargeRangedWeapon : RangedWeapon
{
    // 충전 하는 형태의 무기

    public override void Attack()
    {
        base.Attack();
        if (!CanFire()) return;
        if (_fireRoutine != null) StopCoroutine(_fireRoutine);
        _fireRoutine = StartCoroutine(Charge());
    }

    public void ReleaseCharge()
    {
        if (!_isCharging) return;
        _isCharging = false;
        if (_fireRoutine != null) StopCoroutine(_fireRoutine);
        _fireRoutine = StartCoroutine(FireChargedShot());
    }

    private IEnumerator Charge()
    {
        _chargeTime = 0f;
        _isCharging = true;
        while (_isCharging && _chargeTime < RangedData.maxChargeTime)
        {
            _chargeTime += Time.deltaTime;
            yield return null;
        }
        _fireRoutine = null;
    }

    // 충전 이후 발사 
    private IEnumerator FireChargedShot()
    {
        int bulletCount = Mathf.Clamp(
            Mathf.RoundToInt(RangedData.chargeCurve.Evaluate(_chargeTime / RangedData.maxChargeTime) * RangedData.bulletsPerShot),
            1, RangedData.bulletsPerShot);

        _currentAmmo--;
        yield return StartCoroutine(FireBurst(bulletCount));
        if (_currentAmmo <= 0) Reload();
    }
}