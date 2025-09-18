using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 코루틴 사용
using System; // System.Action을 사용하기 위해 필요합니다.


public class HealthBar : MonoBehaviour
{
    public Slider slider; // 인스펙터 창에서 Slider 컴포넌트를 연결할 변수

    public float changeSpeed = 0.5f;
    private Coroutine _healthChangeCoroutine;

    // 체력바의 최대값을 설정하는 함수
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    // 현재 체력을 받아서 슬라이더의 값에 반영하는 함수
    public void SetHealth(int health, Action onAnimationComplete = null)
    {
        if (_healthChangeCoroutine != null)
        {
            StopCoroutine(_healthChangeCoroutine);
        }

        // 새로운 체력 변경 코루틴을 시작하고, 변수에 저장해 둡니다.
        _healthChangeCoroutine = StartCoroutine(ChangeHealthSmoothly(health, onAnimationComplete));
    }

    private IEnumerator ChangeHealthSmoothly(int newHealth, Action onAnimationComplete)
    {
        float timer = 0f;
        float startHealth = slider.value; // 시작 체력 (현재 슬라이더 값)

        // timer가 changeSpeed에 도달할 때까지 반복합니다.
        while (timer < changeSpeed)
        {
            // 매 프레임 경과 시간을 timer에 더해줍니다.
            timer += Time.deltaTime;
            float t = timer / changeSpeed; // 0과 1 사이의 비율을 계산합니다.

            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            // Lerp를 사용해 시작 체력과 목표 체력 사이의 현재 지점 값을 계산하고 슬라이더에 적용
            slider.value = Mathf.Lerp(startHealth, newHealth, t);

            // 다음 프레임까지 기다립니다.
            yield return null;
        }

        // 루프가 끝난 후, 정확한 목표 체력 값으로 맞춰줍니다.
        slider.value = newHealth;

        onAnimationComplete?.Invoke();
    }
}
