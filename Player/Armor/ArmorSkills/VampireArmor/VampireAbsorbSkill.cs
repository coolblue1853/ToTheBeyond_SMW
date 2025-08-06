using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireAbsorbSkill : ArmorSkill
{
    // 출혈중인 적을 추적하여 흡혈로 변환하는 스킬 
    [SerializeField] private GameObject _bloodOrbPrefab;
    [SerializeField] private float _maxHealPercent = 0.6f;
    [SerializeField] private float _buffDuration = 8f;
    [SerializeField] private DebuffEffectSO _vampireAbsorbSO;
    [SerializeField] private int _capacity = 20;

    private RuntimeStat _stat;
    private PlayerHealth _playerHealth;

    // 이전에 이 스킬이 적용한 버프만 추적
    private StatModifier _activeBuff;
    private Coroutine _buffCoroutine;
    
    public override void Initialize(SkillSO skillData, Transform owner, Armor armor, int level)
    {
        base.Initialize(skillData, owner, armor, level);
        _stat = armor.playerStat;
        _playerHealth = armor.playerController.GetComponent<PlayerHealth>();
    }

    public override bool Activate()
    {
        var enemies = FindEnemiesInCamera(); 
        int totalStacks = 0;

        foreach (var enemy in enemies)
        {
            if (!enemy.TryGetComponent(out EnemyDebuffHandler debuffHandler))
                continue;

            int bleedStacks = debuffHandler.GetDebuffStack(DebuffType.Bleed); // 출혈 수치를 가져와서 
            if (bleedStacks <= 0)
                continue;

            debuffHandler.RemoveDebuff(DebuffType.Bleed);
            debuffHandler.ApplyDebuffStacks(_vampireAbsorbSO, bleedStacks); // 삭제하고 흡혈 수치로 변환 

            for (int i = 0; i < bleedStacks; i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.4f, 0.4f),
                    Random.Range(-0.4f, 0.4f),
                    0f
                );

                Vector3 spawnPos = enemy.transform.position + randomOffset;
                var orb = Instantiate(_bloodOrbPrefab, spawnPos, Quaternion.identity);
                orb.GetComponent<BloodOrbEffect>().Initialize(_playerHealth, 1f);
            }

            totalStacks += bleedStacks;
        }

        // 새로운 버프 적용
        if (totalStacks > 0)
        {
            totalStacks = Mathf.Min(totalStacks, _capacity);

            _activeBuff = new StatModifier(
                StatType.FinalDamageMultiplier,
                1f + (totalStacks / 100f),
                StatModifier.ModifierMode.Multiplicative,
                sourceTag: "VampireAbsorbSkill" 
            );

            _stat.AddModifier(_activeBuff);
            _buffCoroutine = StartCoroutine(RemoveBuffAfter(_buffDuration, _activeBuff));
            
        }

        return totalStacks > 0;
    }

    private IEnumerator RemoveBuffAfter(float duration, StatModifier mod)
    {
        yield return new WaitForSeconds(duration);
        _stat.RemoveModifier(mod);
        if (_activeBuff == mod) _activeBuff = null;
        _buffCoroutine = null;
    }

    // 영역 내 출혈중인 적 추격 
    private List<EnemyController> FindEnemiesInCamera()
    {
        List<EnemyController> enemiesInView = new();
        var allEnemies = FindObjectsOfType<EnemyController>();
        var cam = Camera.main;
        if (cam == null) return enemiesInView;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            Vector3 viewportPos = cam.WorldToViewportPoint(enemy.transform.position);
            bool isInView = viewportPos.z > 0 && viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
            if (isInView)
                enemiesInView.Add(enemy);
        }

        return enemiesInView;
    }
}
