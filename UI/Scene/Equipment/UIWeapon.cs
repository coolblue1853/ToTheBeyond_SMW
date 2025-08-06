using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIWeapon : UIEquipment
{
    [SerializeField] private Image _activeIcon;
    [SerializeField] private TextMeshProUGUI _activeName;
    [SerializeField] private TextMeshProUGUI _activeCooltime;
    [SerializeField] private TextMeshProUGUI _activeDetail;

    [SerializeField] private Image _activeDIcon;
    [SerializeField] private TextMeshProUGUI _activeDName;
    [SerializeField] private TextMeshProUGUI _activeDCooltime;
    [SerializeField] private TextMeshProUGUI _activeDDetail;

    [SerializeField] private Image _activeFIcon;
    [SerializeField] private TextMeshProUGUI _activeFName;
    [SerializeField] private TextMeshProUGUI _activeFCooltime;
    [SerializeField] private TextMeshProUGUI _activeFDetail;
    
    public void SetDetail(PlayerWeaponHandler weaponHandler)
    {
        var weapon = weaponHandler.equippedWeapon;
        _icon.sprite = weapon.data.weaponIcon;
        _equipmentName.text = weapon.data.weaponName;
        _equipmentDetail.text = weapon.data.weaponDetail;

        
        // 액티브 스킬 등록
        var activeSkill = weapon.GetActiveSkillForSlot(SkillSlot.S);
        if (activeSkill != null)
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
        
        var activeSkillD = weapon.GetActiveSkillForSlot(SkillSlot.D);
        if (activeSkillD != null)
        {
            _activeDIcon.sprite = activeSkillD.iconSprite;
            _activeDName.text = activeSkillD.skillName;
            _activeDCooltime.text = activeSkillD.cooldown.ToString();
            _activeDDetail.text = activeSkillD.description;
        }
        else
        {
            _activeDIcon.sprite =null;
            _activeDName.text = "";
            _activeDCooltime.text = "";
            _activeDDetail.text = "";
        }
        
        var activeSkillF = weapon.GetActiveSkillForSlot(SkillSlot.F);
        if (activeSkillF != null)
        {       
            _activeFIcon.transform.parent.gameObject.SetActive(true);
            _activeFIcon.sprite = activeSkillF.iconSprite;
            _activeFName.text = activeSkillF.skillName;
            _activeFCooltime.text = activeSkillF.cooldown.ToString();
            _activeFDetail.text = activeSkillF.description;
        }
        else
        {
            _activeFIcon.sprite =null;
            _activeFName.text = "";
            _activeFCooltime.text = "";
            _activeFDetail.text = "";
            _activeFIcon.transform.parent.gameObject.SetActive(false);
        }
    }
}
