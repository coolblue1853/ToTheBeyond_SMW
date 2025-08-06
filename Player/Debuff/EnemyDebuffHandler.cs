using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDebuffHandler : MonoBehaviour
{
    // 적 디버프 관리자 
    private float _damageTakenBonus = 0f;

    [SerializeField] private GameObject debuffIconUIPrefab;
    [SerializeField] private Transform debuffIconAnchor;

    // 적이 보유한 디버프 리스트 
    private HashSet<DebuffType> _activeDebuffTypes = new();
    private Dictionary<DebuffType, Coroutine> _debuffCoroutines = new();
    private Dictionary<DebuffType, int> _activeDebuffStacks = new();

    private int _bleedStack = 0;
    private Coroutine _bleedCoroutine = null;
    private DamagePayload _lastPayload;

    private EnemyHealth _health;
    private GameObject _bleedUI;
    private GameObject _absorbUI;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    public float GetDamageTakenModifier() => 1f + _damageTakenBonus;


    // 디버프 적용 
    public void ApplyDebuff(DebuffInfo info)
    {    
        if(DebuffExection(info))
            return;
        
        if (info.type == DebuffType.Bleed)
        {
            HandleBleedDebuff(info);
        }
        else
        {
            if (_activeDebuffTypes.Contains(info.type))
            {
                if (_debuffCoroutines.TryGetValue(info.type, out var coroutine))
                {
                    StopCoroutine(coroutine);
                    _debuffCoroutines.Remove(info.type);
                }
            }

            Coroutine newCoroutine = info.type switch
            {
                DebuffType.DamageTakenIncrease => StartCoroutine(HandleDamageTakenDebuff(info.value, info.duration)),
                DebuffType.MarkedTarget => StartCoroutine(HandleMarkedDebuff(info.duration)),
                DebuffType.Slow => StartCoroutine(HandleSlowDebuff(info.value, info.duration)),

                _ => null
            };

            if (newCoroutine != null)
            {
                _activeDebuffTypes.Add(info.type);
                _debuffCoroutines[info.type] = newCoroutine;
                _activeDebuffStacks[info.type] = 1;
            }

            if (info.icon != null && debuffIconUIPrefab != null)
                CreateIcon(info);
        }
    }

    // 디버프 적용 오버로드 (출혈 흡혈등의 데미지 부여 )
    public void ApplyDebuff(DebuffInfo info, DamagePayload payload)
    {
        if(DebuffExection(info))
            return;
        
        _lastPayload = payload;

        bool isFirstBleed = info.type == DebuffType.Bleed && _bleedCoroutine == null;
        ApplyDebuff(info);

        if (info.type == DebuffType.Bleed && isFirstBleed && _bleedUI == null)
        {
            _bleedUI = CreateIcon(info);
        }
    }


    // 특수상황에서 디버프를 막는 경우 ex :  흡혈시 출혈 불가 
    public bool DebuffExection(DebuffInfo info)
    {
        if (info.type == DebuffType.Bleed && HasDebuff(DebuffType.VampireAbsorb))
            return true;
        return false;
    }

    // 스택형 디버프 부여 
    public void ApplyDebuffStacks(DebuffEffectSO so, int stackCount)
    {
        DebuffInfo info = new()
        {
            type = so.type,
            value = so.value,
            duration = so.duration,
            chance = so.applyChance,
            icon = so.icon,
            animatorController = so.animatorController
        };

        if (info.type == DebuffType.VampireAbsorb)
        {
            if (_activeDebuffTypes.Contains(info.type))
            {
                _activeDebuffStacks[info.type] = Mathf.Min(_activeDebuffStacks[info.type] + stackCount, 5);
            }
            else
            {
                _activeDebuffTypes.Add(info.type);
                _activeDebuffStacks[info.type] = Mathf.Min(stackCount, 5);
            }

            if (_debuffCoroutines.TryGetValue(info.type, out var coroutine))
            {
                StopCoroutine(coroutine);
                _debuffCoroutines.Remove(info.type);
            }

            var newCoroutine = StartCoroutine(HandleAbsorbDebuff(info));
            _debuffCoroutines[info.type] = newCoroutine;

            if (_absorbUI != null) Destroy(_absorbUI);
            _absorbUI = CreateIcon(info);
            var uiScript = _absorbUI.GetComponent<DebuffIconUI>();
            uiScript?.UpdateStack(_activeDebuffStacks[info.type]);
        }
    }

    // 스크립터블 오브젝트를 통한 디버프 부여  
    public void ApplyDebuff(DebuffEffectSO so)
    {
        DebuffInfo info = new()
        {
            type = so.type,
            value = so.value,
            duration = so.duration,
            chance = so.applyChance,
            icon = so.icon,
            animatorController = so.animatorController
        };

        ApplyDebuff(info);
    }

    public void ApplyDebuff(DebuffEffectSO so, DamagePayload payload)
    {
        DebuffInfo info = new()
        {
            type = so.type,
            value = so.value,
            duration = so.duration,
            chance = so.applyChance,
            icon = so.icon,
            animatorController = so.animatorController
        };

        ApplyDebuff(info, payload);
    }

    // 아이콘 생성  스택의경우 포함 
    private GameObject CreateIcon(DebuffInfo info)
    {
        Vector3 anchorPos = (debuffIconAnchor != null ? debuffIconAnchor.position : transform.position);

        if (anchorPos.magnitude > 10000f || float.IsNaN(anchorPos.x) || float.IsNaN(anchorPos.y))
        {
            anchorPos = transform.position;
        }

        Vector3 spawnPos = anchorPos + Vector3.up * 1.5f;
        GameObject ui = Instantiate(debuffIconUIPrefab, spawnPos, Quaternion.identity);

        var uiScript = ui.GetComponent<DebuffIconUI>();
        uiScript.Initialize(info);
        uiScript.FollowTarget(debuffIconAnchor != null ? debuffIconAnchor : transform);
        uiScript.StartCoroutine(uiScript.CorrectInitialPosition());

        if (info.type == DebuffType.Bleed)
            uiScript.UpdateStack(_bleedStack);

        return ui;
    }

    private DebuffInfo _latestBleedInfo;

    private void HandleBleedDebuff(DebuffInfo info)
    {
        _latestBleedInfo = info;

        if (_bleedCoroutine != null)
        {
            StopCoroutine(_bleedCoroutine);
            _bleedStack = Mathf.Min(_bleedStack + 1, 5);
        }
        else
        {
            _bleedStack = 1;
        }

        _bleedCoroutine = StartCoroutine(HandleBleedTick());
        _activeDebuffTypes.Add(DebuffType.Bleed);
        _activeDebuffStacks[DebuffType.Bleed] = _bleedStack;

        if (_bleedUI != null)
        {
            var uiScript = _bleedUI.GetComponent<DebuffIconUI>();
            uiScript?.UpdateStack(_bleedStack);
            uiScript?.RefreshDuration(info.duration);
        }
    }

    private IEnumerator HandleBleedTick()
    {
        float elapsed = 0f, tickTimer = 0f;
        float duration = _latestBleedInfo.duration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= 0.5f)
            {
                tickTimer = 0f;
                float baseBleed = _latestBleedInfo.value * (1f + 0.3f * (_bleedStack - 1));
                float totalDamage = _lastPayload.GetBleedDamage(baseBleed);
                _health.TakeDamage(totalDamage, false);
            }

            yield return null;
        }

        ClearBleed();
    }

    // 디버프 해체 함수들 + 디버프 관련 로직. 추후 디버프 관련 로직 ( Handler ) 들은 별도의 스크립트로 분리 필.
    public void ClearBleed()
    {
        _bleedStack = 0;
        _activeDebuffStacks.Remove(DebuffType.Bleed);

        if (_bleedCoroutine != null)
        {
            StopCoroutine(_bleedCoroutine);
            _bleedCoroutine = null;
        }

        if (_bleedUI != null)
        {
            Destroy(_bleedUI);
            _bleedUI = null;
        }

        _activeDebuffTypes.Remove(DebuffType.Bleed);
    }

    private IEnumerator HandleDamageTakenDebuff(float bonusPercent, float duration)
    {
        _damageTakenBonus = bonusPercent;
        yield return new WaitForSeconds(duration);
        _damageTakenBonus = 0f;
        _activeDebuffTypes.Remove(DebuffType.DamageTakenIncrease);
        _debuffCoroutines.Remove(DebuffType.DamageTakenIncrease);
        _activeDebuffStacks.Remove(DebuffType.DamageTakenIncrease);
    }

    private IEnumerator HandleSlowDebuff(float multiplier, float duration)
    {
        if (TryGetComponent<EnemyController>(out var controller))
        {
            controller.SetSpeedMultiplier(multiplier);
        }

        yield return new WaitForSeconds(duration);

        if (TryGetComponent<EnemyController>(out var controllerReset))
        {
            controllerReset.SetSpeedMultiplier(1f);
        }

        _activeDebuffTypes.Remove(DebuffType.Slow);
        _debuffCoroutines.Remove(DebuffType.Slow);
        _activeDebuffStacks.Remove(DebuffType.Slow);
    }

    private IEnumerator HandleMarkedDebuff(float duration)
    {
        yield return new WaitForSeconds(duration);
        _activeDebuffTypes.Remove(DebuffType.MarkedTarget);
        _debuffCoroutines.Remove(DebuffType.MarkedTarget);
        _activeDebuffStacks.Remove(DebuffType.MarkedTarget);
    }

    public bool HasDebuff(DebuffType type) => _activeDebuffTypes.Contains(type);

    public int GetDebuffStack(DebuffType type)
    {
        return _activeDebuffStacks.TryGetValue(type, out var stack) ? stack : 0;
    }

    public Dictionary<DebuffType, int> GetAllActiveDebuffs()
    {
        return new Dictionary<DebuffType, int>(_activeDebuffStacks);
    }

    public void RemoveDebuff(DebuffType type)
    {
        if (type == DebuffType.Bleed)
        {
            ClearBleed();
        }
        else if (_debuffCoroutines.TryGetValue(type, out var coroutine))
        {
            StopCoroutine(coroutine);
            _debuffCoroutines.Remove(type);
            _activeDebuffTypes.Remove(type);
            _activeDebuffStacks.Remove(type);

            if (type == DebuffType.VampireAbsorb && _absorbUI != null)
            {
                Destroy(_absorbUI);
                _absorbUI = null;
            }
        }
    }

    public void ClearAllDebuffs()
    {
        ClearBleed();

        foreach (var coroutine in _debuffCoroutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        _debuffCoroutines.Clear();
        _activeDebuffTypes.Clear();
        _activeDebuffStacks.Clear();
        _damageTakenBonus = 0f;

        if (_absorbUI != null)
        {
            Destroy(_absorbUI);
            _absorbUI = null;
        }
    }

    private void OnDisable()
    {
        ClearAllDebuffs();
    }

    public static Dictionary<EnemyController, int> ExtractBleedStacks(List<EnemyController> enemies)
    {
        Dictionary<EnemyController, int> extracted = new();

        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            if (!enemy.TryGetComponent(out EnemyDebuffHandler debuffHandler)) continue;

            int bleedStack = debuffHandler.GetDebuffStack(DebuffType.Bleed);
            if (bleedStack <= 0) continue;

            debuffHandler.RemoveDebuff(DebuffType.Bleed);
            extracted[enemy] = bleedStack;
        }

        return extracted;
    }

    private IEnumerator HandleAbsorbDebuff(DebuffInfo info)
    {
        float elapsed = 0f;
        float tickInterval = 0.25f;

        float baseDamage = info.value;

        while (elapsed < info.duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            int stack = GetDebuffStack(DebuffType.VampireAbsorb);
            if (_health != null)
            {
                float scaledDamage = baseDamage * (1f + 0.3f * (stack - 1));
                _health.TakeDamage(scaledDamage, false);
            }
        }

        RemoveDebuff(info.type);
    }
}
