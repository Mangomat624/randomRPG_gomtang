using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GradeProbability
{
    // 'Grade'가 아닌 'EquipmentData.Grade'로 명확히 지정
    public EquipmentData.Grade grade;
    public float probability;
}

[CreateAssetMenu(fileName = "NewRerollTable", menuName = "Data/Reroll Probability Table")]
public class RerollProbabilityTable : ScriptableObject
{
    public List<GradeProbability> probabilities;
}