// DebugTokenAdder.cs

using UnityEngine;

public class DebugTokenAdder : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("첫 번째 보조 키 (예: RightArrow)")]
    public KeyCode modifierKey1 = KeyCode.RightArrow; // 이것도 수정가능

    [Tooltip("두 번째 보조 키 (예: Insert)")]
    public KeyCode modifierKey2 = KeyCode.Insert; // 얼마든지 수정가능

    [Tooltip("메인 키 (예: T)")]
    public KeyCode activationKey = KeyCode.T; // 얘도. 이왕이면 애들 손꼬이고 복잡하게 해서 못누르게 하는게 좋을듯

    [Tooltip("한 번에 추가할 토큰 개수")]
    public int tokensToAdd = 4;

    void Update()
    {
        // 두 개의 보조 키가 '눌려있는 동안(GetKey)' 메인 키를 '누르는 순간(GetKeyDown)'을 감지
        if (Input.GetKey(modifierKey1) && Input.GetKey(modifierKey2) && Input.GetKeyDown(activationKey))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddTokens(tokensToAdd);
                Debug.Log($" 토큰 {tokensToAdd}개가 추가되었습니다.");
            }
        }
    }
}