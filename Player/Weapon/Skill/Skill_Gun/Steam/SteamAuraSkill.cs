using UnityEngine;
using DarkTonic.MasterAudio;
public class SteamAuraSkill : WeaponSkill
{
    // 열기를 소모하여 증기를 분출하는 스킬 

    [Header("스팀 오라 설정")]
    [SerializeField] private float _requiredHeat = 30f;
    [SerializeField] private GameObject _auraEffectPrefab;
    [SerializeField] private float _maxDuration = 5f;
    [SerializeField] private float _maxDamageMultiplier = 2f;
    [SerializeField] private bool _isEvolved = false;

    // 효과음 관련 변수 
    [SerializeField] private string _steamSfxName;


    // 발동시 데미지존 생성 
    public override bool Activate()
    {
        if (!(_weapon is RangedWeapon ranged)) return false;

        var controller = _owner.GetComponentInParent<PlayerController>();
        if (controller == null) return false;

        var armorHandler = controller.GetComponent<PlayerArmorHandler>();
        var steamArmor = armorHandler?.equippedArmor as SteamArmor;

        if (steamArmor == null || steamArmor.IsOverheated) return false;
        float currentHeat = steamArmor.CurrentHeat;
        if (currentHeat < _requiredHeat) return false;

        steamArmor.ReduceHeat(_requiredHeat);

        float ratio = Mathf.Clamp01(currentHeat / steamArmor.MaxHeat);
        float duration = _maxDuration * ratio;
        float damageMultiplier = 1f + (_maxDamageMultiplier - 1f) * ratio;
        float baseDamage = _skill.baseDamageByLevel;
 
        MasterAudio.PlaySound(_steamSfxName);
        
        SpawnAuraEffect(_owner.position, ranged.FacingDirection, baseDamage, damageMultiplier, duration, _isEvolved);
        return true;
    }

    // 데미지존 페이로드 주입 
    private void SpawnAuraEffect(Vector3 position, int direction, float baseDamage, float damageMultiplier, float duration, bool evolved)
    {
        GameObject aura = ObjectPooler.SpawnFromPool(_auraEffectPrefab, position, Quaternion.identity);
        if (aura == null)
        {
            return;
        }

        var auraComponent = aura.GetComponent<DamageZoneEffect>();
        if (auraComponent == null)
        {
            return;
        }

        if (_weapon.statProvider == null)
        {
            return;
        }

        if (!evolved)
        {
            aura.transform.SetParent(_owner);
            aura.transform.localPosition = Vector3.zero;
        }

        auraComponent.SetPayload(
            new float[] { baseDamage, baseDamage },
            damageMultiplier,
            _weapon.statProvider.CreatePayloadFrom(true),
            isSkill: true,
            isMelee: true,
            evolved,
            direction,
            duration
        );
    }
}
