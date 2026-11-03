// DebugTokenAdder.cs

using UnityEngine;

public class DebugTokenAdder : MonoBehaviour
{
    [Header("����")]
    public KeyCode modifierKey1 = KeyCode.LeftShift;
    public KeyCode modifierKey2 = KeyCode.RightShift;
    public KeyCode activationKey = KeyCode.T;

    [Tooltip("�� ���� �߰��� ��ū ����")]
    public int tokensToAdd = 4;

    void Update()
    {
        // �� ���� ���� Ű�� '�����ִ� ����(GetKey)' ���� Ű�� '������ ����(GetKeyDown)'�� ����
        if (Input.GetKey(modifierKey1) && Input.GetKey(modifierKey2) && Input.GetKeyDown(activationKey))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddTokens(tokensToAdd);
                Debug.Log($" ��ū {tokensToAdd}���� �߰��Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogWarning("GameManager.Instance가 null입니다.");
            }
        }
    }
}