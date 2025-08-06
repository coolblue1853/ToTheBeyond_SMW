using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;
public class SteamMinigun : PressRangedWeapon
{
    // 프레스 무기 + 열기를 사용하는 스팀 무기 

    [SerializeField] private float _heatPerTick = 5f;
    [SerializeField] private float _heatTickInterval = 0.5f; // 몇 초마다 열기 상승

    private SteamArmor _steamArmor;
    private Coroutine _heatCoroutine;

    public override void Equip(IDamageStatProvider statProvider, RuntimeStat runtimeStat,  Transform ownerTransform)
    {
        base.Equip(statProvider, runtimeStat,ownerTransform);

        var controller = GetComponentInParent<PlayerController>();
        if (controller != null)
        {
            var armorHandler = controller.GetComponent<PlayerArmorHandler>();
            _steamArmor = armorHandler?.equippedArmor as SteamArmor;
        }
    }

    protected override bool CanFire()
    {
        return base.CanFire() && (_steamArmor == null || !_steamArmor.IsOverheated);
    }
    
    protected override IEnumerator AutoFire()
    {
        float heatTimer = 0f;

        while (true)
        {
            if (CanFire())
            {
                _lastFireTime = Time.time;
                _currentAmmo--;

                MasterAudio.PlaySound($"{RangedData.sfxWeaponName}_Fire");
                int bulletCount = Mathf.RoundToInt(RangedData.bulletsPerShot + playerStat.BulletsPerShot);
                StartCoroutine(FireBurst(bulletCount));

                // 열기 누적 로직 (시간 기준)
                heatTimer += RangedData.fireRate / playerStat.AttackSpeed;
                if (_steamArmor != null && !_steamArmor.IsOverheated && heatTimer >= _heatTickInterval)
                {
                    _steamArmor.AddHeat(_heatPerTick);
                    heatTimer = 0f;
                }

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


    public override void StopAttack()
    {
        base.StopAttack();

        // 열기 누적 코루틴 종료
        if (_heatCoroutine != null)
        {
            StopCoroutine(_heatCoroutine);
            _heatCoroutine = null;
        }
    }

}
