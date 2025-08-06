using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DarkTonic.MasterAudio;
public class MarkTargetSkill : WeaponSkill
{
    // 화면 내의 적에게 받는 데미지를 증가시키는 스킬 부여 

    [Header("디버프 설정")]
    [SerializeField] private List<DebuffEffectSO> _debuffsToApply;

    [Header("진화 시 직접 타격 설정")]
    [SerializeField] private GameObject _impactPrefab;
    [SerializeField] private float _impactDelay = 0.1f;
    [SerializeField] private float _impactDuration = 0.5f;

    [Header("설정")]
    [SerializeField] private bool _isEvolved = false;

    public override bool Activate()
    {
        if (!(_weapon is RangedWeapon ranged)) return false;

        List<EnemyHealth> visibleEnemies = FindVisibleEnemies();
        if (visibleEnemies.Count == 0) return false;

        foreach (var enemy in visibleEnemies)
        {
            ApplyDebuffs(enemy);
        }

        if (_isEvolved)
        {
            EnemyHealth target = visibleEnemies.OrderByDescending(e => e.CurrentHealth).FirstOrDefault();
            if (target != null)
            {
                Vector3 impactPos = target.transform.position;
                SpawnImpact(impactPos);
            }
        }

        StartCoroutine(DelayRoutine());
        return true;
    }

    // 화면내의 적 체크 
    private List<EnemyHealth> FindVisibleEnemies()
    {
        List<EnemyHealth> results = new();
        EnemyHealth[] all = GameObject.FindObjectsOfType<EnemyHealth>();

        foreach (var enemy in all)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.transform.position);
            if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
            {
                results.Add(enemy);
            }
        }

        return results;
    }

    private void ApplyDebuffs(EnemyHealth enemy)
    {
        foreach (var debuff in _debuffsToApply)
        {
            if (Random.value <= debuff.applyChance)
            {              
                enemy.ApplyDebuff(debuff);
                MasterAudio.PlaySound($"Debuff_{debuff.debuffSfxName}");
            }
        }
    }

    private void SpawnImpact(Vector3 position)
    {
        if (_impactPrefab != null)
        {
            GameObject go = Instantiate(_impactPrefab, position, Quaternion.identity);
            Destroy(go, _impactDuration);
        }
    }

    public void SetEvolved(bool evolved)
    {
        _isEvolved = evolved;
    }
}