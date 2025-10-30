using TMPro;
using UnityEngine;

public class RankingEntryUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    /// <summary>
    /// RankingSystem이 이 함수를 호출하여 텍스트 값을 설정합니다.
    /// </summary>
    /// <param name="rank">등수</param>
    /// <param name="playerName">플레이어 이름</param>
    /// <param name="score">점수</param>
    public void SetData(int rank, string playerName, long score)
    {
        // 2. 연결된 텍스트 컴포넌트의 내용을 변경합니다.
        if (rankText != null)
            rankText.text = $"{rank}등"; // "1등", "2등" 형식으로 표시

        if (nameText != null)
            nameText.text = playerName;

        if (scoreText != null)
            // "N0"는 숫자에 1,000 단위 콤마(,)를 찍어줍니다 (예: 2,147,483,647)
            scoreText.text = score.ToString("N0");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
