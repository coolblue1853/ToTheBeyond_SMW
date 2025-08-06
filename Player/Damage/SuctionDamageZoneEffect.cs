using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

public class SuctionDamageZoneEffect : MonoBehaviour
{
    // 흠수하는 형태의 데미지존
    // 추후 일반  DamageZoneEffect  와 상속관계 형성이 필요해 보입니다 
    [Header("데미지 설정")]
    [SerializeField] private float _tickInterval = 0.5f;
    [SerializeField] private Vector2 _boxSize = new Vector2(3f, 2f);
    [SerializeField] private Vector2 _boxOffset = Vector2.zero;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _suctionRadius = 5f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private string _hitSfxName;

    [Header("디버프 설정")]
    [SerializeField] private List<DebuffEffectSO> _debuffSOs;

    private float _duration;
    private float _suctionForce;
    private int _direction;

    private DamagePayload _payload;
    private float[] _weaponBaseDamage;
    private float _weaponMultiplier;
    private bool _isSkill;
    private bool _isMelee;


    // 페이로드 외부 주입 
    public void SetPayload(
        float[] weaponBaseDamage,
        float weaponMultiplier,
        DamagePayload payload,
        bool isSkill,
        bool isMelee,
        int direction,
        float duration)
    {
        _weaponBaseDamage = weaponBaseDamage;
        _weaponMultiplier = weaponMultiplier;
        _payload = payload;
        _isSkill = isSkill;
        _isMelee = isMelee;
        _direction = direction;
        _duration = duration;

        StartCoroutine(EffectRoutine());
    }

    public void SetSuction(float force)
    {
        _suctionForce = force;
    }

    private IEnumerator EffectRoutine()
    {
        float timer = 0f;
        float tickTimer = 0f;

        while (timer < _duration)
        {
            if (_moveSpeed > 0f)
            {
                Vector3 dir = Vector3.right * _direction;
                transform.position += dir * _moveSpeed * Time.deltaTime;
            }

            tickTimer += Time.deltaTime;
            if (tickTimer >= _tickInterval)
            {
                ApplyAuraDamage();
                ApplySuction();
                tickTimer = 0f;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    // 데미지 적용 
    private void ApplyAuraDamage()
    {
        Vector2 center = (Vector2)transform.position + _boxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, _boxSize, 0f, _enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<DamageablePart>(out var enemy))
            {
                bool isCrit = Random.value < _payload.critChance;
                float finalDamage = _payload.GetFinalDamage(_weaponBaseDamage, _weaponMultiplier, _isSkill, _isMelee, isCrit);
                enemy.TakePartDamage(transform.position, finalDamage, isCrit);
                MasterAudio.PlaySound(_hitSfxName);

                var appliedTypes = new HashSet<DebuffType>();
                ApplyDebuffs(enemy, appliedTypes);
            }
            else if (hit.TryGetComponent<AttackableObject>(out var obj))
            {
                obj.Hit(transform.position);
            }
        }
    }

    // 흡수 효과 적용 
    private void ApplySuction()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _suctionRadius, _enemyLayer);
        foreach (var hit in hits)
        {
            Rigidbody2D rb = hit.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                Vector2 toCenter = (transform.position - hit.transform.position);
                float distance = toCenter.magnitude;

                if (distance < 0.5f)
                    continue;

                Vector2 dir = toCenter.normalized;
                float force = _suctionForce * (1f - (distance / _suctionRadius));
                force = Mathf.Clamp(force, 1f, _suctionForce);

                rb.velocity = Vector2.zero;
                rb.AddForce(dir * force, ForceMode2D.Force);
            }
        }
    }

    // 디버프 적용  
    private void ApplyDebuffs(DamageablePart part, HashSet<DebuffType> appliedTypes)
    {
        if (part == null || !part.isDebuffalbe) return;

        if (_debuffSOs != null)
        {
            foreach (var debuff in _debuffSOs)
            {
                float chance = debuff.applyChance;

                var copy = ScriptableObject.CreateInstance<DebuffEffectSO>();
                copy.type = debuff.type;
                copy.value = debuff.value;
                copy.duration = debuff.duration;
                copy.icon = debuff.icon;
                copy.debuffSfxName = debuff.debuffSfxName;
                copy.applyChance = Mathf.Clamp01(chance);

                TryApplyDebuff(part, copy, appliedTypes);
            }
        }
    }

    private void TryApplyDebuff(DamageablePart part, DebuffEffectSO debuff, HashSet<DebuffType> appliedTypes)
    {
        if (appliedTypes.Contains(debuff.type)) return;

        if (Random.value < debuff.applyChance)
        {
            part.enemyHandler._health.ApplyDebuff(debuff, _payload);
            appliedTypes.Add(debuff.type);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _suctionRadius);
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + _boxOffset;
        Gizmos.DrawWireCube(center, _boxSize);
    }
}
