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
    public GameObject enchantEffectPrefab;
    public RectTransform effectCanvasRect;
    public Camera mainCamera;

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
        enhanceButton.onClick.AddListener(OnEnchantButtonClicked);

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

    /// <summary>
    /// 강화 버튼이 클릭되었을 때 실행되는 함수
    /// </summary>
    private void OnEnchantButtonClicked()
    {
        // 1. 이펙트 프리팹이 설정되었는지 확인
        if (enchantEffectPrefab == null)
        {
            Debug.LogError("강화 이펙트 프리팹(enchantEffectPrefab)이 연결되지 않았습니다!");
            return;
        }

        /// 1. 변환된 좌표를 저장할 변수
        Vector2 localPoint;

        // 2. RectTransformUtility를 사용해 스크린 좌표(마우스)를 캔버스 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            effectCanvasRect,       // 변환할 대상 캔버스(RectTransform)
            Input.mousePosition,    // 변환할 스크린 좌표 (마우스 위치)
            mainCamera,             // 캔버스의 렌더 카메라 (Main Camera)
            out localPoint          // 변환된 로컬 좌표(Vector2)가 여기에 저장됨
        );

        // 3. 이펙트를 'effectCanvasRect'의 자식으로 생성합니다.
        GameObject effectGO = Instantiate(enchantEffectPrefab, effectCanvasRect);

        // 4. (핵심) 생성된 이펙트의 위치를 마우스 위치로 설정합니다.
        // (UI 캔버스가 Screen Space - Overlay 모드라면, Screen 좌표인 mousePosition을 바로 사용해도 됩니다)
        effectGO.transform.localPosition = localPoint;
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
        timerText.text = $"{time:F2}";
    }
}