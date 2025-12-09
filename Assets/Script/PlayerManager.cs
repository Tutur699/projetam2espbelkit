using StarterAssets;
using UnityEngine;
using Unity.Netcode; 

public class PlayerManager : MonoBehaviour, IEntity
{
    [Header("Vie du joueur")]
    public float maxHP = 100f;
    public float playerHP = 100f;

    [Header("Références")]
    public FPC_PLAYER playerControler;
    public WPManager weaponManager;
    public Texture crosshairTexture;
    
    // Réf. UI retirée car le WPManager gère l'UI (myHUDInstance)
    // public GameObject weaponUIObject; 

    private Vector3 positionDepart;
    private Quaternion rotationDepart;

    private bool isDead = false;
    private bool hasWon = false;

    void Start()
    {
        positionDepart = transform.position;
        rotationDepart = transform.rotation;
        playerHP = Mathf.Clamp(playerHP, 0, maxHP);
    }

    public void ResetDuJoueur()
    {
        isDead = false;
        hasWon = false;
        playerHP = maxHP;

        if (playerControler != null)
        {
            playerControler.enabled = false;
            playerControler.LockCameraPosition = false;
        }

        transform.position = positionDepart;
        transform.rotation = rotationDepart;


        if (playerControler != null) playerControler.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Réactivation de l'UI non nécessaire ici (WPManager la gère)
        // si (weaponUIObject != null) weaponUIObject.SetActive(true); 

        if (weaponManager != null)
        {
            weaponManager.EquipItemFromLibrary(0, 0); 
            weaponManager.ChangeSelectedSlot(0);
        }
    }

    public void ApplyDamage(float points)
    {
        if (isDead || hasWon) return;
        playerHP -= points;
        if (playerHP <= 0) { playerHP = 0; Die(); }
    }

    private void Die()
    {
        isDead = true;
        DisableControls();
        
        // C'est ici que la mort est signalée au WPManager pour cacher l'arme 3D
        if(weaponManager != null) weaponManager.HandleDeath(); 
        
        if(GameManager.instance != null) GameManager.instance.EnnemiGagneManche();
    }

    public void Win()
    {
        if (isDead || hasWon) return;
        hasWon = true;
        DisableControls();
        
        if (GameManager.instance != null) GameManager.instance.JoueurGagneManche();
    }

    void DisableControls()
    {
        if (playerControler != null)
        {
            playerControler.LockCameraPosition = true;
            playerControler.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, Screen.height - 35, 100, 30), ((int)playerHP).ToString() + " HP");

        if (GameManager.instance != null)
        {
            string scoreTexte = "Score: " + GameManager.instance.scoreJoueur + " / " + GameManager.instance.pointsPourGagnerMatch;
            GUI.Box(new Rect(Screen.width / 2 - 50, 10, 100, 30), scoreTexte);
        }

        if (isDead) GUI.Box(new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40), "Manche Perdue...");
        else if (hasWon) GUI.Box(new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40), "Manche Gagnée !");
        else if (crosshairTexture != null) GUI.DrawTexture(new Rect(Screen.width / 2 - 3, Screen.height / 2 - 3, 6, 6), crosshairTexture);
    }
    
    public bool IsAlive() { return !isDead; }
}