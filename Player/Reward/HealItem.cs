using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealItem : ItemData, IUsableItem
{
    // 체력 회복 아이템 
    [SerializeField] private float _healAmount;

    // 회복 아이템 정보 UI 
    [SerializeField] private GameObject _root;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private TextMeshProUGUI _itemEffectDetail;

    [SerializeField] private GameObject _healVFX;
    [SerializeField] private Vector2 _offset;

    private ItemInteractor _currentInteractor;
    private float _checkTimer = 0f;
    private const float _checkInterval = 0.2f;

    // 사용시 체력 회복 
    public void Use(PlayerController player)
    {
        player._playerHealth.Heal(_healAmount);

        var vfx = Instantiate(_healVFX);
        vfx.transform.SetParent(player.transform);
        vfx.transform.localPosition = _offset;
        Destroy(vfx, 1.5f);

        Destroy(gameObject);
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
