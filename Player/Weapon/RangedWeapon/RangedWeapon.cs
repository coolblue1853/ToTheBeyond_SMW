 using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;

public abstract class RangedWeapon : Weapon
{
    //원거리 무기 기초

    [SerializeField] protected int _currentAmmo; // 현재 탄약 
    public int CurrentAmmo => _currentAmmo;
    protected float _lastFireTime;
    [SerializeField] protected bool _isReloading;
    protected Coroutine _reloadCoroutine; // 재장전 코루틴 
    protected Coroutine _fireRoutine;    // 사격 코루틴   
    protected bool _isCharging;
    protected float _chargeTime;

    public Transform firePoint;  // 공격이 나가는 피벗
    [SerializeField] protected bool _canDashReReload = false; // 대쉬 할 때 재장전이 되는가 
    public virtual bool IsReady => !_isReloading && _currentAmmo > 0 && _isEquiped;
    public bool IsReloading => _isReloading;
    public RangedWeaponDataSO RangedData => data as RangedWeaponDataSO;
    [SerializeField] private bool _isHandOffOnReoloadig = false; // 재장전시 손에서 땔것인가.
    [SerializeField] private Vector2 handOffeset;

    //VFX
    [SerializeField] private GameObject nozzleFleshVFX;
    [SerializeField] private Transform muzzleTransform;

    public override void Attack() { }
    protected virtual bool CanFire() =>
        !_isReloading
        && RangedData != null
        && playerStat != null
        && Time.time - _lastFireTime >= RangedData.fireRate / playerStat.AttackSpeed
        && _currentAmmo > 0
        && _isEquiped;

    public GameObject GetProjectile()
    {
        return ObjectPooler.SpawnFromPool(RangedData.projectilePrefab, firePoint.position, firePoint.rotation);
    }

    // 단일 발사 
    protected void FireSingleShot(Vector3 direction, float bulletSpeed)
    {
        GameObject bullet = GetProjectile();
        bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;

        var sr = bullet.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = (direction.x < 0);

        
        var projectile = bullet.GetComponent<Projectile>();
        projectile.SetPayload(
            RangedData.weaponBaseDamage,
            RangedData.weaponDamageMultiplier,
            statProvider.CreatePayloadFrom(isPiercing: RangedData.isPiercing),
            isSkill: false,
            isMelee: false
        );

        PlayMuzzleVFX();
        projectile.SetReturnReference(RangedData.projectilePrefab);
        projectile.StartAutoReturn(RangedData.bulletLifetime);
    }

    protected IEnumerator FireBurst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float baseAngle = RangedData.fireAngleOffset;
            if (_facingDirection == -1)
                baseAngle = 180f - baseAngle;

            float spread = RangedData.useFixedSpread
                ? -RangedData.spreadAngle / 2f + (RangedData.spreadAngle / Mathf.Max(1, count - 1)) * i
                : Random.Range(-RangedData.spreadAngle / 2f, RangedData.spreadAngle / 2f);

            float angle = baseAngle + spread;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * firePoint.right;


            float speed = Random.Range(
                RangedData.bulletSpeedMin * playerStat.BulletSpeed,
                RangedData.bulletSpeedMax * playerStat.BulletSpeed);

            FireSingleShot(direction, speed);

