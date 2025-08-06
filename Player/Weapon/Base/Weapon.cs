using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WeaponKind { Ranged, Melee }
public enum DamageType { Physical, Magical }
public enum WeaponCategory { Gun, Bow, Staff, Sword, Greatsword, Hammer, Claw }
public enum SkillSlot { Armor, S, D, F, A }

public abstract class Weapon : MonoBehaviour
{
    // 기초 컴포넌트 
    public PlayerController  playerController;
    public WeaponDataSO data;
    public WorldWeapon worldWeaponRef; 
    public RuntimeStat playerStat;
    public IDamageStatProvider statProvider;
    public int UpgradeLevel { get; protected set; } = 1;

    [SerializeField] protected bool _isEquiped = false;
    protected int _facingDirection = 1;
    public int FacingDirection => _facingDirection;
    protected Transform _ownerTransform;

    // 스킬 관련
    protected Dictionary<SkillSlot, float> _lastSkillUseTime = new();
    private Dictionary<SkillSlot, WeaponSkill> _skillInstances = new();
    public bool isUsingSkill = false;
    public bool isMovementLocked = false;
    public PlayerAnimatorController animator;
    public bool CanAttack() => !isUsingSkill;

    // 무기 레이어
    public const int handLayer = 45;
    public bool isUpperHand = false;
    [SerializeField] private SpriteRenderer rightHandWeaopn;
    [SerializeField] private SpriteRenderer LeftHandWeaopn;
    private Transform _leftHandWeaponOriginalParent;
    [SerializeField] private Vector2 _leftHandOffset;
    private Vector2 _leftHandOriginalOffset;

    [SerializeField] private bool _isChenageWeaponSpriteMode = false;

    public Vector2 worldOffset;


    public abstract void Attack();

    // 스킬 인풋 즉시 입력되는 것과 홀드 릴리즈로 구분 
    public void HandleSkillInput(SkillSlot slot, bool isDown)
    {
        EnsureSkillInstance(slot);

        if (!_skillInstances.TryGetValue(slot, out WeaponSkill skillInstance)) return;

        SkillSO skill = skillInstance.GetSkillData();
        if (skill == null) return;

        if (isUsingSkill && isDown) return;

        switch (skill.inputType)
        {
            case SkillInputType.Instant:
                if (isDown)
                    TryActivateSkill(slot, skillInstance, skill);
                break;

            case SkillInputType.Hold:
                if (isDown)
                    skillInstance.OnSkillHold();
                else
                {
                    skillInstance.OnSkillRelease();
                    TryActivateSkill(slot, skillInstance, skill);
                }
                break;
        }
    }

    // 스킬 발동 체크 
    private void TryActivateSkill(SkillSlot slot, WeaponSkill instance, SkillSO skill)
    {
        if (!(instance is StackableWeaponSkill))
        {
            if (_lastSkillUseTime.TryGetValue(slot, out float lastTime) &&
                Time.time < lastTime + skill.cooldown)
            {
                return;
            }
        }

        if (instance.Activate())
        {
            if (!(instance is StackableWeaponSkill))
            {
                _lastSkillUseTime[slot] = Time.time;
                WeaponEvents.RaiseSkillUsed(slot, skill.cooldown);
            }
        }
    }

    private void EnsureSkillInstance(SkillSlot slot)
    {
        if (_skillInstances.ContainsKey(slot)) return;

        SkillPerLevel skillData = data.skills
            .Where(s => s.Slot == slot && s.LevelRequired <= UpgradeLevel)
            .OrderByDescending(s => s.LevelRequired)
            .FirstOrDefault();

        if (skillData == null || skillData.Skill == null) return;

        SkillSO skill = skillData.Skill;
        WeaponSkill newSkill = CreateSkillInstance(skill, transform);
        if (newSkill != null)
        {
            newSkill.Initialize(skill, transform);
            _skillInstances[slot] = newSkill;

            if (newSkill is StackableWeaponSkill stackSkill && skill.type == SkillType.Stack)
            {
                SkillUIManager.Instance?.UpdateStackUI(slot, stackSkill.CurrentStack);
            }
        }
    }

    // 실제 스킬 발동 
    public virtual void ActivateSkill(int index)
    {
        if (!CanAttack()) return;
        SkillSlot slot = (SkillSlot)index;

        EnsureSkillInstance(slot);

        if (_skillInstances.TryGetValue(slot, out var skillInstance))
        {
            SkillSO skill = skillInstance.GetSkillData();
            if (skill.inputType == SkillInputType.Instant)
            {
                HandleSkillInput(slot, true);
            }
        }
    }

    private static WeaponSkill CreateSkillInstance(SkillSO skill, Transform owner)
    {
        if (skill.skillLogicPrefab == null)
        {
            return null;
        }

        GameObject instance = GameObject.Instantiate(skill.skillLogicPrefab, owner);
        return instance.GetComponent<WeaponSkill>();
    }

