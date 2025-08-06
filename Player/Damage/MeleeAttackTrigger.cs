using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class MeleeAttackTrigger : MonoBehaviour
{
    // 물리 기본 공격 트리거 
    private DamagePayload _payload;
    private bool _isSkill;
    private bool _isMelee;
    private float _weaponMultiplier;
    private float[] _weaponBaseDamage;
    private Weapon _owner;
    private GameObject _prefabToReturn;
    private Coroutine _returnCoroutine;

    [SerializeField] private List<DebuffEffectSO> _debuffSOs;
    private HashSet<DamageablePart> _hitParts = new();
    private float _lifetime = 0.3f;

    [SerializeField] private string _hitSfxName;

    [Header("멀티 히트 설정")]
    [SerializeField] private int _multiHitCount = 1;
    [SerializeField] private float _multiHitDelay = 0.05f;

    [Header("디버프 적용 설정")]
    [SerializeField] private bool _applyDebuffEachHit = false;

    private void OnEnable()
    {
        _hitParts.Clear();
    }

    public void SetPayload(float[] weaponBaseDamage, float weaponMultiplier, DamagePayload payload, bool isSkill = false, bool isMelee = false)
    {
        _payload = payload;
        _isSkill = isSkill;
        _isMelee = isMelee;
        _weaponMultiplier = weaponMultiplier;
        _weaponBaseDamage = weaponBaseDamage;

    }

    public void SetOwner(Weapon owner) => _owner = owner;
    public void SetReturnReference(GameObject prefab) => _prefabToReturn = prefab;
    public void SetDebuffs(List<DebuffEffectSO> debuffs) => _debuffSOs = debuffs;

    public void SetMultiHit(int count, float delay)
    {
        _multiHitCount = Mathf.Max(1, count);
        _multiHitDelay = Mathf.Max(0f, delay);
    }

    public void SetApplyDebuffEachHit(bool applyEachHit)
    {
        _applyDebuffEachHit = applyEachHit;
    }

    public void SetAutoReturn(float delay)
    {
        _lifetime = delay;
        if (_returnCoroutine != null)
            StopCoroutine(_returnCoroutine);

        _returnCoroutine = StartCoroutine(AutoReturn());
    }

    private IEnumerator AutoReturn()
    {
        yield return new WaitForSeconds(_lifetime);

        if (_prefabToReturn != null && gameObject.activeInHierarchy)
        {
            var trail = GetComponent<TrailRenderer>();
            if (trail != null) trail.Clear();

            ObjectPooler.ReturnToPool(_prefabToReturn, gameObject);
        }
    }

    // 트리거 시 적에게 데미지 부여 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_payload.baseDamage <= 0) return;

        if (collision.TryGetComponent<DamageablePart>(out var part))
        {
            if (_hitParts.Contains(part)) return;
            _hitParts.Add(part);

            StartCoroutine(ApplyMultiHit(part));
        }
        else if (collision.TryGetComponent<AttackableObject>(out var attackable))
        {
            attackable.Hit(this.transform.position);
        }
    }

    // 공격 반영 
    private IEnumerator ApplyMultiHit(DamageablePart part)
    {
        for (int i = 0; i < _multiHitCount; i++)
        {
            bool isCrit = Random.value < _payload.critChance;
            float finalDamage = _payload.GetFinalDamage(_weaponBaseDamage, _weaponMultiplier, _isSkill, _isMelee, isCrit);

            part.TakePartDamage(transform.position, finalDamage, isCrit);
            MasterAudio.PlaySound(_hitSfxName);

            if (_applyDebuffEachHit || i == 0)
            {
                var appliedTypes = new HashSet<DebuffType>(); 
                ApplyDebuffs(part, appliedTypes);
            }

            if (i < _multiHitCount - 1)
                yield return new WaitForSeconds(_multiHitDelay);
        }
    }

    private void ApplyDebuffs(DamageablePart part, HashSet<DebuffType> appliedTypes)
    {
        if (_owner?.playerController is PlayerController player)
        {
            var stat = player.runtimeStat;
            float bleedChance = stat.BleedChance + stat.TemporaryBleedChanceBonus;

            if (bleedChance > 0f && !HasBleedDebuffSO(_debuffSOs))
            {
                var bleedSO = player.defaultDebuffHandler?.GetDebuff(DebuffType.Bleed, bleedChance);
                if (bleedSO != null)
                    TryApplyDebuff(part, bleedSO, appliedTypes);
            }
        }

        if (_debuffSOs != null && part.isDebuffalbe)
        {
            foreach (var debuff in _debuffSOs)
            {
                float finalChance = debuff.applyChance;

                if (debuff.type == DebuffType.Bleed && _owner?.playerStat is RuntimeStat stat)
                {
                    finalChance += stat.BleedChance + stat.TemporaryBleedChanceBonus;
                }

                var copy = ScriptableObject.CreateInstance<DebuffEffectSO>();
                copy.type = debuff.type;
                copy.value = debuff.value;
                copy.duration = debuff.duration;
                copy.icon = debuff.icon;
                copy.debuffSfxName = debuff.debuffSfxName;
                copy.applyChance = Mathf.Clamp01(finalChance);

                TryApplyDebuff(part, copy, appliedTypes);
            }
        }
    }

    private void TryApplyDebuff(DamageablePart part, DebuffEffectSO debuff, HashSet<DebuffType> appliedTypes)
    {
        if (appliedTypes.Contains(debuff.type)) return;

        if (Random.value < Mathf.Clamp01(debuff.applyChance))
        {
            part.enemyHandler._health.ApplyDebuff(debuff, _payload);
            appliedTypes.Add(debuff.type);
        }
    }

    private bool HasBleedDebuffSO(List<DebuffEffectSO> debuffs)
    {
        if (debuffs == null) return false;
        foreach (var debuff in debuffs)
        {
            if (debuff != null && debuff.type == DebuffType.Bleed)
                return true;
        }
        return false;
    }
}
