using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class DuskStrikeSkill : HoldReleaseWeaponSkill
{
    // 화면 내의 적에게 난사를 가하는 스킬 

    [Header("설정")]
    [SerializeField] private float _slowTimeScale = 0.2f;
    [SerializeField] private float _markInterval = 0.05f;
    [SerializeField] private float _impactInterval = 0.1f;
    [SerializeField] private GameObject _markPrefab;
    [SerializeField] private GameObject _impactPrefab;
    [SerializeField] private float _impactDuration = 0.3f;
    [SerializeField] private Vector2 _markOffsetRange = new Vector2(0.3f, 0.3f);
    [SerializeField] private float _markDetectionRadius = 8f;

    private List<(GameObject mark, Vector3 pos)> _markEntries = new();
    private HashSet<Transform> _markedTargets = new();
    private Coroutine _scanRoutine;

    [Header("효과음")] 
    [SerializeField] private string _introSfxName = "Effect_SlowMotion";
    [SerializeField] private string _markSfxName = "Debuff_Mark";
    [SerializeField] private string _shootSfxName = "Revolver_HeadShot";
    [SerializeField] private string _outroSfxName = "Revolver_DropBullet";

    private int _pendingAmmoCost = 0;

    // 애니메이션 관련 변수 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _introAnimStr;
    [SerializeField] private string _attackAnimStr;

    public override bool Activate() => false;

    protected override void BeginSkill()
    {
        if (!(_weapon is RangedWeapon ranged)) return;
        if (ranged.CurrentAmmo <= 0)
        {
            _weapon.isUsingSkill = false;
            _weapon.isMovementLocked = false;
            return;
        }

        _animController = ranged.playerController.GetComponent<PlayerAnimatorController>();
        if (_animController != null && !string.IsNullOrEmpty(_introAnimStr))
        {
            _animController.PlayUpperBodyAnimation(_introAnimStr);
        }

        MasterAudio.PlaySound(_introSfxName);
        Time.timeScale = _slowTimeScale;
        _pendingAmmoCost = 0;
        _scanRoutine = StartCoroutine(ScanAndMarkRoutine());
    }

    protected override void EndSkill()
    {
        if (_scanRoutine != null)
        {
            StopCoroutine(_scanRoutine);
            _scanRoutine = null;
        }

        Time.timeScale = 1f;

        // 탄약은 ExecuteMarks()에서 개별 소비
        StartCoroutine(ExecuteMarks());
    }

    // 화면내에 공격 가능한적을 찾기
    private IEnumerator ScanAndMarkRoutine()
    {
        var ranged = _weapon as RangedWeapon;

        while (true)
        {
            if (_pendingAmmoCost >= ranged.CurrentAmmo) break;

            if (!TryMarkEnemies(ranged))
            {
                yield return new WaitForSecondsRealtime(_markInterval);
                continue;
            }

            yield return new WaitForSecondsRealtime(_markInterval);
        }
    }

    // 해당 적에게 마크 새기기 
    private bool TryMarkEnemies(RangedWeapon ranged)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.position, _markDetectionRadius);
        List<EnemyHealth> candidates = new();

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null && !_markedTargets.Contains(enemy.transform))
            {
                candidates.Add(enemy);
            }
        }

        if (candidates.Count == 0 && _markedTargets.Count > 0)
        {
            _markedTargets.Clear();
            return TryMarkEnemies(ranged);
        }

        if (candidates.Count == 0)
            return false;

        candidates.Sort((a, b) => b.CurrentHealth.CompareTo(a.CurrentHealth));
        var target = candidates[0];
        _markedTargets.Add(target.transform);

        Vector3 offset = new Vector3(
            Random.Range(-_markOffsetRange.x, _markOffsetRange.x),
            Random.Range(-_markOffsetRange.y, _markOffsetRange.y),
            0f
        );

        Vector3 markPos = target.transform.position + offset;
        GameObject mark = Instantiate(_markPrefab, markPos, Quaternion.identity);

        _markEntries.Add((mark, markPos));
        _pendingAmmoCost++;

        MarkExecuted();
        MasterAudio.PlaySound(_markSfxName);

        return true;
    }

    // 마크 위치에 공격 프리팹 생성 
    private IEnumerator ExecuteMarks()
    {
        if (_markEntries.Count == 0)
            yield break;
        
        Time.timeScale = 1f; 

        var ranged = _weapon as RangedWeapon;

        for (int i = 0; i < _markEntries.Count; i++)
        {
            var (mark, pos) = _markEntries[i];

            if (mark != null)
                Destroy(mark);

            GameObject impact = Instantiate(_impactPrefab, pos, Quaternion.identity);
            Destroy(impact, _impactDuration);

            if (_animController != null && !string.IsNullOrEmpty(_attackAnimStr))
            {
                _animController.PlayUpperBodyAttackWithAutoReset(
                    _attackAnimStr,
                    _weapon.data.upperBodyIdleAnim,
                    _impactInterval
                );
            }

            // 탄약 한 발씩 소비
            if (ranged.CurrentAmmo > 0)
            {
                ranged.ConsumeAmmo(1);
            }
            else
            {
                break;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 0.5f);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    float[] weaponBase = new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel };
                    var payload = _weapon.statProvider.CreatePayloadFrom(true);
                    float damage = payload.GetFinalDamage(
                        weaponBase,
                        1.0f,
                        isSkill: true,
                        isMelee: _isMelee,
                        isCrit: false
                    );

                    MasterAudio.PlaySound(_shootSfxName);
                    enemy.TakeDamage(damage, true);
                    break;
                }
            }

            yield return new WaitForSeconds(_impactInterval);
        }

        _markEntries.Clear();
        _markedTargets.Clear();
        _pendingAmmoCost = 0;
        MasterAudio.PlaySound(_outroSfxName);
        StartCoroutine(DelayRoutine());
    }

    private void OnDisable()
    {
        if (_scanRoutine != null)
        {
            StopCoroutine(_scanRoutine);
            _scanRoutine = null;
        }

        Time.timeScale = 1f;

        foreach (var entry in _markEntries)
        {
            if (entry.mark != null)
                Destroy(entry.mark);
        }

        _markEntries.Clear();
        _markedTargets.Clear();
        _pendingAmmoCost = 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_owner != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_owner.position, _markDetectionRadius);
        }
    }
#endif
}
