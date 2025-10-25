// EnhancementManager.cs

using UnityEngine;
using System;
using System.Collections;

public class EnhancementManager : MonoBehaviour
{
    public static EnhancementManager Instance { get; private set; }

    [Header("데이터 연결")]
    public EnhancementTable enhancementTable;
    public SceneChange sceneLoader; // <--- SceneChange 로 수정
    public string nextSceneName = "GameScene";

    [Header("강화 설정")]
    public float totalTimeLimit = 5.0f;
    public float attemptCooldown = 0.1f;
    public float criticalChance = 0.05f;

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
            WarningUI.Instance.ShowWarning("최대 레벨입니다!");
            return;
        }

        StartCoroutine(EnhanceCooldown());

        // ▼▼▼ 1. 수정된 부분 (EnhancementTable.EnhancementStep) ▼▼▼
        EnhancementTable.EnhancementStep nextStep = enhancementTable.GetStepInfo(currentLevel + 1);
        if (nextStep == null) return;

        float randomValue = UnityEngine.Random.Range(0f, 100f);
        if (randomValue <= nextStep.successRate)
        {
            HandleSuccess(nextStep);
        }
        else
        {
            Debug.Log("강화 실패... 레벨이 유지됩니다.");
        }
    }

    // ▼▼▼ 2. 수정된 부분 (EnhancementTable.EnhancementStep) ▼▼▼
    private void HandleSuccess(EnhancementTable.EnhancementStep successStep)
    {
        float criticalValue = UnityEngine.Random.Range(0f, 1f);
        if (criticalValue <= criticalChance && currentLevel < 14)
        {
            Debug.Log("강화 대성공! 레벨 +2");
            // ▼▼▼ 3. 수정된 부분 (EnhancementTable.EnhancementStep) ▼▼▼
            EnhancementTable.EnhancementStep criticalStep = enhancementTable.GetStepInfo(currentLevel + 2);
            currentLevel = criticalStep.level;
            currentBonusAtk = criticalStep.bonusAtk;
            currentBonusAcc = criticalStep.bonusAcc;
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
        sceneLoader.LoadTargetScene(); // 올바른 함수 이름 사용
    }

    private void UpdateUI()
    {
        OnEnhancementUpdated?.Invoke(currentLevel, currentBonusAtk, currentBonusAcc);
    }
}