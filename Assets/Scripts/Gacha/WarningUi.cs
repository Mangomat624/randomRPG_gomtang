// WarningUI.cs

using UnityEngine;
using TMPro;
using System.Collections; // Coroutine 사용을 위해 추가

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance { get; private set; }

    public GameObject warningPanel; // 텍스트와 배경을 포함하는 패널
    public TextMeshProUGUI warningText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 시작할 때 경고창이 보이지 않도록 비활성화
            warningPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 경고 메시지를 화면에 표시합니다.
    /// </summary>
    public void ShowWarning(string message)
    {
        // 이미 경고창이 떠있다면 중복 실행을 막기 위해 이전 코루틴을 중지
        StopAllCoroutines();
        // 새로운 코루틴 시작
        StartCoroutine(ShowWarningCoroutine(message));
    }

    private IEnumerator ShowWarningCoroutine(string message)
    {
        // 1. 메시지 설정하고 패널 활성화
        warningText.text = message;
        warningPanel.SetActive(true);

        // 2. 2초 동안 기다림
        yield return new WaitForSeconds(2f);

        // 3. 패널 다시 비활성화
        warningPanel.SetActive(false);
    }
}