            if (RangedData.burstInterval > 0f)
                yield return new WaitForSeconds(RangedData.burstInterval / playerStat.AttackSpeed);
        }
    }

    // 재장전 
    protected IEnumerator ReloadRoutine()
    {
        _isReloading = true;

        animator?.SetReloading(true);

        Transform root = null;
        if (_isHandOffOnReoloadig) // 재장전시 애니메이션을 위해 손에서 무기를 때어야 하는 경우 
        {
            root = transform.parent;
            transform.SetParent(root.parent.parent);
            transform.localPosition = handOffeset;
        }
         
        float reloadDuration = RangedData.reloadTime / playerStat.AttackSpeed;
        animator.PlayUpperBodyReload(RangedData.upperBodyReloadAnim, reloadDuration);
        yield return new WaitForSeconds(RangedData.reloadTime / playerStat.AttackSpeed);

        if (_isHandOffOnReoloadig && root != null)
        {
            transform.SetParent(root);
            transform.localPosition = Vector2.zero;
        }
        
        _currentAmmo = Mathf.RoundToInt(RangedData.maxAmmo * playerStat.MaxAmmo);
        _isReloading = false;

        animator?.SetReloading(false,RangedData.upperBodyIdleAnim ); 
        _reloadCoroutine = null;
    }


    public virtual void Reload()
    {
        if (_isReloading || _reloadCoroutine != null) return;
        _reloadCoroutine = StartCoroutine(ReloadRoutine());
    }

    public override void OnDash()
    {
        base.OnDash();
        if (_canDashReReload)
        {
            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }
            animator?.SetReloading(false, RangedData.upperBodyIdleAnim);
            _currentAmmo = Mathf.RoundToInt(RangedData.maxAmmo * playerStat.MaxAmmo);
            _isReloading = false;
        }
    }

    // 원거리 무기 장착 
    public override void Equip(IDamageStatProvider statProvider, RuntimeStat runtimeStat, Transform ownerTransform)
    {
        base.Equip(statProvider, runtimeStat,ownerTransform);
        firePoint.localRotation = Quaternion.identity; // 회전 초기화
        firePoint.transform.SetParent(transform.root);
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        _currentAmmo = Mathf.RoundToInt(RangedData.maxAmmo * playerStat.MaxAmmo);
        _isReloading = false;

        AmmoUIManager.Instance.SetRangedWeapon(this);
    }

    // 원거리 무기 장착 해제 
    public override void Unequip()
    {
        firePoint.SetParent(transform);  // 다시 무기 자식으로

        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        _isReloading = false;
        animator.SetReloading(false);

        ObjectPooler.ReturnAllActiveObjects(nozzleFleshVFX);

        AmmoUIManager.Instance.Clear();
        base.Unequip();
    }

    public void ClampAmmoToMax()
    {
        int maxAmmo = Mathf.RoundToInt(RangedData.maxAmmo * playerStat.MaxAmmo);
        _currentAmmo = Mathf.Min(_currentAmmo, maxAmmo);
    }

    // 외부 사용을 위해서 탄환 소모 함수
    public virtual bool ConsumeAmmo(int amount)
    {
        if (_currentAmmo >= amount)
        {
            _currentAmmo -= amount;

            if (_currentAmmo <= 0)
                Reload();
            return true;
        }
        return false;
    }
    
    // 외부 강제 재장전
    public void ForceReloadToMax()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        animator.PlayUpperBodyAnimation(RangedData.upperBodyReloadAnim);

        _currentAmmo = Mathf.RoundToInt(RangedData.maxAmmo * playerStat.MaxAmmo);
        _isReloading = false;
    }

    // 탄매 효과 출력 
    public void PlayMuzzleVFX()
    {
        if (nozzleFleshVFX == null || muzzleTransform == null) return;
        GameObject vfx = ObjectPooler.SpawnFromPool(nozzleFleshVFX, muzzleTransform.position, muzzleTransform.rotation);

        if(vfx!= null)
        {
            var parent = transform.root;

            vfx.transform.SetParent(parent);

            // 방향 처리 (좌우 반전)
            Vector3 localScale = vfx.transform.localScale;
            localScale.x = Mathf.Abs(localScale.x);//* _facingDirection
            vfx.transform.localScale = localScale;

            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                StartCoroutine(DisableAfterTime(vfx, ps.main.duration));
            }
            else
            {
                StartCoroutine(DisableAfterTime(vfx, 0.5f)); // 안전 대체
            }
        }
      
    }


    private IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPooler.ReturnToPool(nozzleFleshVFX, obj);
    }
}