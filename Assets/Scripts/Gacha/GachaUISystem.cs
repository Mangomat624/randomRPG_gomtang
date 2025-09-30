using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GachaUISystem : MonoBehaviour
{
    public GachaSlotUI[] slots;
    public Button rerollButton;

    void OnEnable() { GachaManager.OnGachaFinished += UpdateAllSlots; }
    void OnDisable() { GachaManager.OnGachaFinished -= UpdateAllSlots; }
    void Start()
    {
        rerollButton.onClick.AddListener(OnRerollButtonClicked);
        if (GachaManager.Instance != null) GachaManager.Instance.ExecuteReroll(new bool[4]);
    }
    private void OnRerollButtonClicked()
    {
        bool[] locks = new bool[4];
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) locks[i] = slots[i].IsLocked();
        }
        if (GachaManager.Instance != null) GachaManager.Instance.ExecuteReroll(locks);
    }
    // 파라미터 타입을 'EquipmentData.PartType'으로 명확히 지정
    private void UpdateAllSlots(Dictionary<EquipmentData.PartType, EquipmentData> results)
    {
        foreach (var result in results)
        {
            int slotIndex = (int)result.Key;
            if (slotIndex < slots.Length && slots[slotIndex] != null)
            {
                slots[slotIndex].UpdateSlot(result.Value);
            }
        }
    }
}