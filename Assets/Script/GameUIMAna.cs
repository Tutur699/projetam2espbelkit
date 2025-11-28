using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class GameUIMAna : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject Pannel_Lobby;
    [SerializeField] private GameObject Pannel_InGame;
    [SerializeField] private GameObject Pannel_GameOver;
    [SerializeField] private GameObject Pannel_Pause;
    [Header("Player Components")]
    public FPC_PLAYER playerCamera;
    public PlayerMovement movementScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isGameOver = false;
    private bool lanStarted = false;
    private bool isPaused = false;
    void Start()
    {
        lanStarted = false;
        isGameOver = false;
        isPaused = false;

        if (Pannel_Lobby != null)
            Pannel_Lobby.SetActive(true);
        if (Pannel_InGame != null)
            Pannel_InGame.SetActive(false);
        if (Pannel_GameOver != null)
            Pannel_GameOver.SetActive(false);
        if (Pannel_Pause != null)
            Pannel_Pause.SetActive(false);

        // On bloque le contrôle du joueur tant que la partie n’est pas lancée
        if (playerCamera != null)
            playerCamera.enabled = false;
        if (movementScript != null)
            movementScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }
    // Update is called once per frame
    void Update()
    {
        // Gestion de l’affichage des panneaux UI selon l’état du jeu
        // Lobby visible tant que la LAN n’est pas démarrée
        if (Pannel_Lobby != null)
            Pannel_Lobby.SetActive(!lanStarted);

        // GameOver visible seulement si game over
        if (Pannel_GameOver != null)
            Pannel_GameOver.SetActive(isGameOver);

        // HUD visible si en jeu ET pas en pause ET pas game over
        if (Pannel_InGame != null)
            Pannel_InGame.SetActive(lanStarted && !isPaused && !isGameOver);

        // Menu pause visible si en pause (et pas game over)
        if (Pannel_Pause != null)
            Pannel_Pause.SetActive(isPaused && !isGameOver);
        // Gestion de la mise en pause via la touche ESCAPE
        if (lanStarted && !isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed, toggle pause");
            TogglePause();
        }
        
    }
    public void OnStartLan()
    {
        if (playerCamera != null)
            playerCamera.enabled = true;
        if (movementScript != null)
            movementScript.enabled = true;
        lanStarted = true;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void OnGameOver()
    {
        if (playerCamera != null)
            playerCamera.enabled = false;
        if (movementScript != null)
            movementScript.enabled = false;
        isGameOver = true;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void TogglePause()
    {
        isPaused = !isPaused;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = isPaused;

        // Désactive / active la caméra
        if (playerCamera != null)
            playerCamera.enabled = !isPaused;
        if (movementScript != null)
            movementScript.enabled = !isPaused;

    }
    public void OnMainMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }
    public void OnQuitGameButton()
    {
        Application.Quit();
    }
    public void OnResumeButton()
    {
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
            playerCamera.enabled = true;
        if (movementScript != null)
            movementScript.enabled = true;
    }
}
