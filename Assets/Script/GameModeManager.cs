using UnityEngine;

public enum GameMode
{
    Solo,
    Multiplayer
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    public GameMode CurrentGameMode { get; private set; } = GameMode.Solo;
    
    private void Awake()
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
    
    public void SetGameMode(GameMode mode)
    {
        CurrentGameMode = mode;
        Debug.Log($"GameMode set to: {mode}");
    }
}