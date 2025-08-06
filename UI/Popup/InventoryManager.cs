using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryState
{
    Equipment,
    Item,
    ReplaceItem
}

public class InventoryManager : MonoBehaviour
{
    // 인벤토리 매니져, 지금은 구조가 단순하여 UI 관리와 통합 되어있는데 이를 InventoryUI 로 분리 필요
    // 입력 또한 뉴 인풋시스템으로 추후 변경 필요 

    public static InventoryManager Instance { get; private set; }

    public InventoryState currentState = InventoryState.Equipment;
    [SerializeField] private PlayerController _playerController;

    [Header("UI 슬롯")]
    public List<UISelectableSlot> equipmentSlots;
    public List<UISelectableSlot> itemSlots;

    [SerializeField] private List<ItemData> _items = new();
    private const int MaxItems = 9;
    public IReadOnlyList<ItemData> Items => _items;

    private int _equipmentIndex = 0;
    [SerializeField] private int _itemIndex = 0;

    private const int RowSize = 3;
    private const int ColumnSize = 3;
    private const int EquipmentSlotCount = 2;
    private ItemData _pendingReplaceItem;
    private Vector3 _originalItemPosition;

    [SerializeField] private GameObject _root;
    [SerializeField] private UIArmor _uiArmor;
    [SerializeField] private UIWeapon _uiWeapon;
    [SerializeField] private UIItem _uiItem;

