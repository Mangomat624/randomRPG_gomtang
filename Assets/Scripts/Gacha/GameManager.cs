// GameManager.cs

using UnityEngine;
using System; // ▼▼▼ 1. 이 줄이 있는지 확인 ▼▼▼

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int currentStage = 1;
    public int tokens = 44;

    // ▼▼▼ 2. 이 이벤트 선언이 있는지 확인 ▼▼▼
    public static event Action<int> OnTokensChanged;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // ▼▼▼ 3. 게임 시작 시 신호를 보내는지 확인 ▼▼▼
        OnTokensChanged?.Invoke(tokens);
    }

    public int GetCurrentStage() => currentStage;
    public int GetCurrentTokens() => tokens;

    public void UseTokens(int amount)
    {
        tokens -= amount;
        // ▼▼▼ 4. 토큰 사용 시 신호를 보내는지 확인 ▼▼▼
        OnTokensChanged?.Invoke(tokens);
    }
    public void AddTokens(int amount)
    {
        tokens += amount;
        OnTokensChanged?.Invoke(tokens);
        //이거 과금시스템 구현해놓은거임ㅇㅇ 이거 그 뭐냐 debugtokenadder.cs랑 연결시키는거임
    }
}