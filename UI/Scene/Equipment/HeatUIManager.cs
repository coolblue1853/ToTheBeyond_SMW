using UnityEngine;

public class HeatUIManager : MonoBehaviour
{
    // 증기 무기의 열기 관련 UI 관리자

    public static HeatUIManager Instance { get; private set; }

    [SerializeField] private GameObject heatUIPrefab;
    [SerializeField] private Transform uiParent; // Canvas 하위의 위치

    private GameObject currentUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetSteamArmor(SteamArmor armor)
    {
        Clear();

        currentUI = Instantiate(heatUIPrefab, uiParent);
        currentUI.GetComponent<HeatUIController>().SetArmor(armor);
    }

    public void Clear()
    {
        if (currentUI != null)
        {
            Destroy(currentUI);
            currentUI = null;
        }
    }
}