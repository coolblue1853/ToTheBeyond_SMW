using UnityEngine;
using UnityEngine.UI;

public class UISelectableSlot : MonoBehaviour
{
    // 아이템 슬롯 

    [SerializeField] private Image highlightImage;
    [SerializeField] private Image iconImage;

    public void SetItem(ItemData data)
    {
        if (data != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true; // 아이콘 이미지 보이기
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false; // 아이콘 없으면 숨기기
        }
    }
    public void SetItem(Sprite icon)
    {
        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public void Highlight(bool on)
    {
        if (highlightImage != null)
            highlightImage.gameObject.SetActive(on);
    }
}