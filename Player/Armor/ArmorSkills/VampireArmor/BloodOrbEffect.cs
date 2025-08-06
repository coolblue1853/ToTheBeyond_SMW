using UnityEngine;

public class BloodOrbEffect : MonoBehaviour
{
    // 플레이어의 체력을 회복하는 회복구
    private PlayerHealth _target;
    private float _healAmount;
    private float _speed = 8f;
    private float _delay;

    private bool _isChasing = false;
    private float _maxHealCap = 0.6f;
    
    public void Initialize(PlayerHealth target, float healAmount)
    {
        _target = target;
        _healAmount = healAmount;

        // 0.3 ~ 0.5초 랜덤 대기 후 추적 시작
        _delay = Random.Range(0.3f, 0.5f);
        Invoke(nameof(StartChase), _delay);
    }

    private void StartChase()
    {
        if (_target == null) return;
        _isChasing = true;
    }

    private void Update()
    {
        if (!_isChasing || _target == null) return;

        // 부드럽게 추적
        Vector3 targetPos = _target.transform.position;
        float step = _speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        // 도착 처리
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if(_target.CurrentHealth + _healAmount < _maxHealCap * _target.MaxHealth)
                _target.Heal(_healAmount);
            Destroy(gameObject);
        }
    }
}