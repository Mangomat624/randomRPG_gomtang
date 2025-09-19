using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GradeProbability
{
    public EquipmentData.Grade grade;
    public float probability;
}

[CreateAssetMenu(fileName = "NewRerollTable", menuName = "Data/Reroll Probability Table")]
public class RerollProbabilityTable : ScriptableObject
{
    public List<GradeProbability> probabilities;
}