using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingEntryUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public Image backgroundImage;

    public Image isEndImage; // 인스펙터에서 'IsEnd' 오브젝트의 Image 연결
    private Color isEndGrayColor = new Color(0.368f, 0.368f, 0.368f); // #5E5E5E 색상을 저장할 변수

    [Header("등수별 색상")]
    // (참고: new Color(R, G, B) 값은 0~1 사이의 float 값입니다)
    public Color goldColor = new Color(0.898f, 0.765f, 0.2f, 0.666f); // 금색 (RGB: 255, 215, 0)
    public Color silverColor = new Color(0.75f, 0.75f, 0.75f, 0.666f); // 은색 (RGB: 192, 192, 192)
    public Color bronzeColor = new Color(0.486f, 0.255f, 0.098f, 0.666f); // 구리색 (RGB: 205, 127, 50)
    public Color defaultColor = new Color(0.231f, 0.231f, 0.231f); // 4등 이하 기본색


    /// <summary>
    /// RankingSystem이 이 함수를 호출하여 텍스트 값을 설정합니다.
    /// </summary>
    /// <param name="rank">등수</param>
    /// <param name="playerName">플레이어 이름</param>
    /// <param name="score">점수</param>
    /// <param name="isEnd">클리어유무</param>
    public void SetData(int rank, string playerName, long score, int isEndValue)
    {
        // 2. 연결된 텍스트 컴포넌트의 내용을 변경합니다.
        if (rankText != null)
            rankText.text = $"{rank}등"; // "1등", "2등" 형식으로 표시

        if (nameText != null)
            nameText.text = playerName;

        if (scoreText != null)
            // "N0"는 숫자에 1,000 단위 콤마(,)를 찍어줍니다 (예: 2,147,483,647)
            scoreText.text = score.ToString("N0");

        if (backgroundImage != null)
        {
            switch (rank)
            {
                case 1:
                    backgroundImage.color = goldColor;
                    break;
                case 2:
                    backgroundImage.color = silverColor;
                    break;
                case 3:
                    backgroundImage.color = bronzeColor;
                    break;
                default:
                    // 4등 이하
                    backgroundImage.color = defaultColor;
                    break;
            }
        }

        // --- (추가) isEnd 이미지 색상 설정 ---
        if (isEndImage != null)
        {
            if (isEndValue == 1)
            {
                // 1이면 흰색
                isEndImage.color = Color.white;
            }
            else // 0 또는 그 외
            {
                // 0이면 #5E5E5E
                isEndImage.color = isEndGrayColor;
            }
        }
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
