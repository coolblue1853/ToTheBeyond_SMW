using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUIController : MonoBehaviour
{
    // 장탄 정보를 알려주는 UI 

    [SerializeField] private Image ammoBar;
    [SerializeField] private TextMeshProUGUI ammoText;
    private RangedWeapon trackedWeapon;

    public void SetWeapon(RangedWeapon weapon)
    {
        trackedWeapon = weapon;
        UpdateUI(); // 즉시 갱신
    }

    private void Update()
    {
        if (trackedWeapon == null) return;

        if (trackedWeapon.IsReloading)
        {
            ammoText.text = "Reloading";
            ammoBar.fillAmount = 0f; 
        }
        else
        {
            int current = trackedWeapon.CurrentAmmo;
            int max = Mathf.RoundToInt(trackedWeapon.RangedData.maxAmmo * trackedWeapon.playerStat.MaxAmmo);
            ammoText.text = $"{current} / {max}";
            ammoBar.fillAmount = max > 0 ? (float)current / max : 0f;
        }
    }


    private void UpdateUI()
    {
        int current = trackedWeapon.CurrentAmmo;
        int max = Mathf.RoundToInt(trackedWeapon.RangedData.maxAmmo * trackedWeapon.playerStat.MaxAmmo);
        ammoText.text = $"{current} / {max}";
        ammoBar.fillAmount = max > 0 ? (float)current / max : 0f;
    }
}