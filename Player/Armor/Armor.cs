using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Armor : MonoBehaviour
{   
    public PlayerController playerController;
    public ArmorDataSO data; // 방어구 SO
    public RuntimeStat playerStat;
    private PassiveArmorEffect _passiveEffect;

    private Dictionary<SkillSlot, float> _lastSkillUseTime = new(); // 쿨타임 체크를 위한 딕셔너리
    private Dictionary<SkillSlot, ArmorSkill> _skillInstances = new(); // 현재 보유 스킬 딕셔너리 
    public int upgradeLevel { get; private set; } = 1;


    // 방어구 장착
    public virtual void Equip(RuntimeStat stat)
    {
        playerStat = stat;

        SkillSO passive = GetSkill(data.passiveSkills);
        if (passive != null)
        {
            if (_passiveEffect != null)
            {
                _passiveEffect.RemoveAllModifiers();
                Destroy(_passiveEffect);
            }

            _passiveEffect = gameObject.GetComponent<PassiveArmorEffect>();
            if (_passiveEffect == null)
            {
                _passiveEffect = gameObject.AddComponent<PassiveArmorEffect>();
            }

            _passiveEffect.Initialize(passive, stat, upgradeLevel);
        }

        WeaponEvents.RaiseArmorEquipped(this, upgradeLevel);
    }

    //방어구 해제 
    public virtual void Unequip()
    {
        if (_passiveEffect != null)
        {
            _passiveEffect.RemoveAllModifiers();
            Destroy(_passiveEffect);
        }

        foreach (var kv in _skillInstances)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }

        _skillInstances.Clear();
        Destroy(this.gameObject);
    }

    // 방어구 레벨 업
    public void SetUpgradeLevel(int level)
    {
        upgradeLevel = Mathf.Clamp(level, 1, 3);

        if (_passiveEffect != null)
        {
            _passiveEffect.Reinitialize(upgradeLevel);
        }
        else
        {
            SkillSO passive = GetSkill(data.passiveSkills);
            if (passive != null)
            {
                _passiveEffect = gameObject.AddComponent<PassiveArmorEffect>();
                _passiveEffect.Initialize(passive, playerStat, upgradeLevel);
            }
        }
    }

    // 레벨에 따른 스킬 발동
    public void ActivateSkill(int index)
    {
        SkillSlot slot = (SkillSlot)index;

        SkillPerLevel skillData = data.activeSkills
            .Where(s => s.Slot == slot && s.LevelRequired <= upgradeLevel)
            .OrderByDescending(s => s.LevelRequired)
            .FirstOrDefault();

        if (skillData == null || skillData.Skill == null) return;

        SkillSO skill = skillData.Skill;

        if (_lastSkillUseTime.TryGetValue(slot, out float lastTime) &&
            Time.time < lastTime + skill.cooldown)
        {
            return;
        }

        if (!_skillInstances.TryGetValue(slot, out ArmorSkill skillInstance) ||
            skillInstance == null || skillInstance.name != skill.skillName)
        {
            ArmorSkill newSkill = CreateSkillInstance(skill, transform);
            if (newSkill != null)
            {
                newSkill.Initialize(skill, transform, this, upgradeLevel);
                _skillInstances[slot] = newSkill;
                skillInstance = newSkill;
            }
        }

        if (skillInstance != null && skillInstance.Activate())
        {
            _lastSkillUseTime[slot] = Time.time;
            WeaponEvents.RaiseSkillUsed(slot, skill.cooldown);
        }
    }

    //
    private static ArmorSkill CreateSkillInstance(SkillSO skill, Transform owner)
    {
        if (skill.skillLogicPrefab == null)
        {
            return null;
        }

        GameObject instance = Instantiate(skill.skillLogicPrefab, owner);
        return instance.GetComponent<ArmorSkill>();
    }

    // 스킬 리스트를 가져오기
    private SkillSO GetSkill(List<SkillPerLevel> list)
    {
        return list
            .Where(s => s.LevelRequired <= upgradeLevel)
            .OrderByDescending(s => s.LevelRequired)
            .Select(s => s.Skill)
            .FirstOrDefault();
    }

    public SkillSO GetPassiveSkillForCurrentLevel()
    {
        return data.passiveSkills
            .Where(s => s.Slot == SkillSlot.Armor && s.LevelRequired <= upgradeLevel)
            .OrderByDescending(s => s.LevelRequired)
            .Select(s => s.Skill)
            .FirstOrDefault();
    }

    public SkillSO GetActiveSkillForSlot(SkillSlot slot)
    {
        return data.activeSkills
            .Where(s => s.Slot == slot && s.LevelRequired <= upgradeLevel)
            .OrderByDescending(s => s.LevelRequired)
            .Select(s => s.Skill)
            .FirstOrDefault();
    }

    public SkillSO GetActiveSkillForSlot(SkillSlot slot, int level)
    {
        return data.activeSkills
            .Where(s => s.Slot == slot && s.LevelRequired <= level)
            .OrderByDescending(s => s.LevelRequired)
            .Select(s => s.Skill)
            .FirstOrDefault();
    }
}