    private float _lastUICloseTime = -10f;
    private float _vKeyHoldTime = 0f;
    private const float HoldThreshold = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
 
    }

    private void Start()
    {
        _playerController = GameManager.Instance.playerController;
    }

    void Update()
    {
        HandleNavigation();
        HandleItemDestruction();
    }

    // 장비 최신화 
    public void RefreshEquipmentUI()
    {
        if (_playerController == null) return;

        if (_playerController._armorHandler?.equippedArmor?.data != null)
            equipmentSlots[0].SetItem(_playerController._armorHandler.equippedArmor.data.armorSprite);
        else
            equipmentSlots[0].Clear();

        if (_playerController._weaponHandler?.equippedWeapon?.data != null)
            equipmentSlots[1].SetItem(_playerController._weaponHandler.equippedWeapon.data.weaponIcon);
        else
            equipmentSlots[1].Clear();
    }

    public void TogleUI()
    {
        if (Time.realtimeSinceStartup - _lastUICloseTime < 0.1f) return;

        if (_root.activeSelf == false)
        {
            _root.SetActive(true);

            if (currentState != InventoryState.ReplaceItem)
                currentState = InventoryState.Equipment;

            _equipmentIndex = 0;
            RefreshUI();
            HighlightCurrentSlot();
            RefreshEquipmentUI();
            Time.timeScale = 0;
        }
        else
        {
            _root.SetActive(false);
            Time.timeScale = 1;
        }
    }

    // 패시브 아이템 추가 함수 
    public bool AddItem(ItemData item)
    {
        if (_items.Count >= MaxItems)
        {
            OpenReplaceItemUI(item);
            return false;
        }

        _items.Add(item);
        RefreshUI();
        return true;
    }

    // 패시브 아이템 삭제 함수 
    public void RemoveItem(ItemData item)
    {
        _items.Remove(item);
    }

    // 아이템 교체시의 UI 활성화 
    public void OpenReplaceItemUI(ItemData replaceItem)
    {
        _pendingReplaceItem = replaceItem;
        _originalItemPosition = replaceItem.transform.position;
        currentState = InventoryState.ReplaceItem;
        _itemIndex = 0;

        _root.SetActive(true);
        RefreshUI();
        HighlightCurrentSlot();
        RefreshEquipmentUI();
        Time.timeScale = 0;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < _items.Count)
                itemSlots[i].SetItem(_items[i]);
            else
                itemSlots[i].Clear();

            itemSlots[i].gameObject.SetActive(true);
        }
    }

    // 키 조작시 State 변환, 추후 뉴 인풋 시스템으로 변경 필요  
    void HandleNavigation()
    {
        if (currentState == InventoryState.Equipment)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveEquipmentCursor(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MoveEquipmentCursor(1);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_equipmentIndex == 1) // 방어구 슬롯일 때만 아이템 상태로 전환
                    SwitchToItemState();
                else
                    MoveEquipmentCursor(1); // 무기 → 방어구
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveEquipmentCursor(-1); // 방어구 → 무기 가능하게
            }
        }
        else if (currentState == InventoryState.Item)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveItemCursor(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MoveItemCursor(Vector2Int.right);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) MoveItemCursor(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_itemIndex <= 2) // 0~2번 줄일 경우만
                    SwitchToEquipmentState();
                else
                    MoveItemCursor(Vector2Int.down);
            }
        }
        else if (currentState == InventoryState.ReplaceItem)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveItemCursor(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MoveItemCursor(Vector2Int.right);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) MoveItemCursor(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.UpArrow)) MoveItemCursor(Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.V)) ConfirmReplace();
            else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape)) CancelReplace();
        }
    }

    // 패시브 아이템 교체 승인 
    void ConfirmReplace()
    {
        if (_pendingReplaceItem == null) return;

        if (_itemIndex < _items.Count)
        {
            var oldItem = _items[_itemIndex];

            if (oldItem is IStatItem statItemOld)
                statItemOld.RemoveStat(_playerController);

            _items[_itemIndex] = _pendingReplaceItem;

            if (_pendingReplaceItem is IStatItem statItemNew)
                statItemNew.ApplyStat(_playerController);

            StartCoroutine(DelayedDropItem(oldItem, _originalItemPosition));
        }

        _pendingReplaceItem = null;
        currentState = InventoryState.Item;
        RefreshUI();
        HighlightCurrentSlot();
        _root.SetActive(false);
        Time.timeScale = 1;
    }

    private IEnumerator DelayedDropItem(ItemData item, Vector3 position)
    {
        yield return new WaitForSeconds(0.1f); // 한 프레임 대기
        item.transform.SetParent(_playerController.currentMapRoot);
        item.transform.position = position;
        item.transform.rotation = Quaternion.identity;
        item.gameObject.SetActive(true);
    }

    // 패시브 아이템 교체 취소 
    void CancelReplace()
    {
        if (_pendingReplaceItem != null)
        {
            _pendingReplaceItem.transform.position = _originalItemPosition;
            _pendingReplaceItem.transform.rotation = Quaternion.identity;
            _pendingReplaceItem.gameObject.SetActive(true);
        }

        _pendingReplaceItem = null;
        currentState = InventoryState.Item;
        _root.SetActive(false);
        Time.timeScale = 1;

        _lastUICloseTime = Time.realtimeSinceStartup;
    }

    void MoveItemCursor(Vector2Int direction)
    {
        int row = _itemIndex / RowSize;
        int col = _itemIndex % RowSize;

        int newRow = Mathf.Clamp(row + direction.y, 0, ColumnSize - 1);
        int newCol = Mathf.Clamp(col + direction.x, 0, RowSize - 1);
        int newIndex = newRow * RowSize + newCol;

        _itemIndex = newIndex;
        HighlightCurrentSlot();
    }

    // 선택 아이템 하이라이트 변경 
    void HighlightCurrentSlot()
    {
        ClearAllHighlights();
        ResetAllDetail();

        if (currentState == InventoryState.Equipment && equipmentSlots.Count > 0)
        {
            equipmentSlots[_equipmentIndex].Highlight(true);
            if (_equipmentIndex == 0)
            {
                _uiArmor.gameObject.SetActive(true);
                _uiArmor.SetDetail(_playerController._armorHandler);
            }
            else if (_equipmentIndex == 1)
            {
                _uiWeapon.gameObject.SetActive(true);
                _uiWeapon.SetDetail(_playerController._weaponHandler);
            }
        }
        else if ((currentState == InventoryState.Item || currentState == InventoryState.ReplaceItem) && itemSlots.Count > 0)
        {
            itemSlots[_itemIndex].Highlight(true);
            _uiItem.gameObject.SetActive(true);

            if (_itemIndex < _items.Count)
                _uiItem.SetDetail(_items[_itemIndex]);
            else
                _uiItem.ResetDetail();
        }
    }

    void ResetAllDetail()
    {
        _uiArmor.gameObject.SetActive(false);
        _uiWeapon.gameObject.SetActive(false);
        _uiItem.gameObject.SetActive(false);
    }

    void ClearAllHighlights()
    {
        foreach (var slot in equipmentSlots)
            slot.Highlight(false);
        foreach (var slot in itemSlots)
            slot.Highlight(false);
    }

    void HandleItemDestruction()
    {
        if (currentState != InventoryState.Item) return;

        if (Input.GetKey(KeyCode.V))
        {
            _vKeyHoldTime += Time.unscaledDeltaTime;
            if (_vKeyHoldTime >= HoldThreshold)
            {
                DestroyCurrentItem();
                _vKeyHoldTime = 0f;
            }
        }
        else if (Input.GetKeyUp(KeyCode.V))
        {
            _vKeyHoldTime = 0f;
        }
    }

    void DestroyCurrentItem()
    {
        if (_itemIndex < _items.Count)
        {
            var item = _items[_itemIndex];

            if (item is IStatItem statItem)
                statItem.RemoveStat(_playerController);

            RemoveItem(item);
            Destroy(item.gameObject);
            RefreshUI();
            HighlightCurrentSlot();
        }
    }

    public void ResetInventory()
    {
        // 능력치 제거
        foreach (var item in _items)
        {
            if (item is IStatItem statItem)
            {
                statItem.RemoveStat(_playerController);
            }
        }

        _items.Clear();
        RefreshUI();
    }


    void SwitchToItemState()
    {
        ClearAllHighlights();
        _itemIndex = 0; // 항상 0번 인덱스부터 시작
        currentState = InventoryState.Item;
        HighlightCurrentSlot();
    }

    void SwitchToEquipmentState()
    {
        ClearAllHighlights();
        currentState = InventoryState.Equipment;
        HighlightCurrentSlot();
    }

    void MoveEquipmentCursor(int delta)
    {
        ClearAllHighlights();
        _equipmentIndex = Mathf.Clamp(_equipmentIndex + delta, 0, EquipmentSlotCount - 1);
        HighlightCurrentSlot();
    }
}