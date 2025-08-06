using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeatUIController : MonoBehaviour
{
    // 증기 무기의 열기 관련 UI

    [SerializeField] private Image heatBar;
    [SerializeField] private TextMeshProUGUI heatText;

    private SteamArmor trackedArmor;

    public void SetArmor(SteamArmor armor)
    {
        trackedArmor = armor;
        UpdateUI();
    }

    private void Update()
    {
        if (trackedArmor == null) return;

        if (trackedArmor.IsOverheated)
        {
            heatText.text = "Overheated!";
            heatBar.fillAmount = 1f;
        }
        else
        {
            float current = trackedArmor.CurrentHeat;
            float max = trackedArmor.MaxHeat;
            heatText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
            heatBar.fillAmount = max > 0f ? current / max : 0f;
        }
    }

    private void UpdateUI()
    {
        float current = trackedArmor.CurrentHeat;
        float max = trackedArmor.MaxHeat;
        heatText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        heatBar.fillAmount = max > 0f ? current / max : 0f;
    }
}