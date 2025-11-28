using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets; 
using Unity.Netcode;

public class GameUIMAna : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject Pannel_Lobby;
    [SerializeField] private GameObject Pannel_InGame;
    [SerializeField] private GameObject Pannel_GameOver;
    [SerializeField] private GameObject Pannel_Pause;

    [Header("Player Components")]
    public FPC_PLAYER playerController;

    private bool isGameOver = false;
    private bool lanStarted = false;
    private bool isPaused = false;

    void Start()
    {
        lanStarted = false;
        isGameOver = false;
        isPaused = false;

        if (Pannel_Lobby != null) Pannel_Lobby.SetActive(true);
        if (Pannel_InGame != null) Pannel_InGame.SetActive(false);
        if (Pannel_GameOver != null) Pannel_GameOver.SetActive(false);
        if (Pannel_Pause != null) Pannel_Pause.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        TryFindLocalPlayer();
    }

    private void TryFindLocalPlayer()
    {
        FPC_PLAYER[] allPlayers = FindObjectsOfType<FPC_PLAYER>();

        foreach (var p in allPlayers)
        {
            if (p.IsOwner) // joueur local
            {
                Debug.Log("[UI] Local player found: " + p.name);
                playerController = p;
                break;
            }
        }
    }

    void Update()
    {
        // tant qu’on n’a pas trouvé le joueur local, on essaie
        if (playerController == null)
        {
            TryFindLocalPlayer();
            return; // on attend le prochain frame
        }

        // sécurité : on ne gère que le joueur local
        if (!playerController.IsOwner)
            return;

        if (Pannel_Lobby != null)
            Pannel_Lobby.SetActive(!lanStarted);

        if (Pannel_GameOver != null)
            Pannel_GameOver.SetActive(isGameOver);

        if (Pannel_InGame != null)
            Pannel_InGame.SetActive(lanStarted && !isPaused && !isGameOver);

        if (Pannel_Pause != null)
            Pannel_Pause.SetActive(isPaused && !isGameOver);

        if (lanStarted && !isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void OnStartLan()
    {
        lanStarted = true;
        isPaused = false;

        // si le joueur est déjà spawn, on le dé-pause
        if (playerController != null)
            playerController.isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TogglePause()
    {
        if (playerController == null || !playerController.IsOwner) return;

        isPaused = !isPaused;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        playerController.isPaused = isPaused;
    }

    public void OnResumeButton()
    {
        if (playerController == null || !playerController.IsOwner) return;

        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController.isPaused = false;
    }

    public void OnGameOver()
    {
        if (playerController == null || !playerController.IsOwner) return;

        isGameOver = true;
        isPaused = false;
        playerController.isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnMainMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnQuitGameButton()
    {
        Application.Quit();
    }
}
