using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstantDamageEffect : MonoBehaviour
{
    // 적에게 1번 데미지를 주는 구역 생성 
    [Header("즉발 데미지 설정")]
    [SerializeField] private Vector2 _boxSize = new Vector2(3f, 2f);      // 폭발 범위 크기 (사각형)
    [SerializeField] private float _damageDelay = 0.2f;                    // 데미지 지연 시간
    [SerializeField] private LayerMask _enemyLayer;                        // 적 레이어
    [SerializeField] private bool _applyDamageOnce = true;                  // 데미지 한 번만 적용 여부
    [SerializeField] private Vector2 _boxOffset = Vector2.zero;            // 사각형 범위 오프셋

    private DamagePayload _payload;
    private float[] _weaponBaseDamage;
    private float _weaponMultiplier;
    private bool _isSkill;
    private bool _isMelee;
    private List<IOnHitEffect> _onHitEffects = new();

    public void RegisterOnHitEffect(IOnHitEffect effect)
    {
        if (!_onHitEffects.Contains(effect))
            _onHitEffects.Add(effect);
    }

    // 페이로드 외부 주입 
    public void SetPayload(float[] weaponBaseDamage, float weaponMultiplier, DamagePayload payload, bool isSkill, bool isMelee, float desroyTime)
    {
        _weaponBaseDamage = weaponBaseDamage;
        _weaponMultiplier = weaponMultiplier;
        _payload = payload;
        _isSkill = isSkill;
        _isMelee = isMelee;

        if (_applyDamageOnce)
            StartCoroutine(ApplyInstantDamage());
        Destroy(gameObject, desroyTime);
    }

    // 데미지 및 디버프 적용 
    private IEnumerator ApplyInstantDamage()
    {
        yield return new WaitForSeconds(_damageDelay); // 데미지 지연

        // 즉시 데미지 처리 (사각형 범위 내 적에게 데미지 주기)
        Vector2 center = (Vector2)transform.position + _boxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, _boxSize, 0f, _enemyLayer);
        
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<DamageablePart>(out var enemy))
            {
                bool isCrit = Random.value < _payload.critChance;
                float finalDamage = _payload.GetFinalDamage(_weaponBaseDamage, _weaponMultiplier, _isSkill, _isMelee, isCrit);
                enemy.TakePartDamage(transform.position, finalDamage, isCrit);

                // 추가: 출혈 → 흡혈 전환 등 효과 실행
                foreach (var effect in _onHitEffects)
                {
                    effect.ApplyEffect(enemy.gameObject, _payload);

                }
            }
            else if (hit.TryGetComponent<AttackableObject>(out var attackable))
            {
                attackable.Hit(this.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + _boxOffset;
        Gizmos.DrawWireCube(center, _boxSize); // 사각형 범위 시각화
    }
}
