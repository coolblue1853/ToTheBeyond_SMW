using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    // 스킬 UI 매니저.

    public static SkillUIManager Instance;

    [SerializeField] private Transform weaponPanel;
    [SerializeField] private Transform armorPanel;
    [SerializeField] private SkillCooldownIcon iconPrefab;
    [SerializeField] private Image _backRenderer;
    
    private Dictionary<SkillSlot, SkillCooldownIcon> _weaponIcons = new();
    private Dictionary<SkillSlot, SkillCooldownIcon> _armorIcons = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        WeaponEvents.OnSkillUsed += StartCooldown;
        WeaponEvents.OnWeaponEquipped += UpdateWeaponIcons;
        WeaponEvents.OnArmorEquipped += UpdateArmorIcons;
    }

    private void OnDisable()
    {
        WeaponEvents.OnSkillUsed -= StartCooldown;
        WeaponEvents.OnWeaponEquipped -= UpdateWeaponIcons;
        WeaponEvents.OnArmorEquipped -= UpdateArmorIcons;
    }

    // 해당 슬롯에 쿨타임 시작 
    private void StartCooldown(SkillSlot slot, float cooldown)
    {
        if (_weaponIcons.TryGetValue(slot, out var icon))
        {
            icon.StartCooldown(cooldown);
        }
        else if (_armorIcons.TryGetValue(slot, out var iconArmor))
        {
            iconArmor.StartCooldown(cooldown);
        }
    }

    // 강제로 쿨타임 UI 변경 
    public void ForceCompleteCooldownUI(SkillSlot slot)
    {
        if (_weaponIcons.TryGetValue(slot, out var icon))
        {
            icon.ForceCompleteCooldown();
        }
        else if (_armorIcons.TryGetValue(slot, out var iconArmor))
        {
            iconArmor.ForceCompleteCooldown();
        }
    }

    public void UpdateWeaponIcons(Weapon weapon, int upgradeLevel)
    {
        ClearIcons(_weaponIcons, weaponPanel);

        // 슬롯별로 가장 높은 레벨 스킬만 추출
        var highestSkills = weapon.data.skills
            .Where(s => s.LevelRequired <= upgradeLevel && s.Skill != null)
            .GroupBy(s => s.Slot)
            .Select(g => g.OrderByDescending(s => s.LevelRequired).First());

        foreach (var skl in highestSkills)
        {
            var icon = Instantiate(iconPrefab, weaponPanel);
            icon.Initialize(GetBackForSkill(skl.Skill), GetIconForSkill(skl.Skill), skl.Skill.cooldown, skl.Slot);
            _weaponIcons[skl.Slot] = icon;

            if (weapon.TryGetSkillInstance(skl.Slot, out var instance) &&
                instance is StackableWeaponSkill stackSkill)
            {
                icon.UpdateStack(stackSkill.CurrentStack);
            }
        }
    }



    private void UpdateArmorIcons(Armor armor, int upgradeLevel)
    {
        ClearIcons(_armorIcons, armorPanel);

        foreach (var skl in armor.data.activeSkills)
        {
            if (skl.LevelRequired <= upgradeLevel && skl.Skill != null)
            {
                SkillCooldownIcon icon = Instantiate(iconPrefab, armorPanel);
                icon.Initialize( GetBackForSkill(skl.Skill),GetIconForSkill(skl.Skill),skl.Skill.cooldown, skl.Slot);
                _armorIcons[skl.Slot] = icon;
            }
        }

        if( armor.data.backSprite != null)
        {
            _backRenderer.sprite = armor.data.backSprite;
            _backRenderer.gameObject.SetActive(true);
        }
        else
        {
            _backRenderer.gameObject.SetActive(false);
        }

    }
    public void UpdateStackUI(SkillSlot slot, int current)
    {
        if (_weaponIcons.TryGetValue(slot, out var icon))
        {
            icon.UpdateStack(current);
        }
        else if (_armorIcons.TryGetValue(slot, out var iconArmor))
        {
            iconArmor.UpdateStack(current);
        }
    }
    public void StartStackRecoveryCooldownUI(SkillSlot slot, float cooldown)
    {
        if (_weaponIcons.TryGetValue(slot, out var icon))
        {
            icon.StartRecovery(cooldown);
        }
        else if (_armorIcons.TryGetValue(slot, out var iconArmor))
        {
            iconArmor.StartRecovery(cooldown);
        }
    }


    private void ClearIcons(Dictionary<SkillSlot, SkillCooldownIcon> dict, Transform panel)
    {
        foreach (var icon in dict.Values)
        {
            Destroy(icon.gameObject);
        }
        dict.Clear();
    }

    private Sprite GetIconForSkill(SkillSO skill)
    {
        return skill.iconSprite;
    }
    private Sprite GetBackForSkill(SkillSO skill)
    {
        return skill.iconBackSprite;
    }
}
