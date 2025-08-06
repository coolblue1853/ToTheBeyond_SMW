using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/RangedWeaponData")]
public class RangedWeaponDataSO : WeaponDataSO
{
    [Header("원거리 무기")]
    public int maxAmmo;
    public float reloadTime;
    public float fireRate;
    public float bulletSpeedMin = 10f;
    public float bulletSpeedMax = 15; 
    public float bulletLifetime = 3;
    public GameObject projectilePrefab;


    [Header("Bullet Pattern")]
    public bool isPiercing = false;
    public int bulletsPerShot = 1;
    public float spreadAngle = 0f;
    public bool useFixedSpread = false;
    public float burstInterval = 0.01f;

    [Header("Fire Direction")]
    public float fireAngleOffset = 0f; 

    [Header("Charge Settings")]
    public bool isChargeWeapon = false;
    public float maxChargeTime = 1.5f; 
    public AnimationCurve chargeCurve;
    
    [Header("Sounds Settings")]
    public string sfxWeaponName;
    
    [Header("기본 공격 애니메이션")]
    public string upperBodyAttackAnim;
    public string upperBodyReloadAnim;

}
