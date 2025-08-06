using UnityEngine;

public class SteamTurretSkill : StackableWeaponSkill
{
    // 열기를 소모하여 터렛을 생성하는 스킬

    [Header("터렛 설정")]
    [SerializeField] private float _requiredHeat = 30f;
    [SerializeField] private GameObject _turretPrefab;
    [SerializeField] private float _maxDuration = 5f;
    [SerializeField] private float _maxDamageMultiplier = 2f;
    [SerializeField] private Vector3 _offset;

    // 스탯을 사용 
    protected override bool UseStackSkill()
    {
        if (CurrentStack <= 0) return false;
        if (!(_weapon is RangedWeapon ranged)) return false;

        var controller = _owner.GetComponentInParent<PlayerController>();
        if (controller == null) return false;

        var armorHandler = controller.GetComponent<PlayerArmorHandler>();
        var steamArmor = armorHandler?.equippedArmor as SteamArmor;

        if (steamArmor == null || steamArmor.IsOverheated) return false;

        float currentHeat = steamArmor.CurrentHeat;
        if (currentHeat < _requiredHeat) return false;

        // 열기 소모
        steamArmor.ReduceHeat(_requiredHeat);

        // 데미지 & 지속시간 계산
        float ratio = Mathf.Clamp01(currentHeat / steamArmor.MaxHeat);
        float turretDuration = _maxDuration * ratio;
        float turretDamageMultiplier = 1f + (_maxDamageMultiplier - 1f) * ratio;

        // 터렛 생성 (statProvider 사용)
        SpawnTurret(
            ranged.firePoint.position + _offset,
            ranged.FacingDirection,
            turretDamageMultiplier,
            turretDuration
        );

        return true;
    }

    private void SpawnTurret(Vector3 position, int direction, float damageMultiplier, float duration)
    {
        GameObject turret = ObjectPooler.SpawnFromPool(_turretPrefab, position, Quaternion.identity);
        if (turret == null)
        {
            return;
        }

        var turretComponent = turret.GetComponent<SteamTurret>();
        if (turretComponent == null)
        {
            return;
        }

        if (_weapon.statProvider == null)
        {
            return;
        }

        turretComponent.Initialize(_weapon.statProvider, direction, damageMultiplier, duration);
    }
}
