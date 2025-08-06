using System.Collections;
using System.Linq;
using UnityEngine;

public abstract class StackableWeaponSkill : WeaponSkill
{
    // 스택이 쌓이는 형태의 스킬
    // 현재는 UI 관리 파트도 섞여있는데 SkillUIManager로 분리 필요 

    private int _currentStacks;
    private bool _isRecharging;
    private Coroutine _rechargeCoroutine;
    private SkillSlot? _mySlot;

    public int CurrentStack => _currentStacks;
    public int MaxStack => _skill.maxStack;
    public bool isUpgraded;

    // 외부 초기화 
    public override void Initialize(SkillSO skillData, Transform ownerTransform)
    {
        base.Initialize(skillData, ownerTransform);
        _currentStacks = MaxStack;
        _mySlot = FindMySlotFromData();

        if (_mySlot.HasValue)
            SkillUIManager.Instance?.UpdateStackUI(_mySlot.Value, _currentStacks);
    }

    public override bool Activate()
    {
        if (_currentStacks <= 0) return false;

        bool result = UseStackSkill();
        if (result)
            ConsumeStack();

        return result;
    }

    // 스택 소모 함수 
    protected void ConsumeStack()
    {
        _currentStacks = Mathf.Max(0, _currentStacks - 1);

        if (_mySlot.HasValue)
            SkillUIManager.Instance?.UpdateStackUI(_mySlot.Value, _currentStacks);

        if (!_isRecharging)
        {
            _rechargeCoroutine = StartCoroutine(StackRechargeRoutine());
        }
    }

    // 소모된 스택을 다시 체우는 함수 
    private IEnumerator StackRechargeRoutine()
    {
        _isRecharging = true;

        while (_currentStacks < MaxStack)
        {
            if (_mySlot.HasValue)
                SkillUIManager.Instance?.StartStackRecoveryCooldownUI(_mySlot.Value, _skill.cooldown);

            float cooldown = _skill.cooldown;
            float elapsed = 0f;
            while (elapsed < cooldown)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            _currentStacks = Mathf.Min(_currentStacks + 1, MaxStack);

            if (_mySlot.HasValue)
                UpdateUIChecker();
        }

        _isRecharging = false;
        _rechargeCoroutine = null;
    }


    private void UpdateUIChecker()
    {
        if (isUpgraded != true) // isUpgraded == false 와 동일
        {
            SkillUIManager.Instance?.UpdateStackUI(_mySlot.Value, _currentStacks);
        }
    }

    public void OnUpgradeLevelChanged()
    {
        isUpgraded = true;
        if (_rechargeCoroutine != null)
        {
            StopCoroutine(_rechargeCoroutine);
            _rechargeCoroutine = null;
        }
        _isRecharging = false;

        _currentStacks = MaxStack;

        if (_mySlot.HasValue)
        {
            SkillUIManager.Instance?.UpdateStackUI(_mySlot.Value, _currentStacks);
            SkillUIManager.Instance?.ForceCompleteCooldownUI(_mySlot.Value);
        }
    }

    protected abstract bool UseStackSkill();

    // 스택 관리를 위한 슬롯 추출 
    private SkillSlot? FindMySlotFromData()
    {
        return _weapon.data.skills
            .Where(s => s.Skill == _skill)
            .Select(s => (SkillSlot?)s.Slot)
            .FirstOrDefault();
    }
    private void StopRechargeRoutine()
    {
        _isRecharging = false;

        if (_rechargeCoroutine != null)
        {
            StopCoroutine(_rechargeCoroutine);
            _rechargeCoroutine = null;
        }
    }
    private void UpdateStackUI()
    {
        if (_mySlot.HasValue)
            SkillUIManager.Instance?.UpdateStackUI(_mySlot.Value, _currentStacks);
    }
    public void OnUpgradeLevelChanged(SkillSO newSkillData)
    {
        StopRechargeRoutine(); // 스택 회복 중단
        _skill = newSkillData; // 새로운 스킬 데이터로 교체

        _currentStacks = newSkillData.maxStack;
        _isRecharging = false;

        UpdateStackUI();

        if (_mySlot.HasValue)
            SkillUIManager.Instance?.ForceCompleteCooldownUI(_mySlot.Value);
    }

}