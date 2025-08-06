using UnityEngine;
using DarkTonic.MasterAudio;

public class SpawnDamageZoneSkill : WeaponSkill
{
    // 주기마다 데미지를 주는 데미지존 생성 스킬 

    [Header("데미지존 생성 설정")]
    [SerializeField] private GameObject _zonePrefab;
    [SerializeField] private Vector2 _spawnOffset = Vector2.zero;
    [SerializeField] private float _zoneDuration = 5f;
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private bool _isMoving = false;
    
    [Header("사운드")]
    [SerializeField] private string _spawnSfxName;

    public override bool Activate()
    {
        if (_zonePrefab == null || _weapon.statProvider == null)
        {
            return false;
        }

        MasterAudio.PlaySound(_spawnSfxName);
        SpawnZone();
        return true;
    }

    private void SpawnZone()
    {
        Vector3 offsetPos = _owner.position + new Vector3(_spawnOffset.x * _weapon.FacingDirection, _spawnOffset.y, 0f);
        GameObject zone = ObjectPooler.SpawnFromPool(_zonePrefab, offsetPos, Quaternion.identity);

        if (zone == null)
        {
            return;
        }

        // 좌우 방향 반영
        Vector3 scale = zone.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _weapon.FacingDirection;
        zone.transform.localScale = scale;

        if (zone.TryGetComponent<DamageZoneEffect>(out var zoneEffect))
        {
            zoneEffect.SetPayload(
                weaponBaseDamage: new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel },
                weaponMultiplier: _damageMultiplier,
                payload: _weapon.statProvider.CreatePayloadFrom(true),
                isSkill: true,
                isMelee: true,
                isEvolved : _isMoving,
                direction: _weapon.FacingDirection,
                duration: _zoneDuration
            );
        }
    }
}