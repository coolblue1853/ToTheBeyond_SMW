using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class WaveSlashSkill : WeaponSkill
{
    // 원거리로 검기를 날리는 기술 

    [SerializeField] private GameObject _slashPrefab;
    [SerializeField] private float _speed;
    [SerializeField] private int _slashCount = 1;
    [SerializeField] private float _spreadAngle = 15f;
    [SerializeField] private bool _isEvolved;

    
    [Header("효과음")] 
    [SerializeField] private string _introSfxName;

    // 애니메이션 관련 변수 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _attackAnimStr;

    public override bool Activate()
    {
        if (!(_weapon is MeleeWeapon melee)) return false;
        StartCoroutine(FireSlashes(melee));
        return true;
    }

    private IEnumerator FireSlashes(MeleeWeapon melee)
    {
        _animController = melee.playerController.GetComponent<PlayerAnimatorController>();
        if (_animController != null && !string.IsNullOrEmpty(_attackAnimStr))
        {
            _animController.PlayUpperBodyReloadWithAutoReset(
                _attackAnimStr,
                melee.data.upperBodyIdleAnim,
                _delayTime  / melee.playerStat.AttackSpeed,
                4f
            );
        }
        
        int count = _isEvolved ? 3 : 1;
        float start = -_spreadAngle * (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            MasterAudio.PlaySound(_introSfxName);
            
            float angle = start + _spreadAngle * i;
            Quaternion rot = Quaternion.Euler(0, 0, angle);

            GameObject proj = ObjectPooler.SpawnFromPool(_slashPrefab, melee.transform.position, rot);

            // localScale 방향 고정 (좌우 반전 금지)
            Vector3 scale = proj.transform.localScale;
            scale.x = Mathf.Abs(scale.x); // 항상 정방향 유지
            proj.transform.localScale = scale;

            // 시각적 방향 반전이 필요할 경우만 flipX 사용 (선택적)
            if (proj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.flipX = melee.FacingDirection == -1;
            }

            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                var payload = melee.statProvider.CreatePayloadFrom();
                payload.isPiercing = true;

                p.SetPayload(
                    melee.data.weaponBaseDamage,
                    melee.data.weaponDamageMultiplier,
                    payload,
                    isSkill: true,
                    isMelee: true
                );

                p.SetReturnReference(_slashPrefab);
                p.StartAutoReturn(1f);

                Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 baseDir = rot * Vector2.right;
                    rb.velocity = baseDir * _speed * melee.FacingDirection;
                }
            }

            yield return null;
        }

        StartCoroutine(DelayRoutine());
    }
}
