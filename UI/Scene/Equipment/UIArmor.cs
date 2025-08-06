using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIArmor : UIEquipment
{
    [SerializeField] private Image _passiveIcon;
    [SerializeField] private TextMeshProUGUI _passiveName;
    [SerializeField] private TextMeshProUGUI _passiveDetail;
    
    [SerializeField] private Image _activeIcon;
    [SerializeField] private TextMeshProUGUI _activeName;
    [SerializeField] private TextMeshProUGUI _activeCooltime;
    [SerializeField] private TextMeshProUGUI _activeDetail;


    public void SetDetail(PlayerArmorHandler armorHandler)
    {
        var armor = armorHandler.equippedArmor;
        _icon.sprite = armor.data.armorSprite;
        _equipmentName.text = armor.data.armorName;
        _equipmentDetail.text = armor.data.armorDescription;

        // 패시브 스킬 등록
        var skill = armor.GetPassiveSkillForCurrentLevel();

        if (skill != null)
        {
            _passiveIcon.sprite = skill.iconSprite;
            _passiveName.text = skill.skillName;
            _passiveDetail.text = skill.description;
        }
        else
        {
            _passiveIcon.sprite = null;
            _passiveName.text = "";
            _passiveDetail.text = "";
        }
        
        // 액티브 스킬 등록
        var activeSkill = armor.GetActiveSkillForSlot(SkillSlot.A);
        if (skill != null)
        {
            _activeIcon.sprite = activeSkill.iconSprite;
            _activeName.text = activeSkill.skillName;
            _activeCooltime.text = activeSkill.cooldown.ToString();
            _activeDetail.text = activeSkill.description;
        }
        else
        {
            _activeIcon.sprite =null;
            _activeName.text = "";
            _activeCooltime.text = "";
            _activeDetail.text = "";
        }
    }
}
