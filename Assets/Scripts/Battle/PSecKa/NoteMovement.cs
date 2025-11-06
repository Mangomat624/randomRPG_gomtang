// NoteMovement.cs (최종 정리)

using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    public float speed = 15f;
    public int laneIndex; // 0=Up, 1=Middle, 2=Bottom

    // 노트의 파괴는 NoteManager가 전담하므로
    // noteManager, deleteLineTransform 변수 모두 삭제.

    void OnEnable()
    {
        if (GameManager.Instance.currentStage >= 7)
        {
            HarderSpped();
        }
    }

    void Start()
    {

    }

    public void SetLaneIndex(int index)
    {
        laneIndex = index;
    }

    public void HarderSpped()
    {
        speed = speed * 1.5f;
    }

    void Update()
    {
        // 왼쪽으로 이동하는 코드 "만" 남깁니다.
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }
}