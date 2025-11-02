using UnityEngine;
using UnityEngine.UI; // Button, Image 사용
using TMPro; // TextMeshPro InputField 사용
using UnityEngine.SceneManagement; // Scene 전환


public class StartMenuManager : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Button startButton; // "게임 시작" 버튼
    public GameObject nameInputPanel; // "이름 입력창" 패널
    public TMP_InputField nameInputField; // 이름 입력 필드
    public Button confirmButton; // 이름 입력창의 "확인" 버튼

    public string nextSceneName = "뽑기"; // 이름 입력 후 넘어갈 씬 이름

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 1. 시작 시 이름 입력창은 무조건 숨깁니다.
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        // 2. 각 버튼에 클릭 이벤트를 연결합니다.
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    /// <summary>
    /// '게임 시작' 버튼을 눌렀을 때 호출됩니다.
    /// </summary>
    private void OnStartButtonClicked()
    {
        // 이름 입력창을 켭니다.
        if (nameInputPanel != null)
            nameInputPanel.SetActive(true);

        // (선택) 시작 버튼은 숨깁니다.
        if (startButton != null)
            startButton.gameObject.SetActive(false);
    }
    /// <summary>
    /// 이름 입력창의 '확인' 버튼을 눌렀을 때 호출됩니다.
    /// </summary>
    private void OnConfirmButtonClicked()
    {
        // 1. 입력된 이름을 가져옵니다.
        string newName = nameInputField.text;

        // 2. 이름이 비어있지 않은지 확인합니다.
        if (string.IsNullOrEmpty(newName))
        {
            // (선택) 여기에 "이름을 입력해주세요!" 같은 경고 UI를 띄울 수 있습니다.
            Debug.LogWarning("이름이 비어있습니다!");
            return;
        }

        // 3. (핵심) GameManager에 이름을 저장합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerName = newName;
            Debug.Log($"플레이어 이름이 '{newName}'(으)로 저장되었습니다.");
        }

        // 4. 다음 씬(예: ChapterSelect)으로 넘어갑니다.
        SceneManager.LoadScene(nextSceneName);
    }
}
