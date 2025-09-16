using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class HeatlhBr : MonoBehaviour
{
    protected float curHealth; //* 현재 체력
    public float maxHealth; //* 최대 체력
    public float damage;
    protected bool dead = false;
    public void SetHp(float amount) //*Hp설정
    {
        maxHealth = amount;
        curHealth = maxHealth;
    }

    public Slider HpBarSlider;
    public GameObject Hpbar;

    public void CheckHp() //*HP 갱신
    {
        if (HpBarSlider != null)
            HpBarSlider.value = curHealth;
    }

    public void Damage(float damage) //* 데미지 받는 함수
    {
        if (maxHealth == 0 || curHealth <= 0) //* 이미 체력 0이하면 패스
            return;
        curHealth -= damage;
        CheckHp(); //* 체력 갱신
        if (curHealth <= 0)
        {
            Debug.Log("죽음");
            dead = true;
            Hpbar.SetActive(false);

            
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetHp(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            Damage(damage);
            Debug.Log("대미지 " + curHealth);
            Thread.Sleep(1000);
        }
    }
}
