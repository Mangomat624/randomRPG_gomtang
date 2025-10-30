using UnityEngine;
using UnityEngine.Networking; // UnityWebRequest를 사용하기 위해 꼭 필요합니다.
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro를 사용하기 위해 꼭 필요합니다.

public class RankingSystem : MonoBehaviour
{
    [Header("서버 설정")]
    private string rankingUrl = "https://server-mango.kro.kr/db/ranking";
    private string apiKey = "MangoKey_43f83185-67e3-4f77-83a9-35b951e0af8d";

    [Header("UI 설정")]
    // 랭킹 항목 1줄을 표시할 UI 프리팹 (예: TextMeshProUGUI 컴포넌트만 있는 오브젝트)
    public GameObject rankingEntryPrefab;

    public Transform contentParent;

    [Header("UI 표시 (로딩/에러)")]
    public TextMeshProUGUI statusText; // "로딩 중..." 또는 "에러 발생"을 표시할 텍스트
    void Start()
    {
        // 씬이 시작되면 랭킹을 불러오는 코루틴을 실행합니다.
        StartCoroutine(GetRankingsCoroutine());
    }
    private IEnumerator GetRankingsCoroutine()
    {
        if (statusText != null) statusText.text = "랭킹을 불러오는 중...";

        // 1. UnityWebRequest 객체 생성
        UnityWebRequest www = UnityWebRequest.Get(rankingUrl);

        // 2. (핵심) 커스텀 헤더 추가
        www.SetRequestHeader("x-api-key", apiKey);

        // 3. 웹 요청 보내고 응답 기다리기
        yield return www.SendWebRequest();

        // 4. 결과 확인
        if (www.result == UnityWebRequest.Result.Success)
        {
            // 5. 성공: JSON 데이터 파싱
            string rawJson = www.downloadHandler.text;

            // 6. JsonUtility가 배열을 읽을 수 있도록 수동으로 Wrapper 씌우기
            string wrappedJson = "{\"items\":" + rawJson + "}";

            // 7. JSON을 RankingListWrapper 클래스로 변환
            RankingListWrapper wrapper = JsonUtility.FromJson<RankingListWrapper>(wrappedJson);

            // 8. (선택 사항) 점수(score) 기준으로 내림차순 정렬
            //wrapper.items.Sort((a, b) => b.score.CompareTo(a.score));

            // 9. UI 생성
            if (statusText != null) statusText.text = "랭킹"; // 로딩 완료
            DisplayRankings(wrapper.items);
        }
        else
        {
            // 10. 실패: 에러 메시지 표시
            Debug.LogError("랭킹 로드 실패: " + www.error);
            if (statusText != null) statusText.text = "랭킹을 불러올 수 없습니다.\n" + www.error;
        }

    }

    private void DisplayRankings(List<RankingEntry> rankingList)
    {
        // 기존에 있던 UI 항목들 삭제 (새로고침을 위해)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 랭킹 리스트를 순회하며 UI 생성
        int rank = 1;
        foreach (RankingEntry entry in rankingList)
        {
            GameObject entryGO = Instantiate(rankingEntryPrefab, contentParent);
            TextMeshProUGUI entryText = entryGO.GetComponent<TextMeshProUGUI>();

            if (entryText != null)
            {
                // "1. 구준모 - 2147483647" 형식으로 텍스트 설정
                entryText.text = $"{rank}. {entry.playerName} - {entry.score}";
                rank++;
            }
        }
    }
}
