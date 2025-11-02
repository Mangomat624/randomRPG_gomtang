using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int currentStage;
    public int tokens = 4;
    public string playerName = "플레이어"; // 기본 이름

    [Header("Player Equipment")]

    public EquipmentData.Grade CurrentHeadGrade;
    public EquipmentData.Grade CurrentTorsoGrade;
    public EquipmentData.Grade CurrentLegsGrade;
    public EquipmentData.Grade CurrentWeaponGrade;
    private bool[] gachaLockStates = new bool[4] { false, false, false, false };

    [Header("Player Permanent Stats")]
    // 강화 씬에서 얻은 영구 스탯을 저장할 변수
    public int permanentBonusAtk = 0;
    public int permanentBonusAcc = 0;
    public bool isEnd = false;

    [Header("누적 전투 기록 (게임 세션)")]
    private float gameStartTime; // 게임 시작 시간
    private int cumulativeTotalDamageDealt; // 누적 총 입힌 대미지
    private int cumulativeTotalDamageTaken; // 누적 총 받은 대미지 (완벽주의자용)
    public int xGradeCount = 0;

    // 토큰 개수 변경 시 UI 스크립트에 알려줄 이벤트
    public static event Action<int> OnTokensChanged;

    void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGameSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시, 현재 토큰 개수로 UI를 업데이트하도록 신호를 보냄
        OnTokensChanged?.Invoke(tokens);
        tokens = 4;
        currentStage = 1;
        isEnd = false;
    }

    /// <summary>
    /// 게임 세션이 처음 시작될 때 타이머와 누적 대미지를 0으로 설정합니다.
    /// </summary>
    public void InitializeGameSession()
    {
        gameStartTime = Time.time;
        cumulativeTotalDamageDealt = 0;
        cumulativeTotalDamageTaken = 0;
        // xGradeCount는 게임 전체에서 누적되므로 여기서 초기화하지 않습니다.
    }

    // --- 1. 이벤트 구독 신청 ---
    // 이 스크립트(오브젝트)가 활성화될 때 호출됩니다.
    void OnEnable()
    {
        // GachaManager의 OnGachaFinished 이벤트에
        // 나의 OnGachaDataReceived 함수를 "구독(+=)"합니다.
        // (GachaManager.Instance가 아닌 클래스 이름으로 접근하는 이유는 'static'이기 때문입니다)
        GachaManager.OnGachaFinished += OnGachaDataReceived;
    }


    // --- 2. 이벤트 구독 해지 (필수!) ---
    // 이 스크립트(오브젝트)가 비활성화될 때 호출됩니다.
    void OnDisable()
    {
        // 구독을 해지(-=)합니다.
        // 이렇게 하지 않으면 메모리 누수(Memory Leak)가 발생할 수 있습니다!
        GachaManager.OnGachaFinished -= OnGachaDataReceived;
    }

    public int GetCurrentStage() => currentStage;
    public int GetCurrentTokens() => tokens;

    /// <GachaUISystem>이 잠금 상태를 저장할 때 호출합니다.
    public void UpdateGachaLocks(bool[] locks)
    {
        if (locks.Length == 4)
        {
            gachaLockStates = locks;
        }
    }

    /// <GachaUISystem>이 씬에 다시 로드될 때 잠금 상태를 불러오기 위해 호출합니다.
    public bool[] GetGachaLocks()
    {
        return gachaLockStates;
    }

    /// <summary>
    /// 플레이어가 적에게 입힌 대미지를 누적합니다.
    /// </summary>
    public void AddDamageDealt(int amount)
    {
        cumulativeTotalDamageDealt += amount;
    }

    /// <summary>
    /// 플레이어가 적에게 받은 대미지를 누적합니다.
    /// </summary>
    public void AddDamageTaken(int amount)
    {
        cumulativeTotalDamageTaken += amount;
    }


    public int GetTotalDamageDealt() { return cumulativeTotalDamageDealt; }
    public int GetTotalDamageTaken() { return cumulativeTotalDamageTaken; }

    /// <summary>
    /// 전투에 소요된 시간을 초(float) 단위로 반환합니다.
    /// </summary>
    public float GetGamePlayTime()
    {
        // 전투가 아직 안 끝났을 수도 있으니 현재 시간으로 계산
        return Time.time - gameStartTime;
    }

    /// <summary>
    /// 토큰을 사용하고, 변경된 개수를 UI에 알립니다.
    /// </summary>
    public void UseTokens(int amount)
    {
        tokens -= amount;
        OnTokensChanged?.Invoke(tokens);
    }

    /// <summary>
    /// 토큰을 추가하고(과금/테스트용), 변경된 개수를 UI에 알립니다.
    /// </summary>
    public void AddTokens(int amount)
    {
        tokens += amount;
        OnTokensChanged?.Invoke(tokens);
    }

    /// <summary>
    /// 강화 씬에서 얻은 영구 스탯을 GameManager에 저장합니다.
    /// </summary>
    public void ApplyPermanentEnhancement(int atk, int acc)
    {
        permanentBonusAtk = atk;
        permanentBonusAcc = acc;
        Debug.Log($"영구 스탯 저장 완료: ATK +{permanentBonusAtk}, ACC + {permanentBonusAcc}%");
    }

    private void OnGachaDataReceived(Dictionary<EquipmentData.PartType, EquipmentData> gachaResult)
    {
        {
            // 'gachaResult' 변수가 바로 GachaManager가 보낸 'resultData'입니다.
            // 이제 이 데이터를 마음대로 사용할 수 있습니다.
            foreach (var pair in gachaResult)
            {

                EquipmentData.PartType part = pair.Key;
                EquipmentData data = pair.Value;

                Debug.Log($"새로 뽑은 부위: {part}, 등급: {data.grade}");

                switch (part)
                {
                    case EquipmentData.PartType.Head:
                        CurrentHeadGrade = data.grade;
                        break;
                    case EquipmentData.PartType.Torso:
                        CurrentTorsoGrade = data.grade;
                        break;
                    case EquipmentData.PartType.Legs:
                        CurrentLegsGrade = data.grade;
                        break;
                    case EquipmentData.PartType.Weapon:
                        CurrentWeaponGrade = data.grade;
                        break;
                }
            }
        }
    }
}