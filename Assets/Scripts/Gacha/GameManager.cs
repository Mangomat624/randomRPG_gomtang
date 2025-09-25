// GameManager.cs

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int currentStage = 1;
    public int tokens = 44;

    void Awake()
    {
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

    public int GetCurrentStage() => currentStage;
    public int GetCurrentTokens() => tokens;
    public void UseTokens(int amount) => tokens -= amount;
}