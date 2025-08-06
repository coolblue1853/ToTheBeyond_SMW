using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 원거리 공격 탄환
    private DamagePayload _payload;
    private bool _isSkill;
    private bool _isMelee;
    private float _weaponMultiplier;
    private float[] _weaponBaseDamage;
    private GameObject _prefabToReturn;
    private Coroutine _autoReturnCoroutine;
    private List<DebuffEffectSO> _debuffSOs;

    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private LayerMask _hitReturnLayers;
    [SerializeField] private LayerMask _groundReturnLayers;
    [SerializeField] private string _bulletHitSfxName;

    private int _hitCount = 0;
    private bool _useFalloff = false;
    private bool _hasReturned = false;

    public void SetPayload(float[] weaponBaseDamage, float weaponMultiplier, DamagePayload payload, bool isSkill = false, bool isMelee = false)
    {
        _payload = payload;
        _isSkill = isSkill;
        _isMelee = isMelee;
        _weaponMultiplier = weaponMultiplier;
        _weaponBaseDamage = weaponBaseDamage;

        _hitCount = 0;
    }

    public void SetReturnReference(GameObject prefab)
    {
        _prefabToReturn = prefab;
    }

    public void SetDebuffs(List<DebuffEffectSO> debuffs)
    {
        _debuffSOs = debuffs;
    }

    // 관통시 데미지 하락
    public void SetFalloffDamage(bool useFalloff)
    {
        _useFalloff = useFalloff;
    }

    public void StartAutoReturn(float delay)
    {
        _hasReturned = false;

        if (_autoReturnCoroutine != null)
            StopCoroutine(_autoReturnCoroutine);

        _autoReturnCoroutine = StartCoroutine(AutoReturn(delay));
    }

    private IEnumerator AutoReturn(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_hasReturned || _prefabToReturn == null || !gameObject.activeInHierarchy)
            yield break;

        ReturnToPool();
    }


    // 온트리거, 적에게 데미지 반영
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool didHitEnemy = false;

        if (collision.TryGetComponent<DamageablePart>(out var enemy))
        {
            _hitCount++;
            didHitEnemy = true;

            bool isCrit = Random.value < _payload.critChance;
            float multiplier = _weaponMultiplier;

            if (_useFalloff)
                multiplier *= Mathf.Max(0.5f, 1f - 0.2f * (_hitCount - 1));

            float finalDamage = _payload.GetFinalDamage(_weaponBaseDamage, multiplier, _isSkill, _isMelee, isCrit);

            MasterAudio.PlaySound($"Hit_{_bulletHitSfxName}");
            enemy.TakePartDamage(transform.position, finalDamage, isCrit);

            if (_debuffSOs != null && enemy.isDebuffalbe)
            {
                foreach (var debuff in _debuffSOs)
                {
                    float finalChance = debuff.applyChance;

                    if (debuff.type == DebuffType.Bleed && _payload.statProvider is RuntimeStat runtimeStat)
                    {
                        finalChance += runtimeStat.TemporaryBleedChanceBonus;
                    }

                    if (Random.value < finalChance)
                    {
                        if (!string.IsNullOrEmpty(debuff.debuffSfxName))
                            MasterAudio.PlaySound($"Debuff_{debuff.debuffSfxName}");

                        enemy.enemyHandler.ApplyDebuff(debuff);
                    }
                }
            }
 
        }
        else if (collision.TryGetComponent<AttackableObject>(out var attackable))
        {
            attackable.Hit(transform.position);
        }

        if ((!_payload.isPiercing &&
            _prefabToReturn != null &&
            IsInLayerMask(collision.gameObject.layer, _hitReturnLayers) &&
            (didHitEnemy || collision.TryGetComponent<AttackableObject>(out _)))
            || IsInLayerMask(collision.gameObject.layer, _groundReturnLayers))
        {
            ReturnToPool();
        }


    }

    private void ReturnToPool()
    {
        if (_hasReturned) return;
        _hasReturned = true;

        if (_autoReturnCoroutine != null)
        {
            StopCoroutine(_autoReturnCoroutine);
            _autoReturnCoroutine = null;
        }

        if (_trailRenderer != null)
            _trailRenderer.Clear();

        ObjectPooler.ReturnToPool(_prefabToReturn, gameObject);
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
