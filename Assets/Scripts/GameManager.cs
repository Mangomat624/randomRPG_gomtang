using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int currentStage = 1;
    public int tokens = 44;

    [Header("Player Equipment")]

    public EquipmentData.Grade CurrentHeadGrade;
    public EquipmentData.Grade CurrentTorsoGrade;
    public EquipmentData.Grade CurrentLegsGrade;
    public EquipmentData.Grade CurrentWeaponGrade;

    [Header("Player Permanent Stats")]
    // 강화 씬에서 얻은 영구 스탯을 저장할 변수
    public int permanentBonusAtk = 0;
    public int permanentBonusAcc = 0;

    // 토큰 개수 변경 시 UI 스크립트에 알려줄 이벤트
    public static event Action<int> OnTokensChanged;

    void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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