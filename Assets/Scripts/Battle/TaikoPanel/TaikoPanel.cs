// TaikoMinigame.cs
using System; // Action 사용
using System.Collections;
using System.Collections.Generic; // List 사용
using Unity.VisualScripting;
using UnityEngine;
using System.Linq; // <<< 바로 이 줄을 추가하거나, 빠졌다면 넣어주세요!

[RequireComponent(typeof(AudioSource))]
public class TaikoMinigame : MonoBehaviour
{
    [Header("Setting")]
    public GameObject bigNotePrefab;
    public GameObject smallNotePrefab;
    public Transform noteSpawner;
    public RectTransform judgementLine;
    public float noteSpeed = 500f;

    [Header("Judgement Scale")]
    public float perfectRange = 30f;
    public float greatRange = 60f;
    public float niceRange = 100f;

    [Header("Random Timing")]
    public float minWaitTime = 0.3f;
    public float maxWaitTime = 1.0f;

    [Header("Sound")]
    public AudioClip BighitSound; // 효과음 오디오 파일을 연결할 변수
    public AudioClip SmallhitSound; // 효과음 오디오 파일을 연결할 변수

    private AudioSource audioSource;
    private Queue<NoteObject> activeNotes = new Queue<NoteObject>();
    private List<string> results = new List<string>();
    public Action<AttackResult> OnMinigameFinished;
    private int totalNotes;
    private float distance;

    // ... (StartMinigame, SpawnNotes, Update, CheckHit, RecordResult 함수는 거의 동일) ...
    // StartMinigame의 매개변수 타입만 Action<AttackResult> 로 바꿔주세요.

    void Awake()
    {
        // 이 게임 오브젝트에 붙어있는 AudioSource 컴포넌트를 가져와 변수에 저장합니다.
        audioSource = GetComponent<AudioSource>();
    }

    public void StartMinigame(Action<AttackResult> callback)
    {
        gameObject.SetActive(true);
        OnMinigameFinished = callback;

        // 초기화
        results.Clear();
        // 큐에 남아있을 수 있는 이전 노트들 제거
        while (activeNotes.Count > 0)
        {
            Destroy(activeNotes.Dequeue().gameObject);
        }

        StartCoroutine(SpawnNotesSequence());
    }

    private IEnumerator SpawnNotesSequence()
    {
        // 1. 생성할 노트 목록(악보) 만들기
        List<GameObject> noteChart = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            noteChart.Add(bigNotePrefab);
            noteChart.Add(smallNotePrefab);
        }
        totalNotes = noteChart.Count;

        // 2. 노트 목록을 무작위로 섞기 (Fisher-Yates Shuffle)
        System.Random rand = new System.Random();
        noteChart = noteChart.OrderBy(x => rand.Next()).ToList();

        // 3. 섞인 노트 목록을 순서대로, 랜덤한 시간 간격으로 생성
        foreach (GameObject notePrefabToSpawn in noteChart)
        {
            GameObject noteGO = Instantiate(notePrefabToSpawn, noteSpawner);
            NoteObject note = noteGO.GetComponent<NoteObject>();
            activeNotes.Enqueue(note);

            float randomWaitTime = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }

    IEnumerator EndMinigame()
    {
        Debug.Log("미니게임 종료! 결과 집계 중...");

        // 1. 결과 집계 및 콜백 호출
        AttackResult finalResult = new AttackResult();
        // ... (이전과 동일한 결과 집계 로직) ...
        int successCount = 0;
        finalResult.accuracyBonus = 0;
        foreach (string r in results)
        {
            if (r != "Miss")
            {
                successCount++;
            }
        }
        switch (successCount)
        {
            case 0: finalResult.damageMultiplier = 0f; break;
            case 1: finalResult.damageMultiplier = 0.5f; break;
            case 2: finalResult.damageMultiplier = 0.7f; break;
            case 3: finalResult.damageMultiplier = 1.0f; break;
            case 4: finalResult.damageMultiplier = 1.2f; break;
            case 5: finalResult.damageMultiplier = 1.5f; break;
        }
        finalResult.judgement = $"{successCount} Hits!";
        OnMinigameFinished?.Invoke(finalResult);



        Debug.Log($"성공: {successCount}개. 최종 보너스: 정확도 +{finalResult.accuracyBonus}, 데미지 배율 x{finalResult.damageMultiplier}");


        // 2. 효과음이 끝날 때까지 잠시 대기 (0.5초면 대부분 충분합니다)
        yield return new WaitForSeconds(0.5f);

        // 3. 대기가 끝난 후 UI 패널을 비활성화
        gameObject.SetActive(false);
    }


    // --- 입력 및 판정 로직 변경 ---
    void Update()
    {
        // F키 입력 (BigNote 처리)
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (BighitSound != null) audioSource.PlayOneShot(BighitSound);
            TryHit(NoteObject.NoteType.Big);
        }

        // J키 입력 (SmallNote 처리)
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (SmallhitSound != null) audioSource.PlayOneShot(SmallhitSound);
            TryHit(NoteObject.NoteType.Small);
        }

        // 시간 초과 Miss 판정
        if (activeNotes.Count > 0)
        {
            NoteObject nextNote = activeNotes.Peek();
            if (nextNote.transform.position.x < judgementLine.position.x - niceRange)
            {
                RecordResult("Miss");
                Destroy(activeNotes.Dequeue().gameObject);
            }
        }
    }

    // 판정을 시도하는 함수
    void TryHit(NoteObject.NoteType keyType)
    {
        if (activeNotes.Count > 0)
        {
            NoteObject nextNote = activeNotes.Peek();
            distance = Mathf.Abs(nextNote.transform.position.x - judgementLine.position.x);

            if (distance <= niceRange)
            {
                // 노트가 판정 범위 안에 있을 때만 판정 실행
                CheckHit(nextNote, keyType);
            }
        }
    }

    // 판정 로직 (누른 키 타입과 노트 타입 비교)
    void CheckHit(NoteObject note, NoteObject.NoteType keyType)
    {
        // 누른 키가 노트 타입과 다르면 Miss 처리
        if (note.type != keyType)
        {
            RecordResult("Miss");
            Destroy(activeNotes.Dequeue().gameObject);
            return;
        }

        // 키가 맞다면 거리로 판정
        string judgement = "Nice";
        if (distance <= perfectRange) judgement = "Perfect";
        else if (distance <= greatRange) judgement = "Great";
        else if (distance <= niceRange) judgement = "Nice";

        RecordResult(judgement);
        Destroy(activeNotes.Dequeue().gameObject);
    }

    // 판정 결과를 기록하고 게임 종료 여부 확인
    void RecordResult(string result)
    {
        results.Add(result);
        if (results.Count >= totalNotes)
        {
            StartCoroutine(EndMinigame());
        }
    }
}