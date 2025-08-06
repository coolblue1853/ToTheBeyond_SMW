using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class UltimateHeartPierceSkill : WeaponSkill
{
    // 화면 내의 흡혈 상태의 적에게 연속 공격을 가하는 스킬 

    [SerializeField] private GameObject heartIconPrefab;
    [SerializeField] private Transform handTarget;
    [SerializeField] private float heartTravelTime = 0.5f;
    [SerializeField] private float baseMultiplierPerStack = 0.4f;
    [SerializeField] private int followUpHits = 5;
    [SerializeField] private float initialInterval = 0.3f;
    [SerializeField] private float finalInterval = 0.05f;
    [SerializeField] private float _breakInterval = 1f;


    // 애니메이션 관련 변수 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _introAnimStr;
    [SerializeField] private string _heartbreakAnimStr;
    [SerializeField] private string _IdleAnimStr;

    // 사운드 관련 변수 
    [SerializeField] private GameObject _bloodSFX;
    [SerializeField] private float _sfxDestroyTime = 1.5f;

    public override bool Activate()
    {
        List<EnemyDebuffHandler> targets = FindTargetsWithLifesteal();
        if (targets.Count == 0) return false;

        StartCoroutine(ExecuteUltimate(targets));
        return true;
    }

    //스킬 발동 
    private IEnumerator ExecuteUltimate(List<EnemyDebuffHandler> targets)
    {      
        _animController = _weapon.playerController.GetComponent<PlayerAnimatorController>();
        _weapon.isUsingSkill = true;
        _weapon.isMovementLocked = true;

        _animController.PlayUpperBodyAnimation(_introAnimStr);
        yield return new WaitForSeconds(0.05f);
        
        List<GameObject> hearts = new();
        List<Coroutine> heartCoroutines = new();
        Dictionary<EnemyDebuffHandler, int> stackMap = new();
        List<EnemyStunned> stunnedCache = new();
        List<EnemyDebuffHandler> aliveTargets = new();

        foreach (var target in targets) 
        {
            if (!target || target.Equals(null) || !target.gameObject.activeInHierarchy)
                continue;

            if (target.TryGetComponent<EnemyHealth>(out var health) && health.IsDead)
                continue;

            aliveTargets.Add(target);
        }

        foreach (var target in aliveTargets)
        {
            var root = target.transform.root;
            if (root.TryGetComponent<BehaviorTree>(out var bt)) // 행동트리 정지 
            {
                bt.enabled = false;
            }

            if (root.TryGetComponent<EnemyStunned>(out var stunned))
            {
                stunned.externalControlDisabled = true;
                stunnedCache.Add(stunned);
            }
        }

        var rb = transform.root.GetComponent<Rigidbody2D>();
        float originalGravity = 0f;

        if (rb != null)
        {
            originalGravity = rb.gravityScale;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        _owner.root.GetComponent<PlayerHealth>()?.SetInvincible(true);

        foreach (var target in aliveTargets) // 화면 내의 적에게서 흡혈 버프가 있는지 체크 
        {
            if (!target || target.Equals(null)) continue;

            int stack = target.GetDebuffStack(DebuffType.VampireAbsorb);
            stackMap[target] = stack;
            target.RemoveDebuff(DebuffType.VampireAbsorb);
        }

        foreach (var target in aliveTargets) // 해당하는 적에게서 심장 프리팹을 가져오는 과정 
        {
            if (!target || target.Equals(null)) continue;

            GameObject icon = Instantiate(heartIconPrefab, target.transform.position, Quaternion.identity);
            hearts.Add(icon);
            heartCoroutines.Add(StartCoroutine(MoveHeart(icon.transform, handTarget, heartTravelTime)));

        }

        foreach (var coroutine in heartCoroutines)
            yield return coroutine;

        yield return new WaitForSeconds(_breakInterval);
        
        _animController.PlayUpperBodyAnimation(_heartbreakAnimStr);
        
        foreach (var pair in stackMap) // 해당하는 적에게 즉시 데미지 부여 
        {
            var target = pair.Key;
            if (!target || target.Equals(null) || !target.gameObject.activeInHierarchy) continue;

            int stack = pair.Value;

            if (target.TryGetComponent<EnemyHealth>(out var health) && !health.IsDead)
            {
                var payload = _weapon.statProvider.CreatePayloadFrom(true);
                float damage = payload.GetFinalDamage(
                    new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel },
                    1f,
                    isSkill: true,
                    isMelee: _isMelee
                );

                float multiplier = Mathf.Max(1f, 1 + (stack * baseMultiplierPerStack));
                health.TakeDamage(damage * multiplier, false);
            }
        }

        float delay = initialInterval;
        float step = (initialInterval - finalInterval) / Mathf.Max(1, followUpHits - 1);

        for (int i = 0; i < followUpHits; i++)
        {
            foreach (var target in aliveTargets)
            {
                if (!target || target.Equals(null) || !target.gameObject.activeInHierarchy) continue;

                if (target.TryGetComponent<EnemyHealth>(out var health) && !health.IsDead)
                {
                    int stack = stackMap.TryGetValue(target, out var s) ? s : 1;
                    float multiplier = Mathf.Max(1f, 1 + (stack * baseMultiplierPerStack));

                    var payload = _weapon.statProvider.CreatePayloadFrom(true);
                    float damage = payload.GetFinalDamage(
                        new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel },
                        1f,
                        isSkill: true,
                        isMelee: _isMelee
                    );

                    var vfx = Instantiate(_bloodSFX, handTarget.transform.position, Quaternion.identity);
                    Destroy(vfx, _sfxDestroyTime);

                    health.TakeDamage(damage * multiplier, false);
                }
            }

            yield return new WaitForSeconds(delay);
            delay = Mathf.Max(finalInterval, delay - step);
        }

        foreach (var heart in hearts)          // 스킬 종료를 위해 심장 프리팹 제거 
            if (heart != null) Destroy(heart);

        foreach (var target in aliveTargets)
        {
            if (target == null || !target || target.transform == null)
                continue;

            Transform root = null;
            try
            {
                root = target.transform.root;
            }
            catch
            {
                continue; // transform 접근 중 파괴된 경우
            }

            if (root != null && root.TryGetComponent<BehaviorTree>(out var bt))
            {
                bt.enabled = false;
            }

            if (root != null && root.TryGetComponent<EnemyStunned>(out var stunned))
            {
                stunned.externalControlDisabled = true;
                stunnedCache.Add(stunned);
            }
        }

        foreach (var stunned in stunnedCache)
        {
            if (stunned != null)
                stunned.externalControlDisabled = false;
        }

        if (rb != null)
        {
            rb.gravityScale = originalGravity;
        }

        _owner.root.GetComponent<PlayerHealth>()?.SetInvincible(false);

        
        _animController.PlayUpperBodyAnimation(_IdleAnimStr);
        _weapon.isUsingSkill = false;
        _weapon.isMovementLocked = false;
        StartCoroutine(DelayRoutine());
    }

    // 심장을 손 위치로 이동시키는 함수 
    private IEnumerator MoveHeart(Transform heart, Transform dynamicTarget, float duration)
    {
        Vector3 startPos = heart.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            if (dynamicTarget == null)
                yield break;
            Vector3 targetPos = dynamicTarget.position;
            heart.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (dynamicTarget != null)
            heart.position = dynamicTarget.position;
    }

    // 흡혈상태인 적을 찾는 함수 
    private List<EnemyDebuffHandler> FindTargetsWithLifesteal()
    {
        List<EnemyDebuffHandler> result = new();
        Vector3 camMin = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 camMax = Camera.main.ViewportToWorldPoint(Vector3.one);

        Collider2D[] hits = Physics2D.OverlapAreaAll(camMin, camMax);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyDebuffHandler debuff) &&
                debuff.HasDebuff(DebuffType.VampireAbsorb))
            {
                result.Add(debuff);
            }
        }
        return result;
    }
}