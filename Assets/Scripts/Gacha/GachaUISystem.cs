// GachaUISystem.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GachaUISystem : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public GachaSlotUI[] slots; // 4개의 GachaSlotUI 스크립트를 순서대로 연결 (Head, Body, Legs, Weapon)
    public Button rerollButton; // 전체 리롤 버튼

    void OnEnable()
    {
        // GachaManager의 이벤트 구독
        GachaManager.OnGachaFinished += UpdateAllSlots;
    }

    void OnDisable()
    {
        // GachaManager의 이벤트 구독 해제
        GachaManager.OnGachaFinished -= UpdateAllSlots;
    }

    void Start()
    {
        // 리롤 버튼을 누르면 OnRerollButtonClicked 함수가 실행되도록 설정
        rerollButton.onClick.AddListener(OnRerollButtonClicked);
    }

    // 리롤 버튼 클릭 시 호출
    private void OnRerollButtonClicked()
    {
        // 각 슬롯의 잠금 상태를 배열로 만듦
        bool[] locks = new bool[4];
        for (int i = 0; i < slots.Length; i++)
        {
            locks[i] = slots[i].IsLocked();
        }

        // GachaManager에 잠금 정보와 함께 리롤 실행을 요청
        GachaManager.Instance.ExecuteReroll(locks);
    }

    // GachaManager로부터 리롤 결과를 받았을 때 호출되는 함수
    private void UpdateAllSlots(Dictionary<EquipmentData.PartType, EquipmentData> results)
    {
        foreach (var result in results)
        {
            int slotIndex = (int)result.Key; // PartType을 정수 인덱스로 변환
            if (slotIndex < slots.Length)
            {
                slots[slotIndex].UpdateSlot(result.Value);
            }
        }
    }
}