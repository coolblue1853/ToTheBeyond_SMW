using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RewardCountChance
{
    public int count;
    public float probability;
}

public class RewardManager : MonoBehaviour
{
    // 던전 클리어시 보상을 반환하는 관리자 

    public static RewardManager Instance;

    [Header("보상 데이터")]
    [SerializeField] private List<RewardDataSO> _rewardPool;

    [Header("보상 스폰 위치 및 간격")]
    [SerializeField] private float _spacing = 0.5f;

    [Header("등장 갯수 확률 (합은 1.0이 되어야 함)")]
    [SerializeField] private List<RewardCountChance> _rewardCountChances = new List<RewardCountChance>
    {
        new RewardCountChance { count = 1, probability = 0.6f },
        new RewardCountChance { count = 2, probability = 0.3f },
        new RewardCountChance { count = 3, probability = 0.1f },
    };

    [Header("등급별 등장 확률 (합은 1.0이 아니어도 됨)")]
    private Dictionary<RarityType, float> _rarityChances = new Dictionary<RarityType, float>
    {
        { RarityType.Common, 0.5f },     // 50%
        { RarityType.Rare, 0.25f },      // 25%
        { RarityType.Unique, 0.18f },    // 18%
        { RarityType.Epic, 0.05f },      // 5%
        { RarityType.Legendary, 0.02f }  // 2%
    };

    [SerializeField] private bool _forceAtLeastOneWeapon = true;

    private int _seed = 1111;
    private System.Random _rand;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        LoadRewardPool(); // 자동 등록 추가
    }
    private void LoadRewardPool()
    {
        _rewardPool = Resources.LoadAll<RewardDataSO>("Reward").ToList();
    }
    public void SetRewardSpawnTransform(Transform spawnTransform)
    {
        //_rewardSpawnTransform = spawnTransform;
    }

    // 보상 생성 함수 
    public void GenerateRewards(Transform spawnTransform)
    {
        _seed = System.DateTime.Now.Millisecond + UnityEngine.Random.Range(0, 10000);
        _rand = new System.Random(_seed);

        int rewardCount = GetRandomRewardCount();
        List<RewardDataSO> selectedRewards = new List<RewardDataSO>();
        List<RewardDataSO> availablePool = _rewardPool.ToList();

        for (int i = 0; i < rewardCount; i++)
        {
            RarityType selectedRarity = GetRandomRarity();

            List<RewardDataSO> candidates = availablePool
                .Where(r => r.rarityType == selectedRarity).ToList();

            if (candidates.Count == 0)
                candidates = availablePool.Where(r => r.rarityType == RarityType.Rare).ToList();
            if (candidates.Count == 0) continue;

            RewardDataSO chosen = candidates[_rand.Next(candidates.Count)];
            selectedRewards.Add(chosen);
            availablePool.Remove(chosen);
        }

        // 최소 1개의 Weapon 타입 보상이 포함되도록 강제
        if (_forceAtLeastOneWeapon && !selectedRewards.Any(r => r.rewardType == RewardType.Weapon))
        {
            var weaponCandidates = _rewardPool
                .Where(r => r.rewardType == RewardType.Weapon && !selectedRewards.Contains(r))
                .ToList();

            if (weaponCandidates.Count > 0)
            {
                var forcedWeapon = weaponCandidates[_rand.Next(weaponCandidates.Count)];

                // 보상 수 초과 방지 - 가장 낮은 희귀도 보상을 제거
                if (selectedRewards.Count >= rewardCount)
                {
                    var toRemove = selectedRewards
                        .OrderBy(r => _rarityChances.ContainsKey(r.rarityType) ? _rarityChances[r.rarityType] : 0)
                        .FirstOrDefault();

                    if (toRemove != null)
                        selectedRewards.Remove(toRemove);
                }

                selectedRewards.Add(forcedWeapon);
            }
        }

        SpawnRewards(selectedRewards, spawnTransform);
    }


    // 등장할 보상의 갯수 
    private int GetRandomRewardCount()
    {
        float roll = (float)_rand.NextDouble();
        float total = 0f;

        foreach (var entry in _rewardCountChances.OrderBy(e => e.probability))
        {
            total += entry.probability;
            if (roll <= total)
                return entry.count;
        }

        return 1; // fallback
    }

    // 등장할 보상의 레어도 
    private RarityType GetRandomRarity()
    {
        float roll = (float)_rand.NextDouble();
        float total = 0f;

        foreach (var kvp in _rarityChances.OrderBy(kvp => kvp.Value))
        {
            total += kvp.Value;
            if (roll <= total)
                return kvp.Key;
        }

        return RarityType.Rare;
    }

    private void SpawnRewards(List<RewardDataSO> rewards, Transform spawnTransform)
    {
        if (spawnTransform == null)
        {
            return;
        }

        Transform parentMap = spawnTransform.parent; // 맵의 루트로 설정
        Vector3 startPos = spawnTransform.position - Vector3.right * _spacing * (rewards.Count - 1) / 2f;

        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i].prefab != null)
            {
                GameObject reward = Instantiate(
                    rewards[i].prefab, 
                    startPos + Vector3.right * _spacing * i, 
                    Quaternion.identity,
                    parentMap //  보상 오브젝트를 맵의 자식으로 둠
                );
            }
        }
    }

}



