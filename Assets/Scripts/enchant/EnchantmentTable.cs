// EnhancementTable.cs

using UnityEngine;

[CreateAssetMenu(fileName = "NewEnhancementTable", menuName = "Data/Enhancement Table")]
public class EnhancementTable : ScriptableObject
{
    [System.Serializable]
    public class EnhancementStep
    {
        public int level;
        public float successRate;
        public int bonusAtk;
        public int bonusAcc;
    }

    public EnhancementStep[] steps;

    public EnhancementStep GetStepInfo(int level)
    {
        if (level > 0 && level <= steps.Length)
        {
            return steps[level - 1];
        }
        return null;
    }
}