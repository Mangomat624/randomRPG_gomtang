using UnityEngine;
using System;
using System.Collections.Generic;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }
    public EquipmentDatabase equipmentDatabase;
    public RerollProbabilityTable startTable, midGameTable, lateGameTable;

    // 이벤트의 Dictionary 타입도 'EquipmentData.PartType'으로 명확히 지정
    public static event Action<Dictionary<EquipmentData.PartType, EquipmentData>> OnGachaFinished;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    public void ExecuteReroll(bool[] locks)
    {
        if (GameManager.Instance.GetCurrentTokens() < 1)
        {
            if (WarningUI.Instance != null) WarningUI.Instance.ShowWarning("토큰이 부족합니다!");
            return;
        }
        GameManager.Instance.UseTokens(1);
        var resultData = new Dictionary<EquipmentData.PartType, EquipmentData>();
        RerollProbabilityTable selectedTable = GetCurrentTable(GameManager.Instance.GetCurrentStage());
        for (int i = 0; i < 4; i++)
        {
            if (locks[i]) continue;
            EquipmentData.PartType part = (EquipmentData.PartType)i;
            EquipmentData.Grade grade = GetRandomGradeFromTable(selectedTable);
            EquipmentData data = equipmentDatabase.GetData(part, grade);
            if (data != null) resultData[part] = data;
        }
        OnGachaFinished?.Invoke(resultData);
    }
    private RerollProbabilityTable GetCurrentTable(int stage)
    {
        if (stage >= 6) return lateGameTable;
        if (stage >= 3) return midGameTable;
        return startTable;
    }
    private EquipmentData.Grade GetRandomGradeFromTable(RerollProbabilityTable table)
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;
        foreach (var gradeProb in table.probabilities)
        {
            cumulative += gradeProb.probability;
            if (randomValue <= cumulative) return gradeProb.grade;
        }
        return EquipmentData.Grade.F;
    }
}