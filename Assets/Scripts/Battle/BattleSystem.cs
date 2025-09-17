using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 꼭 필요해요!

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public CharacterStats playerStats; // 플레이어의 CharacterStats 컴포넌트
    public CharacterStats enemyStats;  // 적의 CharacterStats 컴포넌트

    public BattleState state; // 현재 전투 상태

    // AttackMinigame 스크립트를 연결할 변수 추가
    public AttackMinigame attackMinigame;

    private bool isMinigameDone = false;
    private float damageMultiplier = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attackMinigame.gameObject.SetActive(false);
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        state = BattleState.START;
        Debug.Log("전투 준비!");

        Debug.Log(playerStats.unitName + " (HP: " + playerStats.currentHP + ") vs " + enemyStats.unitName + " (HP: " + enemyStats.currentHP + ")");

        yield return new WaitForSeconds(1f);

        // 준비가 끝나면 플레이어 턴으로 전환합니다.
        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurn());
    }
    IEnumerator PlayerTurn()
    {
        Debug.Log("플레이어의 턴!");

        isMinigameDone = false; // 플래그 초기화

        // 미니게임을 시작시키고, 결과가 오면 HandleMinigameResult 함수가 실행되도록 연결
        attackMinigame.StartMinigame(HandleMinigameResult);

        // isMinigameDone 플래그가 true가 될 때까지 코루틴은 여기서 기다립니다.
        while (!isMinigameDone)
        {
            yield return null;
        }

        int damage = Mathf.RoundToInt(playerStats.attackPower * damageMultiplier);
        Debug.Log("플레이어가 " + damage + "의 피해를 입힙니다!");
        bool isDead = enemyStats.TakeDamage(damage);

        yield return new WaitForSeconds(1.5f); // 공격 후 잠시 대기

        if (isDead)
        {
            // 적이 쓰러졌다면 전투 상태를 WON으로 바꾸고 전투를 종료합니다.
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            // 아직 살아있다면, 적의 턴으로 넘어갑니다.
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    // 미니게임이 끝나면 AttackMinigame 스크립트가 이 함수를 호출합니다.
    public void HandleMinigameResult(float multiplier)
    {
        damageMultiplier = multiplier;
        isMinigameDone = true;
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("적의 턴!");
        yield return new WaitForSeconds(1f); // 적이 생각하는 시간처럼 잠시 대기

        int damage = enemyStats.attackPower;
        bool isDead = playerStats.TakeDamage(damage);

        yield return new WaitForSeconds(1.5f);

        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            // 플레이어가 살아있다면, 다시 플레이어의 턴으로 돌아갑니다.
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurn());
        }
    }

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            Debug.Log("승리했습니다!");
        }
        else if (state == BattleState.LOST)
        {
            Debug.Log("패배했습니다...");
        }
    }

}
