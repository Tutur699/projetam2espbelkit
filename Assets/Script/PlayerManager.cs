using StarterAssets;
using UnityEngine;
using Unity.Netcode; 

public class PlayerManager : NetworkBehaviour, IEntity
{
    [Header("Vie du joueur")]
    public float maxHP = 100f;
    public NetworkVariable<float> playerHP = new NetworkVariable<float>(
        100f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    [Header("Références")]
    public FPC_PLAYER playerControler;
    public WPManager weaponManager;
    public Texture crosshairTexture;
     

    private Vector3 positionDepart;
    private Quaternion rotationDepart;

    private bool isDead = false;
    private bool hasWon = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerHP.Value = maxHP;
        }
        positionDepart = transform.position;
        rotationDepart = transform.rotation;

        if (IsOwner)
        {
            if (weaponManager != null)
        {
            weaponManager.EquipItemFromLibrary(0, 0); 
            weaponManager.ChangeSelectedSlot(0);
        }
        }
    }


    public void ResetDuJoueur()
    {
        if(IsServer)ResetLogicServer();
        else ResetDuJoueurServerRpc();
    }

    [ServerRpc]
    private void ResetDuJoueurServerRpc()
    {
        ResetLogicServer();
    }
    private void ResetLogicServer()
    {
        playerHP.Value = maxHP;
        ResetDuJoueurClientRpc();
    } 

    [ClientRpc]
    void ResetDuJoueurClientRpc()
    {
        isDead = false;
        hasWon = false;
        if(IsOwner)
        {
        

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

        
        }
    }

    public void ApplyDamage(float points)
    {
        if(IsClient) ApplyDamageServerRpc(points);
        
    }

    [ServerRpc]
    void ApplyDamageServerRpc(float points)
    {
        if (isDead || hasWon) return;
        playerHP.Value -= points;
        if (playerHP.Value <= 0) { playerHP.Value = 0; DieClientRpc(); }
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        isDead = true;
        DisableControls();
        
        // C'est ici que la mort est signalée au WPManager pour cacher l'arme 3D
        if(weaponManager != null) weaponManager.HandleDeath(); 
        
        if(GameManager.instance != null) GameManager.instance.EnnemiGagneManche();
    }

    public void Win()
    {
        if(IsServer) WinClientRpc();
        else if(IsClient) WinServerRpc();
    }
    [ServerRpc]
    private void WinServerRpc()
    {
        WinClientRpc();
    }
    
    
    [ClientRpc]
    private void WinClientRpc()
    {
        if (isDead || hasWon) return;
        hasWon = true;
        DisableControls();
        
        if (GameManager.instance != null) GameManager.instance.JoueurGagneManche();
    }

    void DisableControls()
    {
        if(!IsOwner) return;
        if (playerControler != null)
        {
            playerControler.LockCameraPosition = true;
            playerControler.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Dans PlayerManager.cs

void OnGUI()
    {
        if(!IsOwner) return; // S'assure que seul le propriétaire affiche l'UI
        GUI.Box(new Rect(10, Screen.height - 35, 100, 30), playerHP.Value.ToString() + " HP");

        if (GameManager.instance != null)
        {
        
            int matchPoints = GameManager.instance.pointsPourGagnerMatch;
            int playerScore = GameManager.instance.scoreJoueur;
            int enemyScore = GameManager.instance.scoreEnnemi;

            float boxWidth = 120;
            float spacing = 10;
            float totalWidth = (2 * boxWidth) + spacing;
            float startX = (Screen.width / 2) - (totalWidth / 2);

            string playerScoreTexte = $"Your Score: {playerScore} / {matchPoints}";
            GUI.Box(new Rect(startX, 10, boxWidth, 30), playerScoreTexte);
        
            string enemyScoreTexte = $"Enemy Score: {enemyScore} / {matchPoints}";
            GUI.Box(new Rect(startX + boxWidth + spacing, 10, boxWidth, 30), enemyScoreTexte);

        
        }


        if (isDead) 
        {
            GUI.Box(new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40), "Manche Perdue...");
        }
        else if (hasWon) 
        {
            GUI.Box(new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40), "Manche Gagnée !");
        }
        else if (crosshairTexture != null) 
        {
            GUI.DrawTexture(new Rect(Screen.width / 2 - 3, Screen.height / 2 - 3, 6, 6), crosshairTexture);
        }
    }
    
    public bool IsAlive() { return !isDead; }
}