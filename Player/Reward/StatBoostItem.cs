using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StatBoostItem : ItemData, IStatItem
{
    // 능력치를 증가시키는 패시브 아이템 
    private List<StatModifier> _appliedModifiers = new();

    // 아이템 정보 UI 
    [SerializeField] private GameObject _root;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private TextMeshProUGUI _itemEffectDetail;

    [System.Serializable]
    public struct BoostData
    {
        public StatType statType;
        public float amount;
        public StatModifier.ModifierMode mode;
    }

    [SerializeField] private BoostData[] _boosts;

    private ItemInteractor _currentInteractor;
    private float _checkTimer = 0f;
    private const float _checkInterval = 0.2f;

    public void Use(PlayerController player)
    {
        if (InventoryManager.Instance.AddItem(this))
        {
            ApplyStat(player);
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ApplyStat(PlayerController player)
    {
        foreach (var boost in _boosts)
        {
            var mod = new StatModifier(boost.statType, boost.amount, boost.mode);
            _appliedModifiers.Add(mod);
            player.runtimeStat.AddModifier(mod);
        }
    }

    public void RemoveStat(PlayerController player)
    {
        foreach (var mod in _appliedModifiers)
            player.runtimeStat.RemoveModifier(mod);

        _appliedModifiers.Clear();

        var weapon = player._weaponHandler?.equippedWeapon as RangedWeapon;
        if (weapon != null)
            weapon.ClampAmmoToMax();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out ItemInteractor interactor))
        {
            interactor.SetNearbyItem(this);
            _currentInteractor = interactor;
            SetDetail();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out ItemInteractor interactor))
        {
            if (interactor.nearbyItem == this)
            {
                interactor.ClearNearbyItem(this);
                _currentInteractor = null;
                ResetDetail();
            }
        }
    }

    private void OnDisable()
    {
        _currentInteractor = null;
        ResetDetail();
    }

    private void Update()
    {
        if (_root != null && _root.activeSelf && _currentInteractor != null)
        {
            _checkTimer -= Time.deltaTime;
            if (_checkTimer <= 0f)
            {
                _checkTimer = _checkInterval;

                Collider2D myCollider = GetComponent<Collider2D>();
                Collider2D playerCollider = _currentInteractor.GetComponent<Collider2D>();

                if (myCollider != null && playerCollider != null)
                {
                    if (!myCollider.IsTouching(playerCollider))
                    {
                        _currentInteractor.ClearNearbyItem(this);
                        _currentInteractor = null;
                        ResetDetail();
                    }
                }
            }
        }
    }

    public void SetDetail()
    {
        _root.SetActive(true);
        _icon.sprite = icon;
        _itemName.text = itemName;
        _itemDescription.text = description;
        _itemEffectDetail.text = effectDescription;
    }

    public void ResetDetail()
    {
        _root.SetActive(false);
        _itemName.text = "";
        _itemDescription.text = "";
        _itemEffectDetail.text = "";
    }
}
