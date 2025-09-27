// GachaManager.cs

using UnityEngine;
using System;
using System.Collections.Generic; // Dictionary 사용을 위해 추가

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }
    public GameObject blockingPanel;

    [Header("연결 필수")]
    public EquipmentDatabase equipmentDatabase;
    public RerollProbabilityTable startTable;   // 시작 시 확률표
    public RerollProbabilityTable midGameTable; // Stage 3 이후 확률표
    public RerollProbabilityTable lateGameTable;  // Stage 6 이후 확률표

    // 리롤 결과를 다른 스크립트에 알려주기 위한 이벤트
    public static event Action<Dictionary<EquipmentData.PartType, EquipmentData>> OnGachaFinished;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // UI 버튼 등에서 이 함수를 호출
    public void ExecuteReroll(bool[] locks)
    {
        int lockCount = 0;
        foreach (bool locked in locks) { if (locked) lockCount++; }

        int requiredTokens = 1;

        if (GameManager.Instance.GetCurrentTokens() < requiredTokens)
        {
<<<<<<< Updated upstream
            WarningUI.Instance.ShowWarning("토큰이 부족합니다!");
=======

            Debug.Log("토큰이 부족합니다.");
            blockingPanel.SetActive(true); // 패널을 활성화해서 입력을 막습니다.

            WarningUI.Instance.ShowWarning("토큰이 부족합니다!");

>>>>>>> Stashed changes
            return;
        }

        // ▼▼▼▼▼ 이 줄을 추가해주세요 ▼▼▼▼▼
        GameManager.Instance.UseTokens(requiredTokens);
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        var resultData = new Dictionary<EquipmentData.PartType, EquipmentData>();
        int currentStage = GameManager.Instance.GetCurrentStage();
        RerollProbabilityTable selectedTable = GetCurrentTable(currentStage);

        for (int i = 0; i < 4; i++) // 4개 파츠 고정
        {
            if (locks[i]) continue; // 잠겨있으면 건너뛰기

            EquipmentData.PartType part = (EquipmentData.PartType)i;
            EquipmentData.Grade grade = GetRandomGradeFromTable(selectedTable);
            EquipmentData data = equipmentDatabase.GetData(part, grade);

            if (data != null)
            {
                resultData[part] = data;
            }
        }
        OnGachaFinished?.Invoke(resultData);
    }

    // 현재 스테이지에 맞는 확률표를 선택
    private RerollProbabilityTable GetCurrentTable(int stage)
    {
        if (stage >= 6) return lateGameTable;
        if (stage >= 3) return midGameTable;
        return startTable;
    }

    // RerollProbabilityTable의 데이터를 읽어 가중치 랜덤 뽑기를 실행
    private EquipmentData.Grade GetRandomGradeFromTable(RerollProbabilityTable table)
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (var gradeProb in table.probabilities)
        {
            cumulative += gradeProb.probability;
            if (randomValue <= cumulative)
            {
                return gradeProb.grade;
            }
        }
        return EquipmentData.Grade.F; // 예외 처리
    }
}