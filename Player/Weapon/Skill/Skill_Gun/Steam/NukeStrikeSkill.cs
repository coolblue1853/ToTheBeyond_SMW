using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic; 
using UnityEngine.InputSystem;
using DarkTonic.MasterAudio;

public class NukeStrikeSkill : HoldReleaseWeaponSkill
{
    // 증기를 소모하여 원하는 위치에 폭탄을 떨어뜨리는 스킬 
    [SerializeField] private float _requiredHeat = 30f;

    [Header("조준")]
    [SerializeField] private GameObject _markerPrefab;
    [SerializeField] private float _markerMoveSpeed = 5f;
    private GameObject _markerInstance;
    private Vector2 _aimInput;
    private Vector3 _targetPosition;

    [Header("낙하 연출")]
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private float _missileStartHeight = 15f;

    [Header("폭발 및 장판")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _damageZonePrefab;
    [SerializeField] private float _impactDelay = 1.0f;
    [SerializeField] private float _zoneDuration = 5f;
    [SerializeField] private float _explosionMultiplier = 2.0f;
    [SerializeField] private float _damageZoneMultiplier = 1.2f;

    [SerializeField] private LayerMask _groundMask;

    [Header("효과음")]
    [SerializeField] private string _introSfxName ;
    [SerializeField] private string _outroSfxName ;

    private void OnAim(InputValue value)
    {
        _aimInput = value.Get<Vector2>();
    }

    protected override void BeginSkill()
    {
        var controller = _owner.GetComponentInParent<PlayerController>();
        if (controller == null)
        {
            CancelSkill();
            return;
        }

        var armorHandler = controller.GetComponent<PlayerArmorHandler>();
        var steamArmor = armorHandler?.equippedArmor as SteamArmor;

        if (steamArmor == null || steamArmor.IsOverheated || steamArmor.CurrentHeat < _requiredHeat)
        {
            CancelSkill();
            return;
        }

        steamArmor.ReduceHeat(_requiredHeat);

        if (_markerPrefab != null)
        {
            _weapon.isMovementLocked = true;
            _targetPosition = _owner.position + Vector3.up * 2f;
            _markerInstance = Instantiate(_markerPrefab, _targetPosition, Quaternion.identity);
            MasterAudio.PlaySound(_introSfxName);
        }
    }

    private void CancelSkill()
    {
        _weapon.isUsingSkill = false;
        _weapon.isMovementLocked = false;
        _canExecute = false; // 쿨타임 등록을 막을 수 있습니다.
    }

    // 조준선 위치 지정 
    protected override void UpdateSkill()
    {
        if (_aimInput.sqrMagnitude > 0.01f && _markerInstance != null)
        {
            _targetPosition += (Vector3)_aimInput.normalized * _markerMoveSpeed * Time.deltaTime;

            // 화면 안에 제한
            Vector3 view = Camera.main.WorldToViewportPoint(_targetPosition);
            view.x = Mathf.Clamp01(view.x);
            view.y = Mathf.Clamp01(view.y);
            _targetPosition = Camera.main.ViewportToWorldPoint(view);
            _targetPosition.z = 0;

            _markerInstance.transform.position = _targetPosition;
        }
    }

    // 릴리즈시 해당 위치에 폭탄 투하 
    protected override void EndSkill()
    {
        if (_markerInstance != null)
            Destroy(_markerInstance);
        MarkExecuted();

        StartCoroutine(ExecuteNukeStrike(_targetPosition));
        StartCoroutine(DelayRoutine());
    }

    // 해당 위치로 폭탄을 떨어뜨리는 함수 
    private IEnumerator ExecuteNukeStrike(Vector3 targetPosition)
    {
        Vector2 groundPoint = FindNearestGround(targetPosition);

        // 미사일 생성 (카메라 밖 상단에서 떨어짐)
        Vector3 missileSpawn = new Vector3(groundPoint.x, Camera.main.transform.position.y + _missileStartHeight, 0f);
        if (_missilePrefab != null)
        {
            GameObject missile = Instantiate(_missilePrefab, missileSpawn, Quaternion.identity);
            StartCoroutine(MoveToGround(missile, groundPoint, () =>
            {
                ExplodeAt(groundPoint);
            }));
        }
        else
        {
            yield return new WaitForSeconds(_impactDelay);
            ExplodeAt(groundPoint);
        }
        
    }

    private IEnumerator MoveToGround(GameObject missile, Vector2 groundPoint, System.Action onImpact)
    {
        float duration = _impactDelay;
        Vector3 start = missile.transform.position;
        Vector3 end = groundPoint;

        float timer = 0f;
        while (timer < duration)
        {
            missile.transform.position = Vector3.Lerp(start, end, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        
        MasterAudio.PlaySound(_outroSfxName);
        Destroy(missile);
        onImpact?.Invoke();
    }

    // 폭팔 후 데미지 생성함수 
    private void ExplodeAt(Vector2 position)
    {
        if (_explosionPrefab != null)
        {
            GameObject explosion = Instantiate(_explosionPrefab, position, Quaternion.identity);
            InstantDamageEffect instantEffect = explosion.GetComponent<InstantDamageEffect>();

            // 폭발 이펙트의 yOffset 보정
            if (explosion.TryGetComponent<SpriteRenderer>(out var sprite))
            {
                float yOffset = sprite.bounds.size.y / 2f;
                explosion.transform.position += Vector3.up * yOffset;
            }
        
            var ranged = _weapon as RangedWeapon;
            if (ranged != null)
            {
                float[] baseDamage = new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel };
                var payload = _weapon.statProvider.CreatePayloadFrom(true);
                instantEffect.SetPayload(baseDamage, _explosionMultiplier, payload, true, false, 1f);
            }
        }

        // 지속 장판
        if (_damageZonePrefab != null)
        {
            GameObject zone = Instantiate(_damageZonePrefab, position, Quaternion.identity);
            var zoneEffect = zone.GetComponent<DamageZoneEffect>();

            // 장판 이펙트의 yOffset 보정
            if (zoneEffect != null && zone.TryGetComponent<SpriteRenderer>(out var zoneSprite))
            {
                float yOffset = zoneSprite.bounds.size.y / 2f;
                zone.transform.position += Vector3.up * yOffset;
            
                float[] baseDamage = new float[] { _skill.baseDamageByLevel, _skill.baseDamageByLevel };
                var payload = _weapon.statProvider.CreatePayloadFrom(true);
                zoneEffect.SetPayload(baseDamage, _damageZoneMultiplier, payload, true, false, false, 0, _zoneDuration);
            }

            Destroy(zone, _zoneDuration);
        }
    }

    // 조준선 기준오르 가장 가까운 Ground 를 찾는 함수 
    private Vector2 FindNearestGround(Vector2 origin)
    {
        List<Vector2> groundPoints = new();

        // 레이캐스트
        var downHits = Physics2D.RaycastAll(origin, Vector2.down, 30f, _groundMask);
        var upHits = Physics2D.RaycastAll(origin, Vector2.up, 30f, _groundMask);

        float originY = origin.y;

        // 아래는 그대로
        foreach (var hit in downHits)
        {
            if (IsInLayerMask(hit.collider.gameObject, _groundMask) && hit.point.y < originY)
            {
                groundPoints.Add(hit.point);
            }
        }

        // 위쪽은 bounds.max.y 보정 + origin보다 위에 있을 때만
        foreach (var hit in upHits)
        {
            if (IsInLayerMask(hit.collider.gameObject, _groundMask))
            {
                float topY = hit.collider.bounds.max.y;
                if (topY > originY)
                    groundPoints.Add(new Vector2(origin.x, topY));
            }
        }

        if (groundPoints.Count == 0)
            return origin;

        return groundPoints.OrderBy(p => Mathf.Abs(p.y - originY)).First();
    }

    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }

    public override bool Activate() => false;
}
