using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public struct AttackResult
{
    public float damageMultiplier; // 최종 대미지 계산 배율
    public int accuracyBonus;      // 적중률 보너스
    public string judgement;       // 판정 이름 (Perfect, Great 등)
}

public class BattleSystem : MonoBehaviour
{
    public CharacterStats playerStats; // 플레이어의 CharacterStats 컴포넌트
    public CharacterStats enemyStats;  // 적의 CharacterStats 컴포넌트

    public BattleState state; // 현재 전투 상태

    [Header("UI")]
    public BattleUISystem battleUI;

    [Header("Minigames")]
    // 두 미니게임을 모두 연결할 변수
    public UndertaleAttack undertaleGame;
    public NoteManager pseckaGame;

    private bool isActionDone = false;
    private AttackResult currentAttackResult;

    public Material transitionMaterial;
    public float transitionSpeed = 1f; // 전환 속도

    public Texture openingPatternTexture;

    private long finalScore;
    public GameObject nextbutton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        undertaleGame.gameObject.SetActive(false);
        pseckaGame.gameObject.SetActive(false );
        nextbutton.gameObject.SetActive(false);
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {

        if (openingPatternTexture != null)
        {
            transitionMaterial.SetTexture("_TransitionTex", openingPatternTexture);
        }

        transitionMaterial.SetFloat("_Cutoff", 1f);

        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime * transitionSpeed;
            // 2. 셰이더의 _Cutoff 값을 1에서 0으로 변경
            transitionMaterial.SetFloat("_Cutoff", t);
            yield return null;
        }

        state = BattleState.START;

        // --- (추가) 각 캐릭터의 사망 애니메이션 완료 이벤트 구독 ---
        // 플레이어가 쓰러지면 HandleCharacterDeath 함수 호출
        playerStats.OnDeathAnimationFinished += HandleCharacterDeath;
        // 적이 쓰러지면 HandleCharacterDeath 함수 호출
        enemyStats.OnDeathAnimationFinished += HandleCharacterDeath;
        // --- 여기까지 ---

        Debug.Log("전투 준비!");



        Debug.Log(playerStats.unitName + " (HP: " + playerStats.currentHP + ") vs " + enemyStats.unitName + " (HP: " + enemyStats.currentHP + ")");

        yield return new WaitForSeconds(1f);

