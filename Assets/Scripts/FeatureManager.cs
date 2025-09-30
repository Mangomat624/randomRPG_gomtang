using UnityEngine;
using System.Collections.Generic;

public class FeatureManager : MonoBehaviour
{
    public GachaUISystem uiSystem;
    public List<EquipmentData.Grade> gradesToCelebrate = new List<EquipmentData.Grade> { EquipmentData.Grade.S, EquipmentData.Grade.SS, EquipmentData.Grade.SSS, EquipmentData.Grade.X };

    void OnEnable() { GachaManager.OnGachaFinished += OnRerollCompleted; }
    void OnDisable() { GachaManager.OnGachaFinished -= OnRerollCompleted; }

    // 파라미터 타입을 'EquipmentData.PartType', 'EquipmentData.Grade'로 명확히 지정
    private void OnRerollCompleted(Dictionary<EquipmentData.PartType, EquipmentData> results)
    {
        foreach (var result in results)
        {
            if (result.Value == null) continue;
            EquipmentData itemData = result.Value;
            if (gradesToCelebrate.Contains(itemData.grade))
            {
                Debug.Log($"축하합니다! {itemData.grade} 등급 아이템을 뽑았습니다!");
            }
            if (itemData.grade == EquipmentData.Grade.X)
            {
                int slotIndex = (int)result.Key;
                if (uiSystem != null && slotIndex < uiSystem.slots.Length)
                {
                    uiSystem.slots[slotIndex].ForceLock();
                }
            }
        }
    }
}