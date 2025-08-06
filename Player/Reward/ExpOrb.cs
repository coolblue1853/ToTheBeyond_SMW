using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpOrb : MonoBehaviour, IUsableItem
{
    // 경험치 획득 아이템 
    [SerializeField] private int _expAmount = 20;

    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private string _effectDetail;

    // 아이템 정보 UI
    // 추후 상속 관계를 통해 아이템 관련 변수들은 부모로 옮겨야함 
    [SerializeField] private GameObject _root;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private TextMeshProUGUI _itemEffectDetail;

    private SpriteRenderer _spriteRenderer;
    private ItemInteractor _currentInteractor;
    private float _checkTimer = 0f;
    private const float _checkInterval = 0.2f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 사용시 경험치 증가 
    public void Use(PlayerController player)
    {
        var expController = player.expController;
        if (expController != null)
        {
            expController.AddExp(_expAmount);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ItemInteractor interactor))
        {
            interactor.SetNearbyItem(this);
            _currentInteractor = interactor;
            SetDetail();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ItemInteractor interactor))
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
        _icon.sprite = _spriteRenderer.sprite;
        _itemName.text = _name;
        _itemDescription.text = _description;
        _itemEffectDetail.text = _effectDetail;
    }

    public void ResetDetail()
    {
        _root.SetActive(false);
        _itemName.text = "";
        _itemDescription.text = "";
        _itemEffectDetail.text = "";
    }
}
