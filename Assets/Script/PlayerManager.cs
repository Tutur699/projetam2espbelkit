using StarterAssets;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IEntity
{
    [Header("Vie du joueur")]
    public float maxHP = 100f;
    public float playerHP = 100f;

    [Header("Références")]
    public FPC_PLAYER playerControler;
    public WPManager weaponManager;

    [Header("UI")]
    public Texture crosshairTexture;

    private bool isDead = false;

    void Start()
    {
        // On s'assure que la vie est bien initialisée
        playerHP = Mathf.Clamp(playerHP, 0, maxHP);
    }

    public void ApplyDamage(float points)
    {
        if (isDead) return; // Si déjà mort, on ignore les dégâts

        playerHP -= points;

        if (playerHP <= 0)
        {
            playerHP = 0;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // On bloque la caméra
        if (playerControler != null)
        {
            playerControler.LockCameraPosition = true;
            playerControler.enabled = false; // plus de déplacements
        }

        // ⚠ On NE désactive PAS weaponManager ici,
        // c’est lui qui va se bloquer tout seul en fonction de IsAlive()

        // Optionnel : mettre le jeu en pause totale
        // Time.timeScale = 0f;

        // Optionnel : libérer la souris pour les menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Tuer le joueur avec la touche K pour les tests
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!isDead){
                Debug.Log("SUICIDE COMMAND ADMIN !"); 
                ApplyDamage(9999f);
            }
        }
    }

    void OnGUI()
    {
        // Affichage des HP
        GUI.Box(
            new Rect(10, Screen.height - 35, 300, 75),
            ((int)playerHP).ToString() + " HP"
        );

        if (isDead)
        {
            // Game Over
            GUI.Box(
                new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40),
                "Game Over"
            );
        }
        else
        {
            // Tant que le joueur est VIVANT ⇒ afficher le viseur
            if (crosshairTexture != null)
            {
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 - 3, Screen.height / 2 - 3, 6, 6),
                    crosshairTexture
                );
            }
        }
    }

    // Petite fonction utilitaire si tu veux savoir si le joueur est vivant
    public bool IsAlive()
    {
        return !isDead;
    }
}