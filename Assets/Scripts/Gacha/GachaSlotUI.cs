

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Action을 사용하기 위해 추가


public class GachaSlotUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Image itemIconImage;
    public TextMeshProUGUI gradeText;
    public Button lockButton;
    public GameObject lockOverlay;

    private bool isLocked = false;
    private EquipmentData currentData;

    public static event Action OnLockStateChanged;

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

        OnLockStateChanged?.Invoke();
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

    // --- (변경) ForceLock 대신 SetLockState 함수 ---
    // GachaUISystem이 씬 로드 시 저장된 상태를 강제로 적용하기 위해 사용
    public void SetLockState(bool state)
    {
        isLocked = state;
        UpdateLockVisual();
    }
    // --- 여기까지 변경 ---
}