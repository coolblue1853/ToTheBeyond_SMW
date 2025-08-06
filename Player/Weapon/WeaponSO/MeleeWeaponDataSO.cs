using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapon/MeleeWeaponData")]
public class MeleeWeaponDataSO : WeaponDataSO
{
    [System.Serializable]
    public class MeleeAttackEntry
    {
        public GameObject attackPrefab;
        public float delay;
        public float lifeTime;
        public bool lockMovement;
        public int pivotIndex; 
        public float attackSpawnDelay;
    }

    public List<MeleeAttackEntry> comboSequence;
    public float ComboResetDelay = 1.0f;
    
    [Header("Sounds Settings")]
    public string sfxWeaponName;
}