// NoteManager.cs (최종 정리)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [Header("프리팹 및 위치")]
    public GameObject normalNotePrefab;
    public GameObject longNotePrefab;
    public Transform[] laneSpawnPoints; // Up(0), Middle(1), Bottom(2) 순서
    public RectTransform canvasRect;

    // (변경) deleteLine 변수는 NoteManager.Update()에서 사용합니다.
    public Transform deleteLine;

    [Header("판정 설정")]
    public Transform judgementLine;
    public float perfectRange = 0.5f;
    public float greatRange = 1.3f;
    public float niceRange = 3.5f;

    [Header("게임 설정")]
    public float minWaitTime = 0.3f;
    public float maxWaitTime = 1.0f;

    [Header("사운드 (필요시 주석 해제)")]
    //public AudioClip hitSound;
    //private AudioSource audioSource;

    [Header("VFX")]
    public GameObject effect;
    public RectTransform effectCanvasRect;

    public Action<AttackResult> OnMinigameFinished;
    private Queue<NoteMovement> activeNotes = new Queue<NoteMovement>();
    private List<string> results = new List<string>();
    private int totalNotes;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        //audioSource = GetComponent<AudioSource>();
    }

    public void StartMinigame(Action<AttackResult> callback)
    {
        OnMinigameFinished = callback;
        results.Clear();
        while (activeNotes.Count > 0)
        {
            Destroy(activeNotes.Dequeue().gameObject);
        }
        gameObject.SetActive(true);
        spawnCoroutine = StartCoroutine(SpawnNotesSequence());
    }

    IEnumerator SpawnNotesSequence()
    {
        List<GameObject> noteChart = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            noteChart.Add(normalNotePrefab);
            noteChart.Add(longNotePrefab);
        }
        totalNotes = noteChart.Count;

        System.Random rand = new System.Random();
        noteChart = noteChart.OrderBy(x => rand.Next()).ToList();

        foreach (GameObject prefab in noteChart)
        {
            int laneIndex = UnityEngine.Random.Range(0, laneSpawnPoints.Length);
            Transform spawnPoint = laneSpawnPoints[laneIndex];
            GameObject noteGO = Instantiate(prefab, canvasRect);
            noteGO.transform.position = spawnPoint.position;

            NoteMovement noteScript = noteGO.GetComponent<NoteMovement>();

            // (변경) SetDeleteLine 호출 완전 삭제
            noteScript.SetLaneIndex(laneIndex);
            activeNotes.Enqueue(noteScript);

            float randomWaitTime = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }

    void Update()
    {
        // (입력 처리 로직은 동일)
        if (Input.GetKeyDown(KeyCode.F)) TryHit(0);
        if (Input.GetKeyDown(KeyCode.Space)) TryHit(1);
        if (Input.GetKeyDown(KeyCode.J)) TryHit(2);

        // --- (변경) 시간 초과 Miss 판정 로직 ---
        if (activeNotes.Count > 0)
        {
            NoteMovement nextNote = activeNotes.Peek();

            if (nextNote == null)
            {
                activeNotes.Dequeue();
                return;
            }

            // (변경) 판정선(judgementLine) 기준이 아닌, deleteLine 기준으로 파괴
            if (nextNote.transform.position.x <= deleteLine.position.x)
            {
                RecordResult("Miss");
                Destroy(activeNotes.Dequeue().gameObject);
            }
        }
    }

    void TryHit(int keyLaneIndex)
    {
        //if (hitSound != null) audioSource.PlayOneShot(hitSound);

        if (activeNotes.Count > 0)
        {
            NoteMovement nextNote = activeNotes.Peek();
            // (추가) 유령 노트 방어
            if (nextNote == null)
            {
                activeNotes.Dequeue();
                return;
            }

            float distance = Mathf.Abs(nextNote.transform.position.x - judgementLine.position.x);

            if (distance <= niceRange)
            {
                if (nextNote.laneIndex == keyLaneIndex)
                {
                    // (변경) 성공 시, 큐에서 제거하는 로직을 CheckHit으로 이동
                    CheckHit(nextNote);
                }
                // (주석) 유저 요청대로 잘못된 키는 무시 (아무것도 안 함)
            }
        }
    }

    void CheckHit(NoteMovement note)
    {
        string judgement;
        float distance = Mathf.Abs(note.transform.position.x - judgementLine.position.x);

        if (distance <= perfectRange)
        {
            judgement = "Perfect";
            Debug.Log("Perfect");
        }
        else if (distance <= greatRange) { 
            judgement = "Great";
            Debug.Log("Great");
        }

        else { 
            judgement = "Nice";
            Debug.Log("Nice");
        }

        if (effect != null)
        {
            GameObject effectGO = Instantiate(effect, effectCanvasRect);
            Vector3 effectPos = new Vector3(
                judgementLine.position.x,
                note.transform.position.y,
                judgementLine.position.z - 1
            );
            effectGO.transform.position = effectPos;

            ParticleSystem ps = effectGO.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        RecordResult(judgement);

        // (변경) 성공한 노트를 큐에서 제거하고 파괴
        activeNotes.Dequeue();
        Destroy(note.gameObject);
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