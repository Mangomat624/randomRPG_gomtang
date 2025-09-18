using UnityEngine;
using System.Collections; // 코루틴 사용

public class CharacterStats : MonoBehaviour
{
    [Header("Info")]
    public string unitName; // 유닛 이름 (예: "플레이어", "슬라임")
    public int currentHP;   // 현재 체력

    [Header("Attack Stats")]
    public int baseDamage = 10; // 기본 공격력 (기존 attackPower)
    public int maxHP;       // 최대 체력
    public int baseAccuracy = 80; // 기본 적중
    public int weaponAccuracy = 0; // 무기 적중
    public int enhancementAccuracy = 0; // 강화 적중

    [Header("Defense Stats")]
    public int evasionRate = 10; // 회피율

    [Header("UI")]
    public HealthBar healthBar;

    [Header("Visual Effect")]
    public float squashDuration = 1f; // 찌그러지는 데 걸리는 시간


    void Awake()
    {
        currentHP = maxHP;
        healthBar.SetMaxHealth(maxHP);
    }


    public bool TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0; // HP가 음수가 되지 않도록


        // 체력이 0 이하로 떨어졌는지 확인
        if (currentHP <= 0)
        {
            Debug.Log(unitName + "이(가) 쓰러졌습니다.");

            healthBar.SetHealth(currentHP, () => {
                StartCoroutine(SquashEffect());
            });

            return true; // 쓰러졌음을 알림
        }
        else
        {
            // HP가 0보다 클 때:
            // 이전과 같이 콜백 없이 SetHealth만 호출합니다.
            healthBar.SetHealth(currentHP);
            Debug.Log(unitName + "이(가) " + damage + "의 피해를 입었습니다. 남은 체력: " + currentHP);
            return false; // 아직 쓰러지지 않았음을 알림
        }
    }

    // 캐릭터가 찌그러지는 효과를 연출하는 코루틴
    private IEnumerator SquashEffect()
    {
        Vector3 startScale = transform.localScale; // 현재 크기
        Vector3 endScale = new Vector3(startScale.x, 0, startScale.z); // 목표 크기 (Y축만 0)
        float timer = 0f;

        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // 부드러운 Ease-Out 효과 적용

            // Lerp를 사용해 시간에 따라 크기를 서서히 변경
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        // 정확한 목표 크기로 설정
        transform.localScale = endScale;

        // 애니메이션이 끝난 후 캐릭터 오브젝트를 비활성화.
        gameObject.SetActive(false);
    }
}
