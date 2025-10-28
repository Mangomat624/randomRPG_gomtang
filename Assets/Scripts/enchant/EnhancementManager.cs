// EnhancementManager.cs

using UnityEngine;
using System;
using System.Collections;

public class EnhancementManager : MonoBehaviour
{
    public static EnhancementManager Instance { get; private set; }

    [Header("데이터 연결")]
    public EnhancementTable enhancementTable;

    [Header("강화 설정")]
    public float totalTimeLimit = 5.0f;
    public float attemptCooldown = 0.1f;
    public float criticalChance = 0.05f;
    // 실패 페널티 확률 (유지 제거, 하락 확률만 설정)
    [Range(0f, 100f)] public float dropPenaltyRate = 50f; // 하락(-1) 확률 (나머지는 초기화)

    private int currentLevel = 0;
    private int currentBonusAtk = 0;
    private int currentBonusAcc = 0;
    private float remainingTime;
    private bool canAttempt = true;
    private bool isFinished = false;
    private bool isPhaseStarted = false;

    public static event Action<int, int, int> OnEnhancementUpdated;
    public static event Action<float> OnTimerUpdated;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        remainingTime = totalTimeLimit;
        UpdateUI();
        OnTimerUpdated?.Invoke(remainingTime);
    }

    void Update()
    {
        if (!isPhaseStarted || isFinished) return;
        remainingTime -= Time.deltaTime;
        OnTimerUpdated?.Invoke(remainingTime);
        if (remainingTime <= 0)
        {
            EndEnhancementPhase();
        }
    }

    public void StartEnhancementPhase()
    {
        if (isPhaseStarted) return;
        isPhaseStarted = true;
        Debug.Log("강화 페이즈 시작!");
    }

    public void AttemptEnhance()
    {
        if (!canAttempt || isFinished || !isPhaseStarted) return;
        if (currentLevel >= 15)
        {
            if (WarningUI.Instance != null) WarningUI.Instance.ShowWarning("최대 레벨입니다!");
            return;
        }

        StartCoroutine(EnhanceCooldown());

        EnhancementTable.EnhancementStep nextStep = enhancementTable.GetStepInfo(currentLevel + 1);
        if (nextStep == null) return;

        float randomValue = UnityEngine.Random.Range(0f, 100f);
        if (randomValue <= nextStep.successRate)
        {
            HandleSuccess(nextStep);
        }
        else
        {
            HandleFailure();
        }
    }

    private void HandleSuccess(EnhancementTable.EnhancementStep successStep)
    {
        float criticalValue = UnityEngine.Random.Range(0f, 1f);
        if (criticalValue <= criticalChance && currentLevel < 14)
        {
            Debug.Log("강화 대성공! 레벨 +2");
            EnhancementTable.EnhancementStep criticalStep = enhancementTable.GetStepInfo(currentLevel + 2);
            if (criticalStep != null)
            {
                currentLevel = criticalStep.level;
                currentBonusAtk = criticalStep.bonusAtk;
                currentBonusAcc = criticalStep.bonusAcc;
            }
        }
        else
        {
            Debug.Log("강화 성공! 레벨 +1");
            currentLevel = successStep.level;
            currentBonusAtk = successStep.bonusAtk;
            currentBonusAcc = successStep.bonusAcc;
        }
        UpdateUI();
    }

    // ▼▼▼ 실패 처리 함수 수정 (유지 페널티 제거) ▼▼▼
    private void HandleFailure()
    {
        float penaltyRoll = UnityEngine.Random.Range(0f, 100f);

        if (penaltyRoll <= dropPenaltyRate)
        {
            // 하락 (-1)
            Debug.Log("강화 실패! (하락)");
            if (currentLevel > 0)
            {
                currentLevel--;
                if (currentLevel == 0)
                {
                    currentBonusAtk = 0;
                    currentBonusAcc = 0;
                }
                else
                {
                    EnhancementTable.EnhancementStep previousStep = enhancementTable.GetStepInfo(currentLevel);
                    if (previousStep != null)
                    {
                        currentBonusAtk = previousStep.bonusAtk;
                        currentBonusAcc = previousStep.bonusAcc;
                    }
                }
            }
            // 레벨 0에서 실패하면 그냥 유지 (변화 없음)
            else
            {
                Debug.Log("강화 실패! (0레벨에서 실패하여 유지)");
            }
        }
        else // 나머지는 모두 초기화
        {
            // 초기화 (*0)
            Debug.Log("강화 실패! (초기화)");
            currentLevel = 0;
            currentBonusAtk = 0;
            currentBonusAcc = 0;
        }
        UpdateUI();
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private IEnumerator EnhanceCooldown()
    {
        canAttempt = false;
        yield return new WaitForSeconds(attemptCooldown);
        canAttempt = true;
    }

    private void EndEnhancementPhase()
    {
        isFinished = true;
        canAttempt = false;
        Debug.Log($"강화 종료! 최종 스탯: +{currentBonusAtk} ATK, +{currentBonusAcc}% ACC");

        GameManager.Instance.ApplyPermanentEnhancement(currentBonusAtk, currentBonusAcc);
    }

    private void UpdateUI()
    {
        OnEnhancementUpdated?.Invoke(currentLevel, currentBonusAtk, currentBonusAcc);
    }
}