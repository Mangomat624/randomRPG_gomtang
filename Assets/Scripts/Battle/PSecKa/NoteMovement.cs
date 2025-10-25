// NoteMovement.cs

using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    // (삭제) NoteType enum과 type 변수는 더 이상 필요 없습니다.
    // public enum NoteType { Normal, Long }
    // public NoteType type;

    public float speed = 800f;

    // (추가) 이 노트가 속한 레인의 인덱스 (0=Up, 1=Middle, 2=Bottom)
    public int laneIndex;

    private Transform deleteLineTransform;

    private NoteManager noteManager;

    public void SetDeleteLine(Transform deleteLine)
    {
        deleteLineTransform = deleteLine;
    }

    // (추가) NoteManager가 이 노트를 스폰할 때 레인 인덱스를 알려줄 함수
    public void SetLaneIndex(int index)
    {
        laneIndex = index;
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (deleteLineTransform != null)
        {
            if (transform.position.x <= deleteLineTransform.position.x)
            {
                // (변경) 파괴되기 전에 'Miss' 판정을 알려줘야 합니다.
                // NoteManager의 RecordResult를 호출하도록 변경합니다.
                noteManager = FindAnyObjectByType<NoteManager>();
                Destroy(gameObject);
            }
        }
    }
}