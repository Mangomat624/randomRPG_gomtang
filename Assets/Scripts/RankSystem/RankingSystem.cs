using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro를 사용하기 위해 꼭 필요합니다.
using UnityEngine;
using UnityEngine.Networking; // UnityWebRequest를 사용하기 위해 꼭 필요합니다.

public class RankingSystem : MonoBehaviour
{
    [Header("서버 설정")]
    private string rankingUrl = "https://server-mango.kro.kr/db/ranking";
    private string apiKey = "MangoKey_43f83185-67e3-4f77-83a9-35b951e0af8d";
    private string myRankingBaseUrl = "https://server-mango.kro.kr/db/ranking?playerName="; // (새 API 주소)

    [Header("UI 설정")]
    // 랭킹 항목 1줄을 표시할 UI 프리팹 (예: TextMeshProUGUI 컴포넌트만 있는 오브젝트)
    public GameObject rankingEntryPrefab;

    public Transform contentParent;

    public RankingEntryUI myRankDisplay; // 'MyRankDisplay' 오브젝트 연결

    [Header("UI 표시 (로딩/에러)")]
    public TextMeshProUGUI statusText; // "로딩 중..." 또는 "에러 발생"을 표시할 텍스트
    void Start()
    {
        // 씬이 시작되면 랭킹을 불러오는 코루틴을 실행합니다.
        StartCoroutine(GetRankingsCoroutine(rankingUrl, DisplayRankings));

        CheckAndDisplayMyRank();
    }

    private void CheckAndDisplayMyRank()
    {
        if (GameManager.Instance == null || myRankDisplay == null) return;

        // 1. GameManager에 "새 랭킹" 깃발이 켜져 있는지 확인
        if (GameManager.Instance.hasNewRankToShow)
        {
            // 2. 깃발이 켜져 있으면, "내 랭킹"을 서버에서 가져오는 코루틴 시작
            string myName = GameManager.Instance.playerName;
            string myRankUrl = myRankingBaseUrl + UnityWebRequest.EscapeURL(myName);

            StartCoroutine(GetMyRankCoroutine(myRankUrl));

            // 3. 깃발을 다시 내리기 (중복 실행 방지)
            GameManager.Instance.hasNewRankToShow = false;
        }
        else
        {
            // 4. 깃발이 꺼져 있으면 "내 랭킹" UI를 숨깁니다.
            myRankDisplay.gameObject.SetActive(false);
        }
    }

    // --- (추가) "내 랭킹"만 가져오는 코루틴 ---
    private IEnumerator GetMyRankCoroutine(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-api-key", apiKey);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string rawJson = www.downloadHandler.text;

            // (중요) 서버가 배열 [...]로 반환한다고 가정
            string wrappedJson = "{\"items\":" + rawJson + "}";
            RankingListWrapper wrapper = JsonUtility.FromJson<RankingListWrapper>(wrappedJson);

            // 1. 배열에서 첫 번째 항목(내 랭킹)을 찾습니다.
            if (wrapper.items != null && wrapper.items.Count > 0)
            {
                RankingEntry myEntry = wrapper.items[0];

                // 2. 'MyRankDisplay' UI에 데이터 채우기 (새로 만든 함수 호출)
                myRankDisplay.SetMyData(myEntry);
                myRankDisplay.gameObject.SetActive(true); // UI 켜기
            }
            else
            {
                Debug.LogWarning("내 랭킹을 GET했으나 데이터가 비어있습니다: " + rawJson);
                myRankDisplay.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("내 랭킹 GET 실패: " + www.error);
            myRankDisplay.gameObject.SetActive(false);
        }
    }

    // --- (변경) 기존 랭킹 리스트 코루틴 (재사용 가능하도록 수정) ---
    private IEnumerator GetRankingsCoroutine(string url, System.Action<List<RankingEntry>> onComplete)
    {
        if (statusText != null) statusText.text = "랭킹을 불러오는 중...";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-api-key", apiKey);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string rawJson = www.downloadHandler.text;
            string wrappedJson = "{\"items\":" + rawJson + "}";
            RankingListWrapper wrapper = JsonUtility.FromJson<RankingListWrapper>(wrappedJson);

            // (정렬)
            wrapper.items.Sort((a, b) => b.score.CompareTo(a.score));

            if (statusText != null) statusText.text = "랭킹";

            // (콜백 실행)
            onComplete?.Invoke(wrapper.items);
        }
        else
        {
            Debug.LogError("랭킹 로드 실패: " + www.error);
            if (statusText != null) statusText.text = "랭킹을 불러올 수 없습니다.\n" + www.error;
        }
    }

    private void DisplayRankings(List<RankingEntry> rankingList)
    {
        // 1. 기존 UI 항목들 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 정렬된 리스트를 순회하며 UI 생성
        int rank = 1;
        foreach (RankingEntry entry in rankingList)
        {
            // 3. 프리팹 생성 (동일)
            GameObject entryGO = Instantiate(rankingEntryPrefab, contentParent);

            // 4. (변경) TextMeshProUGUI 대신 RankingEntryUI 스크립트를 가져옵니다.
            RankingEntryUI entryScript = entryGO.GetComponent<RankingEntryUI>();

            // 5. 스크립트를 찾았다면, SetData 함수를 호출하여 값 전달
            if (entryScript != null)
            {
                entryScript.SetData(rank, entry.playerName, entry.score, entry.isEnd);
                rank++; // 다음 등수
            }
            else
            {
                Debug.LogError("RankingEntryPrefab에 RankingEntryUI 스크립트가 없습니다!");
            }
        }
    }
}
