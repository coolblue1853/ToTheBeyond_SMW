using System;
using UnityEngine;

public class ExpController : MonoBehaviour
{
    [Header("경험치 설정")]
    [SerializeField] private int[] _expRequirements = { 100, 200 }; // 경험치 요구량 
    private const int _maxLevel = 3;  // 최대 레벨 
    private int _currentLevel = 1;
    private int _currentExp = 0;

    public int CurrentLevel => _currentLevel;
    public int CurrentExp => _currentExp;
    public int CurrentRequiredExp => _currentLevel < _maxLevel ? _expRequirements[_currentLevel - 1] : 0;
    public bool IsMaxLevel => _currentLevel >= _maxLevel;

    public Action OnExpChanged;
    private PlayerController _playerController;
    public ExpOrb CurrentOrb { get; private set; }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        ExpUIManager.Instance.Initialize(this);
    }

    // 경험치 증가
    public void AddExp(int amount)
    {
        if (_currentLevel >= _maxLevel)
            return;

        _currentExp += amount;

        while (_currentLevel < _maxLevel && _currentExp >= _expRequirements[_currentLevel - 1])
        {
            _currentExp -= _expRequirements[_currentLevel - 1];
            _currentLevel++;

            _playerController.UpdateLevel(_currentLevel);

            if (_currentLevel >= _maxLevel)
            {
                _currentExp = 0;
                break;
            }
        }

        OnExpChanged?.Invoke();
    }

    // 경험치 초기화 
    public void ResetExp()
    {
        _currentLevel = 1;
        _currentExp = 0;
        OnExpChanged?.Invoke();
    }

    public void SetNearbyOrb(ExpOrb orb)
    {
        CurrentOrb = orb;
    }

    public void ClearNearbyOrb(ExpOrb orb)
    {
        if (CurrentOrb == orb)
            CurrentOrb = null;
    }
}
