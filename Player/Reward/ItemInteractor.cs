using System.Collections;
using UnityEngine;

public class ItemInteractor : MonoBehaviour
{
    public IUsableItem nearbyItem;

    void Update()
    {
        if (nearbyItem != null && Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(UseItemNextFrame());
        }
    }

    IEnumerator UseItemNextFrame()
    {
        yield return null; // 한 프레임 대기
        nearbyItem.Use(GameManager.Instance.playerController);
    }

    public void SetNearbyItem(IUsableItem item)
    {
        nearbyItem = item;
    }

    public void ClearNearbyItem(IUsableItem item)
    {
        if (nearbyItem == item)
            nearbyItem = null;
    }
}
