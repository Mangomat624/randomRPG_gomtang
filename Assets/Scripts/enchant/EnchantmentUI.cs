// EnhancementUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnhancementUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI bonusAtkText;
    public TextMeshProUGUI bonusAccText;
    public Button enhanceButton; // 강화 시도 버튼
    public GameObject enhanceObject; // 강화 시도 버튼

    // ▼▼▼ '강화 시작' 버튼 변수 추가 ▼▼▼
    public Button startButton;

    void OnEnable()
    {
        EnhancementManager.OnEnhancementUpdated += UpdateStatText;
        EnhancementManager.OnTimerUpdated += UpdateTimerText;
    }

    void OnDisable()
    {
        EnhancementManager.OnEnhancementUpdated -= UpdateStatText;
        EnhancementManager.OnTimerUpdated -= UpdateTimerText;
    }

    void Start()
    {
        enhanceObject.SetActive(false);
        enhanceButton.gameObject.SetActive(false);
        // 강화 버튼을 누르면 AttemptEnhance 함수 호출
        enhanceButton.onClick.AddListener(EnhancementManager.Instance.AttemptEnhance);

        // ▼▼▼ '강화 시작' 버튼에 기능 연결 ▼▼▼
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    // ▼▼▼ '강화 시작' 버튼 클릭 시 호출될 함수 ▼▼▼
    private void OnStartButtonClicked()
    {
        // 1. 강화 매니저에게 타이머 시작 명령
        EnhancementManager.Instance.StartEnhancementPhase();

        // 2. 시작 버튼은 한 번만 누를 수 있도록 비활성화
        startButton.gameObject.SetActive(false);
        enhanceObject.SetActive(true);
        enhanceButton.gameObject.SetActive(true);
    }

    private void UpdateStatText(int level, int atk, int acc)
    {
        levelText.text = $"Lv. {level}";
        bonusAtkText.text = $"+{atk} ATK";
        bonusAccText.text = $"+{acc}% ACC";
    }

    private void UpdateTimerText(float time)
    {
        if (time < 0) time = 0;
        timerText.text = $"남은 시간: {time:F2}";
    }
}