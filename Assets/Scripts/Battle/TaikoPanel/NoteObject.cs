// NoteObject.cs
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    // 노트의 종류를 구분하기 위한 Enum(열거형) 정의
    public enum NoteType { Big, Small }
    
    // 이 노트의 타입을 인스펙터에서 설정할 수 있도록 변수 추가
    public NoteType type;

    TaikoMinigame taiko = new TaikoMinigame();
    void Update()
    {
        // 매 프레임 왼쪽으로 정해진 속도만큼 이동
        transform.Translate(Vector2.left * taiko.noteSpeed * Time.deltaTime, Space.World);
    }
}