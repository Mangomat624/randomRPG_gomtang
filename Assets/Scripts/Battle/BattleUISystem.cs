using System; // System.Action을 사용하기 위해 필요합니다.
using System.Collections; // 코루틴 사용
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterStats;


public class BattleUISystem : MonoBehaviour
{
    [Header("Player")]

    public Slider P_Healthbar; // 인스펙터 창에서 Slider 컴포넌트를 연결할 변수
    public TextMeshProUGUI P_HPText;
    public TextMeshProUGUI PlayerText;

    [Header("Enemy")]

    public Slider E_Healthbar; // 인스펙터 창에서 Slider 컴포넌트를 연결할 변수
    public TextMeshProUGUI E_HPText;
    public TextMeshProUGUI EnemyText;

    public float changeSpeed = 0.5f;
    private Coroutine _healthChangeCoroutine;

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
