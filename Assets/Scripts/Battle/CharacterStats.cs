using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public string unitName; // 유닛 이름 (예: "플레이어", "슬라임")
    public int maxHP;       // 최대 체력
    public int currentHP;   // 현재 체력
    public int attackPower; // 공격력

    public HeatlhBar healthBar;

    void Awake()
    {
        currentHP = maxHP;
        healthBar.SetMaxHealth(maxHP);
    }


    public bool TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0; // HP가 음수가 되지 않도록

        healthBar.SetHealth(currentHP);

        // 체력이 0 이하로 떨어졌는지 확인
        if (currentHP <= 0)
        {
            Debug.Log(unitName + "이(가) 쓰러졌습니다.");
            return true; // 쓰러졌음을 알림
        }
        else
        {
            Debug.Log(unitName + "이(가) " + damage + "의 피해를 입었습니다. 남은 체력: " + currentHP);
            return false; // 아직 쓰러지지 않았음을 알림
        }
    }
}
