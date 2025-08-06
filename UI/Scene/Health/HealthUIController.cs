using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIController : MonoBehaviour
{    
    // 체력 관련 UI 

    public static HealthUIController Instance { get; private set; }
    
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    public void SetPlayer(PlayerHealth health)
    {
        if (playerHealth != null)
        {
            // 기존 리스너 제거
            playerHealth.OnDamaged -= UpdateUI;
            playerHealth.OnHealed -= UpdateUI;
            playerHealth.OnMaxHealthChanged -= UpdateUI; 
        }

        playerHealth = health;

        if (playerHealth != null)
        {
            playerHealth.OnDamaged += UpdateUI;
            playerHealth.OnHealed += UpdateUI;
            playerHealth.OnMaxHealthChanged += UpdateUI;
            UpdateUI();
        }
    }


    private void UpdateUI()
    {
        if (playerHealth == null) return;

        float current = playerHealth.CurrentHealth;
        float max = playerHealth.MaxHealth;

        healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        healthBar.fillAmount = max > 0 ? current / max : 0f;
    }
}