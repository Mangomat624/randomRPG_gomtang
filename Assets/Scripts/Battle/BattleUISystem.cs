using System; // System.Action을 사용하기 위해 필요합니다.
using System.Collections; // 코루틴 사용
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterStats;


public class BattleUISystem : MonoBehaviour
{
    [Header("Default Settings")]
    public float changeSpeed = 0.5f;
    public CharacterStats playerStats; // 플레이어의 CharacterStats 컴포넌트
    public CharacterStats enemyStats;  // 적의 CharacterStats 컴포넌트

    public SpriteRenderer backgroundRenderer;
    public List<Sprite> stageBackgrounds;

    [Header("Player")]

    public Slider P_Healthbar; // 인스펙터 창에서 Slider 컴포넌트를 연결할 변수
    public TextMeshProUGUI P_HPText;
    public TextMeshProUGUI PlayerText;

    [Header("Enemy")]

    public Slider E_Healthbar; // 인스펙터 창에서 Slider 컴포넌트를 연결할 변수
    public TextMeshProUGUI E_HPText;
    public TextMeshProUGUI EnemyText;


    private Coroutine _healthChangeCoroutine;

    void Awake()
    {
        ChangeName(playerStats.unitName, enemyStats.unitName);

        int currentStage = GameManager.Instance.GetCurrentStage(); // 예시 호출
        SetBackgroundForStage(currentStage);
    }

    public void SetBackgroundForStage(int stageNumber)
    {
        Sprite backgroundToSet = null; // 설정할 배경 스프라이트

        // 스테이지 범위에 따라 배경 선택
        switch (stageNumber) // 예: 1~3 스테이지
        {
            case >= 1 and <= 3:
                // 리스트의 첫 번째(인덱스 0) 배경 사용
                if (stageBackgrounds.Count > 0)
                    backgroundToSet = stageBackgrounds[0];
                break;
            case >= 4 and <= 6:
                // 리스트의 두 번째(인덱스 1) 배경 사용
                if (stageBackgrounds.Count > 1)
                    backgroundToSet = stageBackgrounds[1];
                break;
            case >= 7 and <= 9:
                // 리스트의 두 번째(인덱스 1) 배경 사용
                if (stageBackgrounds.Count > 2)
                    backgroundToSet = stageBackgrounds[2];
                break;
            case >= 10:
                if (stageBackgrounds.Count > 3)
                    backgroundToSet = stageBackgrounds[3];
                break;
            default:
                // 리스트에 배경이 있다면 마지막 배경을 기본값으로 사용하거나,
                // 특정 인덱스(예: 0)를 기본값으로 사용할 수 있습니다.
                if (stageBackgrounds.Count > 0)
                    backgroundToSet = stageBackgrounds[0]; // 예시: 첫 번째 배경을 기본값으로
                break;
        }

        // 선택된 배경이 있고, 렌더러가 연결되어 있다면 적용
        if (backgroundRenderer != null && backgroundToSet != null)
        {
            backgroundRenderer.sprite = backgroundToSet;
        }
        else if (backgroundRenderer == null)
        {
            Debug.LogWarning("Background Renderer가 연결되지 않았습니다!");
        }
        else
        {
            Debug.LogWarning($"스테이지 {stageNumber}에 해당하는 배경 이미지를 찾을 수 없습니다!");
        }
    }

    // 체력바의 최대값을 설정하는 함수
    public void SetMaxHealth(CharacterStats.PlayerType type, int health)
    {
        Slider Healthbar;
        TextMeshProUGUI HPText;
        switch (type)
        {
            case PlayerType.Player:
                Healthbar = P_Healthbar;
                HPText = P_HPText;
                break;
            case PlayerType.Enemy:
                Healthbar = E_Healthbar;
                HPText = E_HPText;
                break;
            default:
                // (추가) Player도 Enemy도 아닌 경우
                // 경고를 출력하고 함수를 종료합니다.
                Debug.LogWarning("SetMaxHealth: 알 수 없는 PlayerType입니다: " + type);
                return; // <- 이 return 덕분에 아래 코드가 실행되지 않습니다.
        }

        Healthbar.maxValue = health;
        Healthbar.value = health;
        HPText.text = "HP : " + health.ToString();
    }

    // 현재 체력을 받아서 슬라이더의 값에 반영하는 함수
    public void SetHealth(CharacterStats.PlayerType type,int health, Action onAnimationComplete = null)
    {
        if (_healthChangeCoroutine != null)
        {
            StopCoroutine(_healthChangeCoroutine);
        }

        switch (type)
        {
            case PlayerType.Player:
                P_HPText.text = "HP : " + health.ToString();
                break;
            case PlayerType.Enemy:
                E_HPText.text = "HP : " + health.ToString();
                break;
        }

        // 새로운 체력 변경 코루틴을 시작하고, 변수에 저장해 둡니다.
        _healthChangeCoroutine = StartCoroutine(ChangeHealthSmoothly(type, health, onAnimationComplete));
    }

    private IEnumerator ChangeHealthSmoothly(CharacterStats.PlayerType type, int newHealth, Action onAnimationComplete)
    {
        Slider Healthbar;
        switch (type)
        {
            case PlayerType.Player:
                Healthbar = P_Healthbar;
                break;
            case PlayerType.Enemy:
                Healthbar = E_Healthbar;
                break;
            default:
                // (추가) Player도 Enemy도 아닌 경우
                // 경고를 출력하고 함수를 종료합니다.
                Debug.LogWarning("SetMaxHealth: 알 수 없는 PlayerType입니다: " + type);
                yield break;
        }

        float timer = 0f;
        float startHealth = Healthbar.value; // 시작 체력 (현재 슬라이더 값)
        // timer가 changeSpeed에 도달할 때까지 반복합니다.
        while (timer < changeSpeed)
        {
            // 매 프레임 경과 시간을 timer에 더해줍니다.
            timer += Time.deltaTime;
            float t = timer / changeSpeed; // 0과 1 사이의 비율을 계산합니다.

            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            // Lerp를 사용해 시작 체력과 목표 체력 사이의 현재 지점 값을 계산하고 슬라이더에 적용
            Healthbar.value = Mathf.Lerp(startHealth, newHealth, t);
            // 다음 프레임까지 기다립니다.
            yield return null;
        }

        // 루프가 끝난 후, 정확한 목표 체력 값으로 맞춰줍니다.
        Healthbar.value = newHealth;

        onAnimationComplete?.Invoke();
    }

    public void ChangeName(string P_name, string E_name)
    {
        PlayerText.text = P_name;
        EnemyText.text = E_name;
    }
}
