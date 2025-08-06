using UnityEngine;
using System.Linq;
using DarkTonic.MasterAudio;

public class SniperRangedWeapon : RangedWeapon
{
    // 저격형 무기 
    [SerializeField] private GameObject _impactPrefab; // 저격 상태일때 직접 공격하는 프리팹 
    [SerializeField] private float _impactDuration = 0.5f;
    [SerializeField] private float _markedSniperDamageMultiplier = 1.5f;
    [SerializeField] private string _flybySfxName;

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
        EnemyHealth marked = FindMarkedTargetInCamera();
        if (marked != null)
        {
            _lastFireTime = Time.time;
            _currentAmmo--;
            FireSniperAtTarget(marked);

            if (_currentAmmo <= 0)
                Reload();

            return;
        }

        _lastFireTime = Time.time;
        _currentAmmo--;
        
        MasterAudio.PlaySound($"{RangedData.sfxWeaponName}_Fire");
        
        int bulletCount = Mathf.RoundToInt(RangedData.bulletsPerShot + playerStat.BulletsPerShot);
        StartCoroutine(FireBurst(bulletCount));

        if (_currentAmmo <= 0)
            Reload();
    }

    // 카메라 내의 마킹 디버프를 가진 적 반환 
    private EnemyHealth FindMarkedTargetInCamera()
    {
        var enemies = GameObject.FindObjectsOfType<EnemyHealth>();

        // 마크 + 카메라 안
        var visibleMarkedEnemies = enemies
            .Where(e =>
            {
                Vector3 viewPos = Camera.main.WorldToViewportPoint(e.transform.position);
                bool inView = viewPos.z > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1;

                var handler = e.GetComponent<EnemyDebuffHandler>();
                return inView && handler != null && handler.HasDebuff(DebuffType.MarkedTarget);
            })
            .OrderByDescending(e => e.CurrentHealth) // 체력 높은 순
            .ToList();

        return visibleMarkedEnemies.FirstOrDefault(); // 없으면 null
    }

    // 마크를 가진 적에게 직접 공격 
    private void FireSniperAtTarget(EnemyHealth target)
    {
        Vector3 hitPos = target.transform.position;

        float[] baseDamage = RangedData.weaponBaseDamage;
        float finalDamage = statProvider.CreatePayloadFrom(true).GetFinalDamage(
            baseDamage,
            RangedData.weaponDamageMultiplier * _markedSniperDamageMultiplier, // 여기 추가
            isSkill: false,
            isMelee: false,
            isCrit: true
        );

        PlayMuzzleVFX();

        MasterAudio.PlaySound($"{RangedData.sfxWeaponName}_Fire");
        if(_flybySfxName != null)
            MasterAudio.PlaySound($"{RangedData.sfxWeaponName}_Flyby");
        
        target.TakeDamage(finalDamage, true);

        if (_impactPrefab != null)
        {
            GameObject go = Instantiate(_impactPrefab, hitPos, Quaternion.identity);
            Destroy(go, _impactDuration);
        }
    }
}