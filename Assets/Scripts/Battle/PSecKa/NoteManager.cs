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
    public RectTransform canvasRect;
    public Transform deleteLine;

    [Header("스테이지별 스폰 포인트 그룹")]
    public List<Transform> stage1_3SpawnPoints; // 1~3 스테이지용 (2개 예상)
    public List<Transform> stage4_6SpawnPoints; // 4~6 스테이지용 (3개 예상)
    public List<Transform> stage7_10SpawnPoints; // 7~10 스테이지용 (4개 예상)

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
    private int activeLaneCount = 0; // 기본값

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
        int currentStage = GameManager.Instance.GetCurrentStage(); // GameManager 구조에 맞게 수정 필요

        if (currentStage <= 3) activeLaneCount = 2;
        else if (currentStage <= 6) activeLaneCount = 3;
        else if (currentStage <= 10) activeLaneCount = 4;
        else activeLaneCount = GetMaxLaneCount();

        List<Transform> currentActiveSpawners = GetCurrentSpawners(currentStage);

        Debug.Log($"스테이지 {currentStage}: {activeLaneCount}개의 레인 사용 (지정된 그룹 사용)");

        int bigNoteCount = 5;
        int smallNoteCount = 5;
        totalNotes = bigNoteCount + smallNoteCount;

        // ... (노트 차트 생성 및 셔플 로직은 동일) ...
        List<GameObject> noteChart = new List<GameObject>();
        for (int i = 0; i < bigNoteCount; i++) noteChart.Add(normalNotePrefab);
        for (int i = 0; i < smallNoteCount; i++) noteChart.Add(longNotePrefab);
        System.Random rand = new System.Random();
        noteChart = noteChart.OrderBy(x => rand.Next()).ToList();

        foreach (GameObject prefab in noteChart)
        {
            int laneIndex = UnityEngine.Random.Range(0, activeLaneCount);
            Transform spawnPoint = currentActiveSpawners[laneIndex];
            GameObject noteGO = Instantiate(prefab, canvasRect);
            noteGO.transform.position = spawnPoint.position;

            RectTransform noteRectTransform = noteGO.GetComponent<RectTransform>();

            // 2. RectTransform이 있는지 확인합니다 (안전 장치).
            if (noteRectTransform != null)
            {
                float targetHeight = 50f; // 기본 높이 값 (예시)

                // 3. activeLaneCount 값에 따라 목표 Height 결정
                switch (activeLaneCount)
                {
                    case 2:
                        targetHeight = 75f;
                        break;
                    case 3:
                        targetHeight = 50f;
                        break;
                    case 4:
                        targetHeight = 37.5f;
                        break;
                    default:
                        // 예외 처리: 기본 높이 유지
                        targetHeight = noteRectTransform.sizeDelta.y; // 현재 높이 유지
                        break;
                }

                // 4. RectTransform의 sizeDelta를 변경합니다.
                // sizeDelta는 Width와 Height를 함께 가지는 Vector2 값입니다.
                // Width는 그대로 두고 Height만 변경합니다.
                noteRectTransform.sizeDelta = new Vector2(noteRectTransform.sizeDelta.x, targetHeight);
            }

            NoteMovement noteScript = noteGO.GetComponent<NoteMovement>();

            // (변경) SetDeleteLine 호출 완전 삭제
            noteScript.SetLaneIndex(laneIndex);
            activeNotes.Enqueue(noteScript);

            float randomWaitTime = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }

    private List<Transform> GetCurrentSpawners(int stage)
    {
        if (stage <= 3) return stage1_3SpawnPoints;
        else if (stage <= 6) return stage4_6SpawnPoints;
        else return stage7_10SpawnPoints; // 7 이상은 모두 이 리스트 사용
    }
    private int GetMaxLaneCount()
    {
        int max = 0;
        if (stage1_3SpawnPoints != null) max = Mathf.Max(max, stage1_3SpawnPoints.Count);
        if (stage4_6SpawnPoints != null) max = Mathf.Max(max, stage4_6SpawnPoints.Count);
        if (stage7_10SpawnPoints != null) max = Mathf.Max(max, stage7_10SpawnPoints.Count);
        return max;
    }

    void Update()
    {
        // activeLaneCount 값에 따라 입력받을 키를 다르게 처리
        switch (activeLaneCount)
        {
            case 2: // 레인 2개 (1~3 스테이지)
                if (Input.GetKeyDown(KeyCode.F)) TryHit(0); // 첫 번째 레인
                if (Input.GetKeyDown(KeyCode.J)) TryHit(1); // 두 번째 레인
                break;
            case 3: // 레인 3개 (4~6 스테이지)
                if (Input.GetKeyDown(KeyCode.F)) TryHit(0); // 첫 번째 레인
                if (Input.GetKeyDown(KeyCode.Space)) TryHit(1); // 두 번째 레인
                if (Input.GetKeyDown(KeyCode.J)) TryHit(2); // 세 번째 레인
                break;
            case 4: // 레인 4개 (7~10 스테이지)
                if (Input.GetKeyDown(KeyCode.D)) TryHit(0); // 첫 번째 레인
                if (Input.GetKeyDown(KeyCode.F)) TryHit(1); // 두 번째 레인
                if (Input.GetKeyDown(KeyCode.J)) TryHit(2); // 세 번째 레인
                if (Input.GetKeyDown(KeyCode.K)) TryHit(3); // 네 번째 레인
                break;
            default:
                // 혹시 모를 예외 처리 (모든 레인을 F, Space, J로 처리)
                if (Input.GetKeyDown(KeyCode.F)) TryHit(0);
                if (Input.GetKeyDown(KeyCode.Space)) TryHit(1);
                if (Input.GetKeyDown(KeyCode.J)) TryHit(2);
                break;
        }

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