        // 준비가 끝나면 플레이어 턴으로 전환합니다.
        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurn());
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnDeathAnimationFinished -= HandleCharacterDeath;
        if (enemyStats != null)
            enemyStats.OnDeathAnimationFinished -= HandleCharacterDeath;
    }

    private void HandleCharacterDeath()
    {
        // EndBattle 코루틴을 시작시킵니다.
        StartCoroutine(EndBattleSequence());
    }

    IEnumerator PlayerTurn()
    {
        Debug.Log("플레이어의 턴!");

        isActionDone = false;


        // 0 또는 1을 랜덤으로 선택
        int minigameType = Random.Range(0, 2);

        //언더테일 테스트
        //int minigameType = Random.Range(0, 1);

        //프세카 테스트
        // minigameType = Random.Range(1, 2);

        if (minigameType == 0)
        {
            Debug.Log("이번 턴은 언더테일 미니게임!");
            undertaleGame.StartMinigame(HandleAttackResult);
        }
        else
        {
            Debug.Log("이번 턴은 프세카 미니게임!");
            pseckaGame.StartMinigame(HandleAttackResult);
        }

        // 미니게임이 끝날 때까지 대기
        while (!isActionDone)
        {
            yield return null;
        }

        // 1. 적중률 계산    
        int totalAccuracy = playerStats.baseAccuracy + playerStats.weaponAccuracy + playerStats.enhancementAccuracy + currentAttackResult.accuracyBonus+GameManager.Instance.permanentBonusAcc;
        int hitChance = 50; // 기본값
        if (totalAccuracy < enemyStats.evasionRate) {
            hitChance = 0;
        }
        else
        {
            hitChance = totalAccuracy - enemyStats.evasionRate;
        }

        // 2. 명중 판정 (랜덤)
        int roll = Random.Range(1, 101); // 1~100 사이의 랜덤 숫자
        if (roll <= hitChance)
        {
            // 3. 명중 시 데미지 계산 및 적용
            Debug.Log("명중! (적중률: " + hitChance + "%, 주사위: " + roll + ")");
            int damage = Mathf.RoundToInt((playerStats.baseDamage+GameManager.Instance.permanentBonusAtk) * currentAttackResult.damageMultiplier);

            // --- (추가) GameManager에 입힌 대미지 기록 ---
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddDamageDealt(damage);
            }

            bool isDead = enemyStats.TakeDamage(damage);
            if (isDead)
            {
                state = BattleState.WON;
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
        int monsterAccuracy = 100; // 몬스터 적중은 100으로 고정
        int evasionChance = (100 + playerStats.evasionRate) - monsterAccuracy; //회피율
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
            int enemydamage = enemyStats.baseDamage - playerStats.defensivePower;
            if (enemydamage < 0)
            {
                enemydamage = 0;
            }
            else 
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddDamageTaken(enemydamage);
                }
                bool isDead = playerStats.TakeDamage(enemydamage);
                if (isDead)
                {
                    state = BattleState.LOST;
                    yield break; // 전투 종료
                }
            }
        }

        yield return new WaitForSeconds(1.5f);

        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurn());
    }

    IEnumerator GameEndSequence()
    {
        if (state == BattleState.LOST)
        {
            GameManager.Instance.isEnd = false;
        }
        else { GameManager.Instance.isEnd = true; }

        // 1. 점수 계산
        finalScore = CalculateFinalScore(GameManager.Instance.isEnd);

        yield break;
    }

    IEnumerator EndBattleSequence()
    {
        if (state == BattleState.WON)
        {
            Debug.Log("승리했습니다!");
            GameManager.Instance.currentStage += 1;
            if (GameManager.Instance.currentStage >= 11)
            {
                Debug.Log("클리어");
                StartCoroutine(GameEndSequence());
                yield return StartCoroutine(PostScoreCoroutine(playerStats.unitName, finalScore));
                GameManager.Instance.hasNewRankToShow = true;
            }
            else
            {
                yield return new WaitForSeconds(1f);
                switch (GameManager.Instance.currentStage)
                {
                    case 4:
                        SceneManager.LoadScene("뽑기");
                        GameManager.Instance.tokens += 6;
                        break;
                    case 7:
                        SceneManager.LoadScene("뽑기");
                        GameManager.Instance.tokens += 8;
                        break;
                    case 10:
                        SceneManager.LoadScene("뽑기");
                        GameManager.Instance.tokens += 8;
                        break;
                    default:
                        nextbutton.SetActive(true);
                        break;

                        
                }
            }
        }
        else if (state == BattleState.LOST)
        {   
            Debug.Log("패배했습니다...");
            StartCoroutine(GameEndSequence());
            yield return StartCoroutine(PostScoreCoroutine(playerStats.unitName, finalScore));
            GameManager.Instance.hasNewRankToShow = true;
        }
    }
    private long CalculateFinalScore(bool isClear)
    {
        if (GameManager.Instance == null) return 0;

        // 1. 기본 점수: 게임 전체에서 입힌 총 대미지 (누적됨)
        long score = GameManager.Instance.GetTotalDamageDealt();

        if (isClear)
        {
            score += 3000;

            // 2. 시간 보너스: (게임 전체 플레이 시간)
            float playTime = GameManager.Instance.GetGamePlayTime(); // <- 함수 이름 변경
            int timeBonus = Mathf.Max(0, 1000 - Mathf.RoundToInt(playTime * 3));
            score += timeBonus;

            // 3. 완벽주의자: (게임 전체에서 받은 총 대미지) (누적됨)
            if (GameManager.Instance.GetTotalDamageTaken() <= 777)
            {
                score += 777;
            }
        }

        // 세트 보너스 (F등급 제외)
        var gm = GameManager.Instance;
        var head = gm.CurrentHeadGrade;
        if (head != EquipmentData.Grade.F &&
            head == gm.CurrentTorsoGrade &&
            head == gm.CurrentLegsGrade &&
            head == gm.CurrentWeaponGrade)
        {
            switch (head)
            {
                case EquipmentData.Grade.D: score += 200; break;
                case EquipmentData.Grade.C: score += 400; break;
                case EquipmentData.Grade.B: score += 500; break;
                case EquipmentData.Grade.A: score += 700; break;
                case EquipmentData.Grade.S: score += 1000; break;
                case EquipmentData.Grade.SS: score += 2000; break;
                case EquipmentData.Grade.SSS: score += 3000; break;
                    // X등급은 세트 보너스가 있는지 불명확하여 제외
            }
        }

        // ??? 보너스 (X등급)
        score += (gm.xGradeCount * 500);

        return score;
    }

    [System.Serializable]
    private class PostRankingData
    {
        public string playerName;
        public long score;
        public int isEnd;
    }

    private IEnumerator PostScoreCoroutine(string playerName, long finalScore) // <- isClear 파라미터 추가
    {
        string url = "https://server-mango.kro.kr/db/ranking";
        string apiKey = "MangoKey_43f83185-67e3-4f77-83a9-35b951e0af8d";

        bool didClear = GameManager.Instance.isEnd;

        // 1. 보낼 데이터 JSON으로 생성 (isClear 포함)
        PostRankingData data = new PostRankingData
        {
            playerName = playerName,
            score = finalScore,
            isEnd = System.Convert.ToInt32(didClear)
        };
        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // 2. UnityWebRequest (POST) 생성
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        // 3. 헤더 설정
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("X-Api-Key", apiKey);

        // 4. 요청 보내기
        Debug.Log($"랭킹 점수 전송 시도... Player: {playerName}, Score: {finalScore}, Clear: {didClear}");
        yield return www.SendWebRequest();

        // 5. 결과 확인
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("랭킹 전송 성공! 응답: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogWarning("랭킹 전송 실패: " + www.error + "\n응답: " + www.downloadHandler.text);
        }
    }
}
