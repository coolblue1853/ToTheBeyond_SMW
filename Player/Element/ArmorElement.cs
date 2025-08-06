using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmorElement : MonoBehaviour
{
    // 방어구 속성 관리자 
    public Armor armorComponent;
    public ArmorElementType _type;
    public SpriteRenderer spriteRenderer;

    private PlayerArmorHandler _currentHandler;
    private float _checkInterval = 0.2f;
    private float _checkTimer = 0f;

    // 아이템 접촉시 상세 디테일 UI 
    [SerializeField] private GameObject _root;
    [SerializeField] protected Image _icon;
    [SerializeField] protected TextMeshProUGUI _equipmentName;
    [SerializeField] protected TextMeshProUGUI _equipmentDetail;

    [SerializeField] private Image _passiveIcon;
    [SerializeField] private TextMeshProUGUI _passiveName;
    [SerializeField] private TextMeshProUGUI _passiveDetail;

    [SerializeField] private Image _activeIcon;
    [SerializeField] private TextMeshProUGUI _activeName;
    [SerializeField] private TextMeshProUGUI _activeCooltime;
    [SerializeField] private TextMeshProUGUI _activeDetail;

    // 접촉시 UI 출력 트리거  
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerArmorHandler handler))
        {
            handler.nearbyArmor = this;
            _currentHandler = handler;
            SetDetail(handler.elementController, handler.playerController.expController.CurrentLevel);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerArmorHandler handler))
        {
            if (handler.nearbyArmor == this)
            {
                handler.nearbyArmor = null;
                _currentHandler = null;
                OffDetail();
            }
        }
    }

    // 트리거 프레임 체크로 인해 UI가 꺼지지 않을때를 위한 안전장치 
    private void Update()
    {
        if (_root != null && _root.activeSelf && _currentHandler != null)
        {
            _checkTimer -= Time.deltaTime;
            if (_checkTimer <= 0f)
            {
                _checkTimer = _checkInterval;

                Collider2D myCollider = GetComponent<Collider2D>();
                Collider2D playerCollider = _currentHandler.GetComponent<Collider2D>();

                if (myCollider != null && playerCollider != null)
                {
                    if (!myCollider.IsTouching(playerCollider))
                    {
                        _currentHandler.nearbyArmor = null;
                        _currentHandler = null;
                        OffDetail();
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        OffDetail();
        _currentHandler = null;
    }

    public void OffDetail()
    {
        if (_root == null)
            return;
        _root.SetActive(false);
    }

    // UI 정보 할당 
    public void SetDetail(ElementController elementController, int level)
    {
        if (_root == null)
            return;

        var armor = elementController.armorDictionary[_type].armorComponent;
        _icon.sprite = armor.data.armorSprite;
        _equipmentName.text = armor.data.armorName;
        _equipmentDetail.text = armor.data.armorDescription;

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

        var activeSkill = armor.GetActiveSkillForSlot(SkillSlot.A, level);
        if (activeSkill != null)
        {
            _activeIcon.sprite = activeSkill.iconSprite;
            _activeName.text = activeSkill.skillName;
            _activeCooltime.text = activeSkill.cooldown.ToString();
            _activeDetail.text = activeSkill.description;
        }
        else
        {
            _activeIcon.sprite = null;
            _activeName.text = "";
            _activeCooltime.text = "";
            _activeDetail.text = "";
        }

        _root.SetActive(true);
    }
}
