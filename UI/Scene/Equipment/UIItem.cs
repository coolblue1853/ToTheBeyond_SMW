using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private TextMeshProUGUI _itemEffectDetail;

    public void SetDetail(ItemData item)
    {
        if (item == null)
        {
            _icon.gameObject.SetActive(false);
            _itemName.text = "";
            _itemDescription.text = "";
            _itemEffectDetail.text = "";
            return;
        }

        _icon.gameObject.SetActive(true);
        _icon.sprite = item.icon;
        _itemName.text = item.itemName;
        _itemDescription.text = item.description;
        _itemEffectDetail.text = item.effectDescription;
    }

    public void ResetDetail()
    {
        _icon.gameObject.SetActive(false);
        _itemName.text = "";
        _itemDescription.text = "";
        _itemEffectDetail.text = "";
    }
}