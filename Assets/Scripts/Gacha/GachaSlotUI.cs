// GachaSlotUI.cs

using TMPro; // TextMeshPro를 사용한다면 추가
using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 사용하기 위해 추가
using System.Collections.Generic;


public class GachaSlotUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Image itemIconImage; // 아이템 아이콘
    public TextMeshProUGUI gradeText; // 아이템 등급 텍스트
    public Button lockButton; // 잠금 버튼
    public GameObject lockOverlay; // 잠겼을 때 표시될 이미지 등

    private bool isLocked = false;
    private EquipmentData currentData;

    List<string> highgrades = new List<string> { "S", "SS", "SSS", "X" };

    void Start()
    {
        // 잠금 버튼을 누르면 OnLockButtonClicked 함수가 실행되도록 설정
        lockButton.onClick.AddListener(OnLockButtonClicked);
        UpdateLockVisual();
    }

    // 슬롯의 UI를 아이템 데이터에 맞게 업데이트하는 함수
    public void UpdateSlot(EquipmentData data)
    {
        currentData = data;
        if (currentData != null)
        {
            // (가정) 아이템 아이콘은 Resources 폴더나 다른 곳에서 불러옵니다.
            // itemIconImage.sprite = Resources.Load<Sprite>("Icons/" + data.name);
            string gradeCurrent = data.grade.ToString();
            gradeText.text = gradeCurrent;
                if (highgrades.Contains(gradeCurrent))
                {
                    // 5. 포함되어 있다면, 축하 메시지를 출력합니다.
                    Debug.Log($"축하합니다! {gradeCurrent} 등급을 뽑았습니다!");
                }

            gameObject.SetActive(true);
        }
        else
        {
            // 데이터가 없으면 슬롯을 비활성화
            gameObject.SetActive(false);
        }
    }

    // 잠금 버튼 클릭 시 호출
    private void OnLockButtonClicked()
    {
        isLocked = !isLocked;
        UpdateLockVisual();
    }

    // 현재 잠금 상태를 반환하는 함수
    public bool IsLocked()
    {
        return isLocked;
    }

    // 잠금 상태에 따라 시각적 요소 업데이트
    private void UpdateLockVisual()
    {
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(isLocked);
        }
    }
}