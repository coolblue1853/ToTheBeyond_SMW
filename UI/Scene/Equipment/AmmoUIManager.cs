using UnityEngine;

public class AmmoUIManager : MonoBehaviour
{
    // 잔여 탄약 UI 관리자 

    public static AmmoUIManager Instance { get; private set; }

    [SerializeField] private GameObject ammoUIPrefab;
    [SerializeField] private Transform uiParent; 

    private GameObject currentUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetRangedWeapon(RangedWeapon weapon)
    {
        if (currentUI != null)
            Destroy(currentUI);

        currentUI = Instantiate(ammoUIPrefab, uiParent);
        currentUI.GetComponent<AmmoUIController>().SetWeapon(weapon);
    }

    public void Clear() => Destroy(currentUI);
}