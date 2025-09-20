using System.Collections;
using UnityEngine;

public struct AttackResult
{
    public float damageMultiplier; // 최종 대미지 계산 배율
    public int accuracyBonus;      // 적중률 보너스
    public string judgement;       // 판정 이름 (Perfect, Great 등)
}

public class UndertaleAttack : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform indicator;
    public RectTransform perfectZone;
    public RectTransform greatZone; // 새로 추가
    public RectTransform niceZone;  // 새로 추가
    public RectTransform normalZone; // 기존 TargetZone
    public RectTransform backgroundBar;

    [Header("Game Settings")]
    public float moveSpeed = 400f; // 인디케이터 이동 속도

    public System.Action<AttackResult> OnMinigameFinished;

    private Coroutine _moveCoroutine;

    public void StartMinigame(System.Action<AttackResult> callback)
    {
        gameObject.SetActive(true); // UI 패널 보이기
        OnMinigameFinished = callback; // 콜백 함수 등록
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveIndicator());
    }
    private IEnumerator MoveIndicator()
    {
        float barWidth = backgroundBar.rect.width;
        float halfWidth = barWidth / 2;

        float currentPos = halfWidth;

        // 인디케이터가 왼쪽 끝에 도달할 때까지 루프 반복
        while (currentPos > -halfWidth)
        {
            // 스페이스 바 입력 감지
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckHit(); // 판정 함수 호출
                yield break; // 입력했으므로 코루틴 즉시 종료
            }

            // 왼쪽으로 이동
            currentPos -= moveSpeed * Time.deltaTime;
            indicator.anchoredPosition = new Vector2(currentPos, 0);

            yield return null; // 다음 프레임까지 대기
        }

        // 루프가 끝까지 실행됐다면 (키를 못 눌렀다면) 자동으로 Miss 처리
        Debug.Log("시간 초과! Miss!");
        ForceMiss();

    }
    // 판정 함수
    private void CheckHit()
    {
        float indicatorPos = indicator.anchoredPosition.x;
        AttackResult result = new AttackResult();

        // 가장 작은 존(Perfect)부터 바깥쪽으로 순서대로 체크합니다.
        if (IsHit(indicatorPos, perfectZone))
        {
            result.judgement = "Perfect";
            result.accuracyBonus = 30;
            result.damageMultiplier = 1.2f;
        }
        else if (IsHit(indicatorPos, greatZone))
        {
            result.judgement = "Great";
            result.accuracyBonus = 15;
            result.damageMultiplier = 1.1f;
        }
        else if (IsHit(indicatorPos, niceZone))
        {
            result.judgement = "Nice";
            result.accuracyBonus = 7;
            result.damageMultiplier = 1.05f;
        }
        else if (IsHit(indicatorPos, normalZone))
        {
            result.judgement = "Normal";
            result.accuracyBonus = 0;
            result.damageMultiplier = 1.0f;
        }
        else
        {
            result.judgement = "Miss";
            result.accuracyBonus = 0; // Miss는 적중 보너스 없음
            result.damageMultiplier = 0f;
        }

        Debug.Log("판정: " + result.judgement);

        gameObject.SetActive(false);
        OnMinigameFinished?.Invoke(result);
    }

    // 자동으로 Miss 판정을 내리는 새로운 함수
    private void ForceMiss()
    {
        AttackResult result = new AttackResult
        {
            judgement = "Miss",
            accuracyBonus = 0,
            damageMultiplier = 0f
        };

        gameObject.SetActive(false);
        OnMinigameFinished?.Invoke(result);
    }

    private bool IsHit(float indicatorPosition, RectTransform zone)
    {
        float zoneMin = zone.anchoredPosition.x - (zone.rect.width / 2);
        float zoneMax = zone.anchoredPosition.x + (zone.rect.width / 2);
        return indicatorPosition >= zoneMin && indicatorPosition <= zoneMax;
    }
}
