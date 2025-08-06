using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;
public class SteamTurret : MonoBehaviour
{
    // 열기를 소모하여 생성된 터렛 

    [Header("탄환 설정")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _fireInterval = 0.5f;
    [SerializeField] private float _baseDamage = 5f;
    [SerializeField] private bool _isMelee;
    
    private IDamageStatProvider _statProvider;
    private float _damageMultiplier;
    private int _direction;
    private float _duration;

    private Coroutine _attackRoutine;

    [SerializeField] private string _setTurretSfxName;
    [SerializeField] private string _shootSfxName;


    // 외부 주입 
    public void Initialize(IDamageStatProvider statProvider, int direction, float damageMultiplier, float duration)
    {
        if (statProvider == null)
        {
            return;
        }

        MasterAudio.PlaySound(_setTurretSfxName);
        _statProvider = statProvider;
        _direction = direction;
        _damageMultiplier = damageMultiplier;
        _duration = duration;

        transform.localScale = new Vector3(direction, 1f, 1f); // 방향 반영

        if (_attackRoutine != null)
            StopCoroutine(_attackRoutine);
        _attackRoutine = StartCoroutine(AttackRoutine());

        StartCoroutine(AutoDisable(_duration));
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            Fire();
            yield return new WaitForSeconds(_fireInterval);
        }
    }

    private void Fire()
    {
        GameObject bullet = ObjectPooler.SpawnFromPool(_projectilePrefab, _firePoint.position, Quaternion.identity);
        if (bullet == null) return;

        Vector2 directionVec = Vector2.right * _direction;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = directionVec.normalized * _bulletSpeed;

        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            float damage = _baseDamage * _damageMultiplier;

            proj.SetPayload(
                new float[] { damage, damage },
                1f,
                _statProvider.CreatePayloadFrom(true),
                isSkill: true,
                isMelee: _isMelee
            );

            proj.SetReturnReference(_projectilePrefab);
            proj.StartAutoReturn(2f);
        }
        MasterAudio.PlaySound(_shootSfxName);
        var sr = bullet.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = (_direction < 0);
    }

    private IEnumerator AutoDisable(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
        gameObject.SetActive(false); // 오브젝트 풀 사용 전제
    }
}
