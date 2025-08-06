using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpUIManager : MonoBehaviour
{
    // 경험치 관련 UI 관리 

    public static ExpUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Image expFillBar;

    private ExpController _expController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize(ExpController controller)
    {
        _expController = controller;
        _expController.OnExpChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        if (_expController != null)
        {
            _expController.OnExpChanged -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        int level = _expController.CurrentLevel;
        int exp = _expController.CurrentExp;
        int required = _expController.CurrentRequiredExp;

        if (_expController.IsMaxLevel)
        {
            levelText.text = $"Lv. {level}";
            expText.text = "";
            expFillBar.fillAmount = 1f;
        }
        else
        {
            levelText.text = $"Lv. {level}";
            expText.text = $"{exp} / {required}";
            expFillBar.fillAmount = Mathf.Clamp01((float)exp / required);
        }
    }
}