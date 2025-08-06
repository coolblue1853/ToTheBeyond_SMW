using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class WorldWeapon : MonoBehaviour
{
    public Weapon weaponComponent;
    public GameObject leftWeaponPrefab = null;

    [SerializeField] private GameObject _root;
    [SerializeField] protected Image _icon;
    [SerializeField] protected TextMeshProUGUI _equipmentName;
    [SerializeField] protected TextMeshProUGUI _equipmentDetail;
    
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

    private PlayerWeaponHandler _currentHandler;
    private float _checkInterval = 0.2f;
    private float _checkTimer = 0f;



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerWeaponHandler handler) && weaponComponent != null)
        {
            handler.nearbyWeapon = this;
            _currentHandler = handler; // 추가
            SetDetail(handler.playerController.expController.CurrentLevel);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerWeaponHandler handler))
        {
            if (handler.nearbyWeapon == this)
            {
                handler.nearbyWeapon = null;
                _currentHandler = null; // 추가
                OffDetail();
            }
        }
    }

    private void Update()
    {
        if (_root != null && _root.activeSelf && _currentHandler != null)
        {
            _checkTimer -= Time.deltaTime;
            if (_checkTimer <= 0f)
            {
                _checkTimer = _checkInterval;

                Collider2D collider = GetComponent<Collider2D>();
                Collider2D playerCollider = _currentHandler.GetComponent<Collider2D>();

                if (collider != null && playerCollider != null)
                {
                    if (!collider.IsTouching(playerCollider))
                    {
                        // 강제로 Detail 끔
                        _currentHandler.nearbyWeapon = null;
                        _currentHandler = null;
                        OffDetail();
                    }
                }
            }
        }
    }


    public void OffDetail()
    {    if(_root == null)
            return;
        _root.SetActive(false);
    }
    public void SetDetail(int level)
    {
        if(_root == null)
            return;
        
        var weapon = weaponComponent;
        _icon.sprite = weapon.data.weaponIcon;
        _equipmentName.text = weapon.data.weaponName;
        _equipmentDetail.text = weapon.data.weaponDetail;


        // 액티브 스킬 등록
        var activeSkill = weapon.GetActiveSkillForSlot(SkillSlot.S, level);
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

        var activeSkillD = weapon.GetActiveSkillForSlot(SkillSlot.D, level);
        if (activeSkillD != null)
        {
            _activeDIcon.sprite = activeSkillD.iconSprite;
            _activeDName.text = activeSkillD.skillName;
            _activeDCooltime.text = activeSkillD.cooldown.ToString();
            _activeDDetail.text = activeSkillD.description;
        }
        else
        {
            _activeDIcon.sprite = null;
            _activeDName.text = "";
            _activeDCooltime.text = "";
            _activeDDetail.text = "";
        }

        var activeSkillF = weapon.GetActiveSkillForSlot(SkillSlot.F, level);
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
            _activeFIcon.sprite = null;
            _activeFName.text = "";
            _activeFCooltime.text = "";
            _activeFDetail.text = "";
            _activeFIcon.transform.parent.gameObject.SetActive(false);
        }
        _root.SetActive(true);
    }

}
