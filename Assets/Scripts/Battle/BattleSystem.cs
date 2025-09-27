using UnityEngine;
using System.Collections;
using Unity.VisualScripting; // 코루틴을 사용하기 위해 꼭 필요해요!

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public CharacterStats playerStats; // 플레이어의 CharacterStats 컴포넌트
    public CharacterStats enemyStats;  // 적의 CharacterStats 컴포넌트

    public BattleState state; // 현재 전투 상태

    [Header("Minigames")]
    // 두 미니게임을 모두 연결할 변수
    public UndertaleAttack undertaleGame;

    private bool isActionDone = false;
    private AttackResult currentAttackResult;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        undertaleGame.gameObject.SetActive(false);
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

        isActionDone = false;

            Debug.Log("이번 턴은 언더테일 미니게임!");
            // 두 게임 모두 동일한 콜백 함수(HandleAttackResult)를 사용합니다.
            undertaleGame.StartMinigame(HandleAttackResult);


        // isMinigameDone 플래그가 true가 될 때까지 코루틴은 여기서 기다립니다.
        while (!isActionDone)
        {
            yield return null;
        }

        // 1. 적중률 계산
        int totalAccuracy = playerStats.baseAccuracy + playerStats.weaponAccuracy + playerStats.enhancementAccuracy + currentAttackResult.accuracyBonus;
        int hitChance = totalAccuracy - enemyStats.evasionRate;

        // 2. 명중 판정 (랜덤)
        int roll = Random.Range(1, 101); // 1~100 사이의 랜덤 숫자
        if (roll <= hitChance)
        {
            // 3. 명중 시 데미지 계산 및 적용
            Debug.Log("명중! (적중률: " + hitChance + "%, 주사위: " + roll + ")");
            int damage = Mathf.RoundToInt(playerStats.baseDamage * currentAttackResult.damageMultiplier);
            bool isDead = enemyStats.TakeDamage(damage);
            if (isDead)
            {
                state = BattleState.WON;
                EndBattle();
                yield break; // 전투 종료
            }
        }
        else
        {
            Debug.Log("빗나감! (적중률: " + hitChance + "%, 주사위: " + roll + ")");
        }

        yield return new WaitForSeconds(1.5f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    // 미니게임이 끝나면 undertalegame 스크립트가 이 함수를 호출합니다.
    public void HandleAttackResult(AttackResult result)
    {
        currentAttackResult = result;
        isActionDone = true;
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("적의 턴!");
        yield return new WaitForSeconds(1f); // 적이 생각하는 시간처럼 잠시 대기

        // 1. 플레이어의 1차 회피 판정
        int monsterAccuracy = 100; // 몬스터 적중은 100
                                   // 으로 고정
        int evasionChance = playerStats.evasionRate;
        int roll = Random.Range(1, 101);

        if (roll <= evasionChance)
        {
            Debug.Log("회피 성공! (회피율: " + evasionChance + "%, 주사위: " + roll + ")");
        }
        else
        {
            Debug.Log("회피 실패. (회피율: " + evasionChance + "%, 주사위: " + roll + ")");
            // 2. 1차 회피 실패 시, 회피 미니게임 등장 (현재는 바로 데미지)
            // TODO: 여기에 매우 어려운 회피 미니게임을 호출하는 로직을 추가해야 합니다.
            // 성공 시 return; 하여 데미지를 입지 않게 합니다.

            Debug.Log("플레이어가 적의 공격을 받습니다.");
            bool isDead = playerStats.TakeDamage(enemyStats.baseDamage);
            if (isDead)
            {
                state = BattleState.LOST;
                EndBattle();
                yield break; // 전투 종료
            }

        }

        yield return new WaitForSeconds(1.5f);

        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurn());
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