    // 레벨 업데이트
    public void SetUpgradeLevel(int level)
    {
        UpgradeLevel = Mathf.Clamp(level, 1, 3);

        foreach (SkillSlot slot in System.Enum.GetValues(typeof(SkillSlot)))
        {
            if (slot == SkillSlot.Armor)
                continue;

            SkillPerLevel skillData = data.skills
                .Where(s => s.Slot == slot && s.LevelRequired <= UpgradeLevel)
                .OrderByDescending(s => s.LevelRequired)
                .FirstOrDefault();

            if (skillData == null || skillData.Skill == null)
                continue;

            SkillSO newSkill = skillData.Skill;

            if (_skillInstances.TryGetValue(slot, out var instance))
            {
                Destroy(instance.gameObject);
                WeaponSkill newInstance = CreateSkillInstance(newSkill, transform);
                if (newInstance != null)
                {
                    newInstance.Initialize(newSkill, transform);
                    _skillInstances[slot] = newInstance;
                }
            }
            else
            {
                WeaponSkill newInstance = CreateSkillInstance(newSkill, transform);
                if (newInstance != null)
                {
                    newInstance.Initialize(newSkill, transform);
                    _skillInstances[slot] = newInstance;
                }
            }
        }

        SkillUIManager.Instance?.UpdateWeaponIcons(this, UpgradeLevel);

        isUsingSkill = false;
        isMovementLocked = false;

        if (this is MeleeWeapon melee)
            melee.isDashing = false;
    }


    // 장비 장착 
    public virtual void Equip(IDamageStatProvider statProvider, RuntimeStat runtimeStat, Transform ownerTransform)
    {
        _isEquiped = true;
        this.statProvider = statProvider;
        playerStat = runtimeStat;
        _ownerTransform = ownerTransform;
        animator = playerController?.GetComponent<PlayerAnimatorController>();

        
        if (_isChenageWeaponSpriteMode)
        {
            playerController._weaponHandler.rightWeaponSprtieRenderer.sprite =  rightHandWeaopn.sprite;
            rightHandWeaopn.gameObject.SetActive(false);
            if (LeftHandWeaopn != null)
            {
  
                playerController._weaponHandler.leftWeaponSprtieRenderer.sprite =  LeftHandWeaopn.sprite;
                LeftHandWeaopn.gameObject.SetActive(false);
            }
        }
        else
        {
            if (LeftHandWeaopn != null)
            {

                SetLeftHandSprite();
            }
        }

        foreach (SkillSlot slot in System.Enum.GetValues(typeof(SkillSlot)))
        {
            if (slot != SkillSlot.Armor)
                EnsureSkillInstance(slot);
        }

        if (isUpperHand)
        {
            rightHandWeaopn.sortingOrder = handLayer + 1;
        }

        WeaponEvents.RaiseWeaponEquipped(this, UpgradeLevel);
    }

    private void SetLeftHandSprite()
    {
        if (LeftHandWeaopn != null)
        {
    
            _leftHandOriginalOffset = LeftHandWeaopn.transform.localPosition;
            _leftHandWeaponOriginalParent = LeftHandWeaopn.transform.parent;

            LeftHandWeaopn.transform.SetParent(playerController._weaponHandler.leftWeaponHolder, false);
            LeftHandWeaopn.transform.localPosition = _leftHandOffset;
        }
    }
    
    // 장비 장착 해제 
    public virtual void Unequip()
    {
        _isEquiped = false;
        statProvider = null;

        
        if (_isChenageWeaponSpriteMode)
        {
            playerController._weaponHandler.rightWeaponSprtieRenderer.sprite =  null;
            rightHandWeaopn.gameObject.SetActive(true);
            if (LeftHandWeaopn != null)
            {
  
                playerController._weaponHandler.leftWeaponSprtieRenderer.sprite =  null;
                LeftHandWeaopn.gameObject.SetActive(true);
            }
        }
        else
        {
            if (LeftHandWeaopn != null && _leftHandWeaponOriginalParent != null)
            {
                LeftHandWeaopn.transform.SetParent(_leftHandWeaponOriginalParent);
                _leftHandWeaponOriginalParent = null;
                LeftHandWeaopn.transform.localPosition = _leftHandOriginalOffset;
            }
        }


    }


    public virtual void OnDash() { }

    public void SetDirection(int direction)
    {
        _facingDirection = direction;
    }

    public void RegisterSkillCooldown(SkillSlot slot, float cooldown)
    {
        if (!_skillInstances.ContainsKey(slot)) return;

        float now = Time.time;
        _lastSkillUseTime[slot] = now;
        WeaponEvents.RaiseSkillUsed(slot, cooldown);
    }

    public bool IsSkillOnCooldown(SkillSlot slot, float cooldown)
    {
        if (_lastSkillUseTime.TryGetValue(slot, out float lastTime))
        {
            return Time.time < lastTime + cooldown;
        }
        return false;
    }

    public bool TryGetSkillInstance(SkillSlot slot, out WeaponSkill skill)
    {
        return _skillInstances.TryGetValue(slot, out skill);
    }

    // 현재 액티브 가능한 스킬을 가져오기 
    public SkillSO GetActiveSkillForSlot(SkillSlot slot)
    {
        return data.skills
            .Where(s => s.Slot == slot && s.LevelRequired <= UpgradeLevel)
            .OrderByDescending(s => s.LevelRequired)
            .Select(s => s.Skill)
            .FirstOrDefault();
    }
    public SkillSO GetActiveSkillForSlot(SkillSlot slot, int level)
    {
        return data.skills
            .Where(s => s.Slot == slot && s.LevelRequired <= level)
            .OrderByDescending(s => s.LevelRequired)
            .Select(s => s.Skill)
            .FirstOrDefault();
    }

}
