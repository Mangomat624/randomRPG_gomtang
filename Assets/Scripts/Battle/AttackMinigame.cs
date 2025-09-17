using System.Collections;
using UnityEngine;

public class AttackMinigame : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform indicator;
    public RectTransform targetZone;
    public RectTransform perfectZone;
    public RectTransform backgroundBar;

    [Header("Game Settings")]
    public float moveSpeed = 600f; // 인디케이터 이동 속도

    public System.Action<float> OnMinigameFinished;
    private IEnumerator MoveIndicator()
    {
        float barWidth = backgroundBar.rect.width;
        float halfWidth = barWidth / 2;
        float currentPos = -halfWidth;
        int direction = -1; // 1: 오른쪽으로, -1: 왼쪽으로

        while (true)
        {
            // 키 입력 감지
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckHit(); // 판정 함수 호출
                yield break; // 코루틴 종료
            }

            currentPos += moveSpeed * Time.deltaTime * direction;

            // 방향 전환
            if (currentPos > halfWidth || currentPos < -halfWidth)
            {
                direction *= -1;
                currentPos = Mathf.Clamp(currentPos, -halfWidth, halfWidth);
            }

            indicator.anchoredPosition = new Vector2(currentPos, 0);
            yield return null;
        }
    }
    public void StartMinigame(System.Action<float> callback)
    {
        gameObject.SetActive(true); // UI 패널 보이기
        OnMinigameFinished = callback; // 콜백 함수 등록
        StartCoroutine(MoveIndicator());
    }
    // 판정 함수
    private void CheckHit()
    {
        float indicatorPos = indicator.anchoredPosition.x;

        float perfectMin = perfectZone.anchoredPosition.x - (perfectZone.rect.width / 2);
        float perfectMax = perfectZone.anchoredPosition.x + (perfectZone.rect.width / 2);

        float targetMin = targetZone.anchoredPosition.x - (targetZone.rect.width / 2);
        float targetMax = targetZone.anchoredPosition.x + (targetZone.rect.width / 2);

        float result = 0f; // 대미지 배율

        if (indicatorPos >= perfectMin && indicatorPos <= perfectMax)
        {
            Debug.Log("PERFECT!");
            result = 1.0f; // 완벽: 대미지 2배
        }
        else if (indicatorPos >= targetMin && indicatorPos <= targetMax)
        {
            Debug.Log("Success!");
            result = 0.5f; // 성공: 대미지 1배
        }
        else
        {
            Debug.Log("Miss!");
            result = 0f; // 빗나감: 대미지 0
        }

        gameObject.SetActive(false); // UI 패널 숨기기
        OnMinigameFinished?.Invoke(result); // 결과를 BattleSystem에 알림
    }
}
