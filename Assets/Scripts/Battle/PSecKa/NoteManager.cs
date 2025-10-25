// NoteManager.cs

using System; // Action
using System.Collections;
using System.Collections.Generic; // List
using System.Linq; // OrderBy (셔플)
using UnityEngine;

// AudioSource 컴포넌트를 강제로 추가
//[RequireComponent(typeof(AudioSource))]
public class NoteManager : MonoBehaviour
{
    [Header("프리팹 및 위치")]
    public GameObject normalNotePrefab;
    public GameObject longNotePrefab;
    public Transform[] laneSpawnPoints; // Up, Middle, Bottom 연결
    public RectTransform canvasRect;
    public Transform deleteLine; // DelectLine 연결

    [Header("판정 설정")]
    public Transform judgementLine; // 판정선 오브젝트 연결
    public float perfectRange = 30f;
    public float greatRange = 60f;
    public float niceRange = 100f;

    [Header("게임 설정")]
    public float minWaitTime = 0.3f;
    public float maxWaitTime = 1.0f;

    [Header("사운드")]
    //public AudioClip hitSound;

    // BattleSystem과 통신하기 위한 콜백
    public Action<AttackResult> OnMinigameFinished;

    //private AudioSource audioSource;
    private Queue<NoteMovement> activeNotes = new Queue<NoteMovement>();
    private List<string> results = new List<string>();
    private int totalNotes;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        //audioSource = GetComponent<AudioSource>();
    }

    // BattleSystem에서 호출할 미니게임 시작 함수
    public void StartMinigame(Action<AttackResult> callback)
    {
        OnMinigameFinished = callback;
        results.Clear();

        // 큐에 남아있는 노트가 있다면 모두 파괴
        while (activeNotes.Count > 0)
        {
            Destroy(activeNotes.Dequeue().gameObject);
        }

        // 이 오브젝트(패널)를 활성화
        gameObject.SetActive(true);

        // 노트 생성 시작
        spawnCoroutine = StartCoroutine(SpawnNotesSequence());
    }

    // 노트 생성 코루틴 (10개 생성)
    // (변경) NoteMovement.cs에서 Miss 판정을 호출할 수 있도록 public으로 변경
    IEnumerator SpawnNotesSequence()
    {
        List<GameObject> noteChart = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            noteChart.Add(normalNotePrefab);
            noteChart.Add(longNotePrefab);
        }
        totalNotes = noteChart.Count;

        // 리스트 무작위로 섞기
        System.Random rand = new System.Random();
        noteChart = noteChart.OrderBy(x => rand.Next()).ToList();

        foreach (GameObject prefab in noteChart)
        {
            // 1. 레인 랜덤 선택
            int laneIndex = UnityEngine.Random.Range(0, laneSpawnPoints.Length);
            Transform spawnPoint = laneSpawnPoints[laneIndex];

            // 2. 노트 생성 및 설정
            GameObject noteGO = Instantiate(prefab, canvasRect);
            noteGO.transform.position = spawnPoint.position;

            NoteMovement noteScript = noteGO.GetComponent<NoteMovement>();
            noteScript.SetDeleteLine(deleteLine);

            // (추가) 노트에게 자신의 레인 인덱스를 알려줍니다.
            noteScript.SetLaneIndex(laneIndex);
            noteScript.SetDeleteLine(deleteLine);

            // 3. 큐에 추가
            activeNotes.Enqueue(noteScript);

            // 4. 랜덤 시간 대기
            float randomWaitTime = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }

    // 입력 처리
    // --- (핵심 변경) 입력 처리 ---
    void Update()
    {
        // Up 레인 (F키)
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryHit(0); // 0번 레인 판정
        }
        // Middle 레인 (Space Bar)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryHit(1); // 1번 레인 판정
        }
        // Bottom 레인 (J키)
        if (Input.GetKeyDown(KeyCode.J))
        {
            TryHit(2); // 2번 레인 판정
        }

        // --- (핵심 변경) 시간 초과 Miss 판정 ---
        if (activeNotes.Count > 0)
        {
            NoteMovement nextNote = activeNotes.Peek();
            // 판정선을 너무 많이 지나쳤다면
            if (nextNote.transform.position.x < judgementLine.position.x - niceRange)
            {
                // (변경) NoteMovement.cs가 아닌 NoteManager가 직접 처리
                RecordResult("Miss");
                Destroy(activeNotes.Dequeue().gameObject);
            }
        }
    }

    // (변경) 판정 시도 함수 (KeyType 대신 laneIndex를 받음)
    void TryHit(int keyLaneIndex)
    {
        //if (hitSound != null) audioSource.PlayOneShot(hitSound);

        if (activeNotes.Count > 0)
        {
            NoteMovement nextNote = activeNotes.Peek();
            float distance = Mathf.Abs(nextNote.transform.position.x - judgementLine.position.x);

            // 판정 범위 안에 있고,
            if (distance <= niceRange)
            {
                // (추가) 누른 키의 레인과 노트의 레인이 일치할 때만 판정
                if (nextNote.laneIndex == keyLaneIndex)
                {
                    CheckHit(nextNote);
                }
            }
        }
    }

    // (변경) 판정 실행 함수
    void CheckHit(NoteMovement note)
    {
        string judgement;
        float distance = Mathf.Abs(note.transform.position.x - judgementLine.position.x);

        // (변경) 키와 노트 타입 비교 로직 삭제 (이미 TryHit에서 처리함)
        if (distance <= perfectRange) judgement = "Perfect";
        else if (distance <= greatRange) judgement = "Great";
        else judgement = "Nice";

        RecordResult(judgement);
        Destroy(activeNotes.Dequeue().gameObject);
    }


    // 결과 기록
    public void RecordResult(string result)
    {
        results.Add(result);
        if (results.Count >= totalNotes)
        {
            // (변경) 이미 코루틴이 실행 중일 수 있으니 확인
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            StartCoroutine(EndMinigame());
        }
    }

    // 미니게임 종료 및 결과 전송
    IEnumerator EndMinigame()
    {
        AttackResult finalResult = new AttackResult();
        int successCount = 0;
        finalResult.accuracyBonus = 0; // 적중률 보너스 없음

        foreach (string r in results)
        {
            if (r != "Miss")
            {
                successCount++;
            }
        }

        // 10개 노트 기준 데미지 배율 (기존 5개 기준에서 확장)
        if (successCount <= 0) finalResult.damageMultiplier = 0f;
        else if (successCount <= 2) finalResult.damageMultiplier = 0.5f;
        else if (successCount <= 4) finalResult.damageMultiplier = 0.7f;
        else if (successCount <= 6) finalResult.damageMultiplier = 1.0f;
        else if (successCount <= 8) finalResult.damageMultiplier = 1.2f;
        else finalResult.damageMultiplier = 1.5f; // 9~10개 성공

        finalResult.judgement = $"{successCount} Hits!";

        // BattleSystem에 결과 전송
        OnMinigameFinished?.Invoke(finalResult);

        // 마지막 소리 재생을 위해 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 패널 비활성화
        gameObject.SetActive(false);
    }
}