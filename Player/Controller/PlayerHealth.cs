using UnityEngine;
using System;
using System.Collections;
public class PlayerHealth : MonoBehaviour
{
    private RuntimeStat _stat;
    private PlayerController _playerController;

    [SerializeField] private float _invincibleDuration = 1.0f; // 무적 시간 
    [SerializeField] private float _currentHealth;
    private bool _isInvincible = false;
    private bool _isDead = false;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _stat.MaxHealth;
    public bool IsInvincible => _isInvincible;

    public event Action OnMaxHealthChanged;
    public event Action OnDamaged;
    public event Action OnHealed;
    public event Action OnDeath;

    private bool _restoreFullOnMaxHealthChange = true;

    public void Init(PlayerController playerController)
    {
        _playerController  = playerController;
        _stat = _playerController.runtimeStat;
        _currentHealth = _stat.MaxHealth;
        _stat.OnStatChanged += HandleStatChanged;
        OnDeath += () => GameManager.Instance.ShowDeathUI();
    }

    private void OnDestroy()
    {
        if (_stat != null)
        {
            _stat.OnStatChanged -= HandleStatChanged;
        }
    }

    private void HandleStatChanged(StatType type)
    {
        if (type == StatType.MaxHealth)
        {
            _currentHealth = _restoreFullOnMaxHealthChange ? MaxHealth : Mathf.Clamp(_currentHealth, 0, MaxHealth);
            OnMaxHealthChanged?.Invoke();
        }
    }

    public void SetRestoreFullOnMaxHealthChange(bool value)
    {
        _restoreFullOnMaxHealthChange = value;
    }

    // 피격 함수 
    public void TakeDamage(float amount)
    {
        if (_isInvincible || amount <= 0f || _isDead) return;

        float damageMultiplier = 1f;

        if (_stat != null)
        {
            damageMultiplier += _stat.DamageTakenIncrease; // 예: 0.2f → 20% 증가
        }

        float finalDamage = amount * damageMultiplier;

        _playerController.effectController.PlayBlinkEffect(_invincibleDuration);
        _currentHealth -= finalDamage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
        OnDamaged?.Invoke();

        if (_currentHealth <= 0)
        {
            _isDead = true;
            _playerController.isControllable = false;
            _playerController.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            OnDeath?.Invoke();
        }
        else
        {
            StartCoroutine(StartInvincibility());
        }
    }

    // 회복 함수 
    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
        OnHealed?.Invoke();
    }

    public bool UseHealth(float amount)
    {
        if (_currentHealth >= amount)
        {
            _currentHealth -= amount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
            OnDamaged?.Invoke();
            return true;
        }
        return false;
    }

    // 무적시간 
    private IEnumerator StartInvincibility()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(_invincibleDuration);
        _isInvincible = false;
    }
    public void StartInvincibility(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(InvincibleCoroutine(duration));
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        _isInvincible = true;
        yield return new WaitForSeconds(duration);
        _isInvincible = false;
    }

    public void SetInvincible(bool value)
    {
        _isInvincible = value;
    }

    public void ResetHealth()
    {
        _currentHealth = MaxHealth;
        _isDead = false;           // 다시 살아났으므로 사망 상태 초기화
        OnHealed?.Invoke();        // UI 갱신용
    }

}
