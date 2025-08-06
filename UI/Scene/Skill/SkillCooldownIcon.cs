using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownIcon : MonoBehaviour
{
    // 스킬 쿨타임 아이콘 
    public Image Icon;
    public Image CooldownOverlay;
    public Image skillSlotIcon;
    public TextMeshProUGUI StackText;

    private float _cooldownDuration;
    private float _cooldownStartTime;
    private float _cooldownEndTime;
    private bool _isActiveCooldown = false;

    [SerializeField] private Sprite _aIcon;
    [SerializeField] private Sprite _sIcon;
    [SerializeField] private Sprite _dIcon;
    [SerializeField] private Sprite _fIcon;

    public void Initialize(Sprite iconSprite, Sprite backSprite, float cooldown, SkillSlot skillSlot)
    {
        Icon.sprite = iconSprite;
        CooldownOverlay.sprite = backSprite;
        _cooldownDuration = cooldown;

        _isActiveCooldown = false;
        CooldownOverlay.fillAmount = 1f;


        switch (skillSlot)
        {
            case SkillSlot.A:
                skillSlotIcon.sprite = _aIcon;
                break;
            case SkillSlot.S:
                skillSlotIcon.sprite = _sIcon;
                break;
            case SkillSlot.D:
                skillSlotIcon.sprite = _dIcon;
                break;
            case SkillSlot.F:
                skillSlotIcon.sprite = _fIcon;
                break;
        }

        if (StackText != null)
            StackText.text = "";
    }

    public void StartRecovery(float duration)
    {
        StartCooldown(duration);
    }

    public void StartCooldown(float duration)
    {
        _cooldownDuration = duration;
        _cooldownStartTime = Time.time;
        _cooldownEndTime = Time.time + duration;
        _isActiveCooldown = true;
        CooldownOverlay.fillAmount = 0f;
    }

    public void UpdateStack(int current)
    {
        if (StackText == null) return;
        StackText.text = current > 0 ? current.ToString() : "";
    }

    public void ForceCompleteCooldown()
    {
        _isActiveCooldown = false;
        CooldownOverlay.fillAmount = 1f;
    }

    private void Update()
    {
        if (!_isActiveCooldown) return;

        float elapsed = Time.time - _cooldownStartTime;
        float ratio = Mathf.Clamp01(elapsed / _cooldownDuration);
        CooldownOverlay.fillAmount = ratio;

        if (Time.time >= _cooldownEndTime)
        {
            CooldownOverlay.fillAmount = 1f;
            _isActiveCooldown = false;
        }
    }
}