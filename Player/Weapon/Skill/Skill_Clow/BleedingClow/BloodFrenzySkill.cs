using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;
using Unity.Cinemachine;
public class BloodFrenzySkill : WeaponSkill
{
    // 화면 내의 적에게 순간이동하여 공격하는 스킬, 출혈이 있을시 출혈 폭파  

    [SerializeField] private float _dashSpeed = 25f;
    [SerializeField] private float _stunDuration = 1f;
    [SerializeField] private float _attackDelay = 0.15f;
    [SerializeField] private float _bleedExplosionMultiplier = 1.5f;
    [SerializeField] private GameObject _slashVFX;
    [SerializeField] private GameObject _finishVFX;
    [SerializeField] private string _slashSfx = "Slash";
    [SerializeField] private int _attackCount = 10;
    [SerializeField] private float _moveInterval = 0.05f;

    // 애니메이션 변수 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _rightAttackAnimStr;
    [SerializeField] private string _leftAttackAnimStr;
    [SerializeField] private string _idleAnimStr;

    // SFX 변수 
    [SerializeField] private GameObject _rightSFX;
    [SerializeField] private GameObject _leftSFX;
    [SerializeField] private float _sfxDestroyTime = 1.5f;

    public override bool Activate()
    {
        List<Transform> enemies = FindEnemiesInCamera();
        if (enemies.Count == 0) return false;

        if (!(_weapon is MeleeWeapon melee)) return false;
        _animController = melee.playerController.GetComponent<PlayerAnimatorController>();
        
        StartCoroutine(ExecuteBloodFrenzy(enemies));
        return true;
    }

    // 카메라 내에서 출혈 여부, 거리를 확인해서 적을 체크 
    private List<Transform> FindEnemiesInCamera()
    {
        List<Transform> results = new();
        Camera cam = Camera.main;
        Vector3 camMin = cam.ViewportToWorldPoint(Vector3.zero);
        Vector3 camMax = cam.ViewportToWorldPoint(Vector3.one);

        Collider2D[] hits = Physics2D.OverlapAreaAll(camMin, camMax);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out var health) && health.CurrentHealth > 0f)
                results.Add(hit.transform);
        }
        return results;
    }

    private IEnumerator ExecuteBloodFrenzy(List<Transform> targets)
    {
        Transform player = _owner.root;
        Vector3 originalPos = player.position;

        // 📷 시네머신 추적 해제
        CinemachineVirtualCamera vcam = null;
        Transform originalFollow = null;
        CinemachineCamera cam = null;
        
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null && brain.ActiveVirtualCamera != null)
        {
            // CinemachineCamera로 캐스트
             cam = brain.ActiveVirtualCamera as CinemachineCamera;
            if (cam != null)
            {
                originalFollow = cam.Follow;  // 현재 추적 대상 저장
                cam.Follow = null;            // 추적 해제
            }
        }


        // 중력 제거
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        float originalGravity = rb != null ? rb.gravityScale : 0f;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        int targetCount = targets.Count;
        if (targetCount == 0) yield break;

        List<int> hitCounts = new();
        int baseHit = _attackCount / targetCount;
        int remainder = _attackCount % targetCount;
        for (int i = 0; i < targetCount; i++)
            hitCounts.Add(baseHit + (i < remainder ? 1 : 0));

        int maxHits = Mathf.Max(hitCounts.ToArray());
        for (int i = 0; i < maxHits; i++)
        {
            for (int j = 0; j < targets.Count; j++)
            {
                if (hitCounts[j] > i && targets[j] != null)
                {
                    yield return MoveToTarget(player, targets[j].position);

                    if (j % 2 == 0)
                    {
                        var vfx = Instantiate(_rightSFX, transform.root.position, Quaternion.identity);
                        Destroy(vfx, _sfxDestroyTime);

                        if (_animController != null && !string.IsNullOrEmpty(_rightAttackAnimStr))
                        {
                            _animController.PlayUpperBodyAttack(
                                _rightAttackAnimStr,
                                _moveInterval  / _weapon.playerStat.AttackSpeed
                            );
                        }
                    }
                    else
                    {

                        var vfx = Instantiate(_leftSFX,  transform.root.position, Quaternion.identity);
                        Destroy(vfx, _sfxDestroyTime);

                        if (_animController != null && !string.IsNullOrEmpty(_leftAttackAnimStr))
                        {
                            _animController.PlayUpperBodyAttack(
                                _leftAttackAnimStr,
                                _moveInterval  / _weapon.playerStat.AttackSpeed
                            );
                        }
                    }
                    yield return new WaitForSeconds(_moveInterval);
                }
            }
            
        }

        yield return MoveToTarget(player, originalPos); // 다음 타겟 이동 
        if (rb != null) rb.gravityScale = originalGravity;
        
        if (cam != null && originalFollow != null)
            cam.Follow = originalFollow;    
        
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            StartCoroutine(PerformAttack(targets[i], hitCounts[i]));
        }

        if (_finishVFX != null)
            Instantiate(_finishVFX, player.position, Quaternion.identity);
        
        if (_animController != null )
        {
            _animController.PlayUpperBodyAnimation(
                _idleAnimStr,
                _delayTime
            );
        }
        StartCoroutine(DelayRoutine());
    }


    // 적에게로 이동하는 함수 
    private IEnumerator MoveToTarget(Transform player, Vector3 destination)
    {
        player.position = destination;
        yield return null;
    }

    private IEnumerator PerformAttack(Transform enemy, int hitCount)
    {
        if (enemy == null) yield break;
        if (!enemy.TryGetComponent<EnemyHealth>(out var health)) yield break;
        if (!enemy.TryGetComponent<EnemyDebuffHandler>(out var debuff)) yield break;

        float[] weaponBase = new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel };
        var payload = _weapon.statProvider.CreatePayloadFrom(true);
        float damage = payload.GetFinalDamage(
            weaponBase,
            1f,
            isSkill: true,
            isMelee: true,
            isCrit: false
        );

        if (debuff.HasDebuff(DebuffType.Bleed))
        {
            int stack = debuff.GetDebuffStack(DebuffType.Bleed);
            damage *= (1 + _bleedExplosionMultiplier * stack);
            debuff.ClearBleed();
        }

        for (int i = 0; i < hitCount; i++)
        {
            //  안전 체크
            if (enemy == null || health == null || health.Equals(null)) yield break;

            health.TakeDamage(damage, false);

            if (_slashVFX != null)
                Instantiate(_slashVFX, enemy.position, Quaternion.identity);

            MasterAudio.PlaySound(_slashSfx);
            yield return new WaitForSeconds(_attackDelay);
        }
    }

}
