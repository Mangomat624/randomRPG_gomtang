// StartMenuManager.cs (수정된 전체 코드)

using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions; // 정규식(Regex)
using System.Collections; // 코루틴

public class StartMenuManager : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Button startButton;
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public TextMeshProUGUI warningText; // 경고 텍스트

    [Header("설정")]
    public string nextSceneName = "ChapterSelect";

    private const string NameValidationPattern = @"^[a-zA-Z0-9가-힣]{2,10}$";
    private const string WarningMessage = "이름은 최소 2자에서 최대 10자까지 가능하며 영문 또는 한글과 숫자 조합만 가능합니다.";

    private Coroutine warningCoroutine;
    private Color defaultWarningColor = Color.white; // 기본 색상(흰색) 저장

    void Start()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        // (변경) WarningText를 숨기지 않고, 기본 메시지와 색상으로 설정합니다.
        if (warningText != null)
        {
            warningText.text = WarningMessage;
            warningText.color = defaultWarningColor;
            warningText.gameObject.SetActive(false); // 이름 입력창이 뜰 때까지는 숨김
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(true);

        // (변경) 이름 입력창을 켤 때 WarningText도 함께 켭니다.
        if (warningText != null)
        {
            // 혹시 이전에 빨간색으로 남아있었다면 흰색으로 리셋
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
            warningText.text = WarningMessage;
            warningText.color = defaultWarningColor;
            warningText.gameObject.SetActive(true); // 텍스트를 켬
        }

        if (startButton != null)
            startButton.gameObject.SetActive(false);
    }

    private void OnConfirmButtonClicked()
    {
        string newName = nameInputField.text;

        if (Regex.IsMatch(newName, NameValidationPattern))
        {
            // --- 유효성 검사 성공 ---
            if (warningText != null)
            {
                if (warningCoroutine != null) // 혹시 깜빡이는 중이었다면 중지
                {
                    StopCoroutine(warningCoroutine);
                    warningCoroutine = null;
                }
                warningText.gameObject.SetActive(false); // 성공했으니 숨김
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGameState(); // 새 게임 시작 시 전체 초기화
                GameManager.Instance.playerName = newName;
            }
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // --- 유효성 검사 실패 ---
            if (warningText != null)
            {
                // (변경) 이미 실행 중인 코루틴이 있다면 멈추고 새로 시작
                if (warningCoroutine != null)
                {
                    StopCoroutine(warningCoroutine);
                }
                warningCoroutine = StartCoroutine(ShowWarningBlink());
            }
            Debug.LogWarning($"이름 유효성 검사 실패: {newName}");
        }
    }

    // (변경) 경고 텍스트를 "깜빡"이게 하는 코루틴
    private IEnumerator ShowWarningBlink()
    {
        // 1. 텍스트가 켜져 있는지 확인 (이미 켜져 있음)
        warningText.text = WarningMessage;

        // 2. 빨간색으로 변경
        warningText.color = Color.red;

        // 3. 1초 동안 기다립니다.
        yield return new WaitForSeconds(1f);

        // 4. 1초가 지나면 다시 기본 색상(흰색)으로 변경합니다.
        warningText.color = defaultWarningColor;

        // 5. 코루틴이 끝났음을 알립니다.
        warningCoroutine = null;
    }
}