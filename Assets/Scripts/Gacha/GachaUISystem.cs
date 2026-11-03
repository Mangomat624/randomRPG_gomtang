using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GachaUISystem : MonoBehaviour
{
    public GachaSlotUI[] slots;
    public Button rerollButton;

    void OnEnable() { 
        GachaManager.OnGachaFinished += UpdateAllSlots;
        GachaSlotUI.OnLockStateChanged += SaveCurrentLockStates;
    }
    void OnDisable() { 
        GachaManager.OnGachaFinished -= UpdateAllSlots;
        GachaSlotUI.OnLockStateChanged += SaveCurrentLockStates;
    }
    void Start()
    {
        rerollButton.onClick.AddListener(OnRerollButtonClicked);
        //if (GachaManager.Instance != null) GachaManager.Instance.ExecuteReroll(new bool[4]); 이거 실험적 기능..? 넣을필요 없음
        InitializeSlotsWithCurrentGear();
    }

    // --- (추가) GachaSlotUI가 잠길 때마다 GameManager에 상태를 저장하는 함수 ---
    private void SaveCurrentLockStates()
    {
        if (slots == null) return;
        bool[] locks = new bool[4];
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) locks[i] = slots[i].IsLocked();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGachaLocks(locks);
        }
    }


    private void InitializeSlotsWithCurrentGear()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GachaUISystem: GameManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        // 1. 스테이지 1이면 초기화하지 않음 (이전 로직)
        int currentStage = GameManager.Instance.GetCurrentStage();
        if (currentStage == 1)
        {
            Debug.Log("GachaUISystem: 스테이지 1이므로 장비 슬롯 초기화를 건너뜁니다.");
            return;
        }

        if (GachaManager.Instance == null || GachaManager.Instance.equipmentDatabase == null)
        {
            Debug.LogError("GachaUISystem: GachaManager 인스턴스 또는 EquipmentDatabase를 찾을 수 없습니다!");
            return;
        }

        // 2. (추가) GameManager에서 저장된 "잠금 상태"를 불러옵니다.
        bool[] savedLocks = GameManager.Instance.GetGachaLocks();

        // 4개의 부위 (Head, Torso, Legs, Weapon)를 순회합니다.
        for (int i = 0; i < 4; i++)
        {
            // i 값을 PartType으로 변환합니다 (Head=0, Torso=1...)
            EquipmentData.PartType part = (EquipmentData.PartType)i;
            EquipmentData.Grade currentGrade;

            // GameManager에서 해당 부위의 현재 등급을 가져옵니다.
            switch (part)
            {
                case EquipmentData.PartType.Head:
                    currentGrade = GameManager.Instance.CurrentHeadGrade;
                    break;
                case EquipmentData.PartType.Torso:
                    currentGrade = GameManager.Instance.CurrentTorsoGrade;
                    break;
                case EquipmentData.PartType.Legs:
                    currentGrade = GameManager.Instance.CurrentLegsGrade;
                    break;
                case EquipmentData.PartType.Weapon:
                    currentGrade = GameManager.Instance.CurrentWeaponGrade;
                    break;
                default:
                    // 이 경우는 발생하지 않아야 하지만, 안전을 위해 Error등급으로 처리
                    currentGrade = EquipmentData.Grade.Error;
                    Debug.LogWarning($"GachaUISystem: 알 수 없는 PartType 인덱스 {i}");
                    break;
            }

            // GachaManager의 데이터베이스를 사용해 (부위, 등급)에 맞는 EquipmentData를 가져옵니다.
            EquipmentData data = GachaManager.Instance.equipmentDatabase.GetData(part, currentGrade);

            // 해당 인덱스의 슬롯 UI가 존재하고, 데이터를 성공적으로 가져왔다면 업데이트합니다.
            if (i < slots.Length && slots[i] != null)
            {
                if (data != null)
                {
                    slots[i].UpdateSlot(data);
                    // (선택 사항) 시작 시에는 모든 슬롯을 잠금 해제 상태로 만들 수 있습니다.
                    // slots[i].Unlock(); // GachaSlotUI에 Unlock() 함수가 필요
                    slots[i].SetLockState(savedLocks[i]);
                }
                else
                {
                    // 데이터베이스에 해당 아이템이 없는 경우 처리 (예: 슬롯 숨기기)
                    Debug.LogWarning($"EquipmentDatabase에 {part} ({currentGrade}) 데이터가 없습니다. 슬롯 {i}를 비활성화합니다.");
                    slots[i].gameObject.SetActive(false);
                }
            }
        }
    }
    private void OnRerollButtonClicked()
    {
        bool[] locks = new bool[4];
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) locks[i] = slots[i].IsLocked();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGachaLocks(locks);
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