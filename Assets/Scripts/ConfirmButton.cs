using UnityEngine;

public class ConfirmButton : MonoBehaviour
{
    // 인스펙터 창에서 비활성화하고 싶은 게임 오브젝트를 이 변수에 연결합니다.
    public GameObject targetObject;

    // 이 함수는 버튼의 OnClick() 이벤트에서 호출될 것입니다.
    public void TurnOffTargetObject()
    {
        // targetObject 변수에 게임 오브젝트가 연결되어 있는지 확인합니다 (실수 방지).
        if (targetObject != null)
        {
            // 연결된 게임 오브젝트를 비활성화합니다 (Hierarchy 창에서 회색으로 변하며 꺼짐).
            targetObject.SetActive(false);
        }
        else
        {
            // 만약 아무것도 연결하지 않았다면, 경고 메시지를 콘솔에 출력합니다.
            Debug.LogWarning("비활성화할 대상 오브젝트(Target Object)가 설정되지 않았습니다!");
        }
    }
}
