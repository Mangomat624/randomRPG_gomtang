// GachaSlotUI.cs (정리된 버전)

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaSlotUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Image itemIconImage;
    public TextMeshProUGUI gradeText;
    public Button lockButton;
    public GameObject lockOverlay;

    private bool isLocked = false;
    private EquipmentData currentData;

    void Start()
    {
        lockButton.onClick.AddListener(OnLockButtonClicked);
        UpdateLockVisual();
    }

    public void UpdateSlot(EquipmentData data)
    {
        currentData = data;
        if (currentData != null)
        {
            gradeText.text = data.grade.ToString();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public EquipmentData GetCurrentData()
    {
        return currentData;
    }


    private void OnLockButtonClicked()
    {
        isLocked = !isLocked;
        UpdateLockVisual();
    }

    public bool IsLocked() { return isLocked; }

    private void UpdateLockVisual()
    {
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(isLocked);
        }
    }

    // 외부에서 강제 잠금을 할 수 있도록 ForceLock 함수는 남겨둡니다.
    public void ForceLock()
    {
        isLocked = true;
        UpdateLockVisual();
    }
}