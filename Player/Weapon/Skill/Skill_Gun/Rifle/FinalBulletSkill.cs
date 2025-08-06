using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DarkTonic.MasterAudio;
public class FinalBulletSkill : HoldReleaseWeaponSkill
{
    // 조준경을 만들어서 저격하는 형태의 스킬 

    [Header("조준 및 타격")]
    [SerializeField] private GameObject _aimingPrefab;
    [SerializeField] private GameObject _impactPrefab;
    [SerializeField] private float _impactDuration = 0.5f;

    [Header("데미지 계산")]
    [SerializeField] private float _baseMultiplier = 1.0f;
    [SerializeField] private float _perAmmoBonus = 0.2f;

    private GameObject _aimingInstance;
    private Vector3 _targetPosition;
    
    private const float moveSpeed = 5f;
    private Vector2 _aimInput;
    
    [Header("효과음")] 
    [SerializeField] private string _introSfxName ;
    [SerializeField] private string _outroSfxName ;

    // 애니메이션 
    private PlayerAnimatorController _animController;
    [SerializeField] private string _introAnimStr;
    [SerializeField] private string _attackAnimStr;

    private void OnAim(InputValue value)
    {
        _aimInput = value.Get<Vector2>();
    }
    
    protected override void BeginSkill()
    {
        if (!(_weapon is RangedWeapon ranged)) return;

        if (!ranged.IsReady || ranged.CurrentAmmo <= 0)
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
        _weapon.isMovementLocked = true;
        _targetPosition = ranged.firePoint.position;

        if (_aimingPrefab != null)
            _aimingInstance = Instantiate(_aimingPrefab, _targetPosition, Quaternion.identity);
    }

    // 카메라 영역 내 조준선 위치 지정 
    protected override void UpdateSkill()
    {
        if (_aimInput.sqrMagnitude > 0.01f)
        {
            _targetPosition += (Vector3)_aimInput.normalized * moveSpeed * Time.deltaTime;

            Vector3 view = Camera.main.WorldToViewportPoint(_targetPosition);
            view.x = Mathf.Clamp01(view.x);
            view.y = Mathf.Clamp01(view.y);
            _targetPosition = Camera.main.ViewportToWorldPoint(view);
            _targetPosition.z = 0;

            if (_aimingInstance != null)
                _aimingInstance.transform.position = _targetPosition;
        }
    }

    // 스킬 종료시 해당 위치에 데미지 부여 
    protected override void EndSkill()
    {
        if (!(_weapon is RangedWeapon ranged)) return;
        if (!ranged.IsReady) return;

        int currentAmmo = ranged.CurrentAmmo;
        if (currentAmmo <= 0) return;

        MarkExecuted();
        ranged.PlayMuzzleVFX();

        float multiplier = _baseMultiplier + _perAmmoBonus * currentAmmo;
        float[] baseDamage = new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel };

        float finalDamage = _weapon.statProvider.CreatePayloadFrom(true).GetFinalDamage(
            baseDamage,
            multiplier, 
            isSkill: true,
            isMelee: _isMelee,
            isCrit: true);

        Collider2D[] hits = Physics2D.OverlapCircleAll(_targetPosition, 0.5f, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out var enemy))
            {
                enemy.TakeDamage(finalDamage, true);
            } 
        }

        if (_impactPrefab != null)
            Destroy(Instantiate(_impactPrefab, _targetPosition, Quaternion.identity), _impactDuration);

        if (_aimingInstance != null)
            Destroy(_aimingInstance);

        
        if (_animController != null && !string.IsNullOrEmpty(_attackAnimStr))
        {
            _animController.PlayUpperBodyAttackWithAutoReset(
                _attackAnimStr,
                _weapon.data.upperBodyIdleAnim,
                _delayTime
            );
        }
        
        MasterAudio.PlaySound(_outroSfxName);
        StartCoroutine(DelayConsumAmmoRoutine(ranged, currentAmmo));
        StartCoroutine(DelayRoutine());
    }

    IEnumerator DelayConsumAmmoRoutine(RangedWeapon ranged, int currentAmmo)
    {
        yield return new WaitForSeconds(_delayTime);
        ranged.ConsumeAmmo(currentAmmo);
    }


    public override bool Activate()
    {
        return false;
    }
}
