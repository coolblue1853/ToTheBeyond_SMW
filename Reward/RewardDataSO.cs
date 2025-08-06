using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RarityType
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}

public enum RewardType
{
    Experience,
    Attribute,
    ActiveItem,
    PassiveItem,
    Weapon
}
[CreateAssetMenu(fileName = "RewardData", menuName = "Reward/RewardData")]
public class RewardDataSO : ScriptableObject
{
    public string rewardName;
    public RewardType rewardType;
    public RarityType rarityType;
    public GameObject prefab; // 실제 인스턴스화할 프리팹
}
