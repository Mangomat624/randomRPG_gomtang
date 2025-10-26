using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int currentStage = 1;
    public int tokens = 44;

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
        Debug.Log($"영구 스탯 저장 완료: ATK +{permanentBonusAtk}, ACC +{permanentBonusAcc}%");
    }
}