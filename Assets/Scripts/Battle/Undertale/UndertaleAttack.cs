using System.Collections;
using UnityEngine;

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


    [Header("Damage Speed Bonus")]
    public CharacterStats playerStats; // 인스펙터에서 플레이어 연결
    public float speedPerDamage = 20f;
    public float maxBonusSpeed = 200f;
    private float baseMoveSpeed = 320f;
    public System.Action<AttackResult> OnMinigameFinished;

    private Coroutine _moveCoroutine;
    public void StartMinigame(System.Action<AttackResult> callback)
    {
        // 매번 원본 속도에서 새로 계산 (누적 방지)
        moveSpeed = baseMoveSpeed;

        if (GameManager.Instance.currentStage >= 7)
        {
            moveSpeed += 300f;
        }

        if (playerStats != null && playerStats.playerType == CharacterStats.PlayerType.Player)
        {
            // 스테이지 7 이상은 상한 100, 그 외는 인스펙터 값
            if(GameManager.Instance.currentStage >= 7)
                maxBonusSpeed = 450f;
            else
                maxBonusSpeed = 250f;

            speedPerDamage = playerStats.baseDamage / 10;
            float rawBonus = playerStats.baseDamage * speedPerDamage;
            float bonus = Mathf.Min(rawBonus, maxBonusSpeed);

            Debug.Log($"[Undertale] baseDamage:{playerStats.baseDamage} / 무브스피드 : {moveSpeed}/ 스피트퍼데미지 : {speedPerDamage} /상한 전:{rawBonus} / 상한:{maxBonusSpeed} / 실제 보너스:{bonus} / 최종 속도:{moveSpeed + bonus}");

            moveSpeed += bonus;
        }
        else
        {
            Debug.LogWarning("[Undertale] playerStats가 비어 있거나 Player 타입이 아니에요. 인스펙터 연결을 확인하세요.");
        }

        gameObject.SetActive(true);
        OnMinigameFinished = callback;
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
            result.accuracyBonus = 42;
            result.damageMultiplier = 1.2f;
        }
        else if (IsHit(indicatorPos, greatZone))
        {
            result.judgement = "Great";
            result.accuracyBonus = 21;
            result.damageMultiplier = 1.1f;
        }
        else if (IsHit(indicatorPos, niceZone))
        {
            result.judgement = "Nice";
            result.accuracyBonus = 12;
            result.damageMultiplier = 1.05f;
        }
        else if (IsHit(indicatorPos, normalZone))
        {
            result.judgement = "Normal";
            result.accuracyBonus = 6;
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
