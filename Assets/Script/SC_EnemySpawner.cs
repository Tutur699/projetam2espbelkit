using UnityEngine;
using Unity.Netcode;
using TMPro;
public class SC_EnemySpawner : NetworkBehaviour
{
    [Header("Configuration")]
    public GameObject enemyPrefab;
    public PlayerManager player;
    public Transform[] spawnPoints;
    public float startDelay = 10f; // 10 secondes avant le début de la partie

    [Header("État du Jeu (NetworkVariable)")]
    public NetworkVariable<float> netTimer = new NetworkVariable<float>(10f);
    public NetworkVariable<bool> netGameStarted = new NetworkVariable<bool>(false);

    bool enemyAlive = false;
    bool playerWon = false;
    bool gameStarted = false;
    bool gameOver = false;

    float startTimer;

    public override void OnNetworkSpawn()
    {
        // On initialise le timer seulement si on est le Serveur (l'Hôte)
        if (IsServer)
        {
            netTimer.Value = startDelay;
            netGameStarted.Value = false;
        }

        // On cherche notre propre PlayerManager local
        // (Astuce : WPManager est sur le joueur, donc on peut chercher WPManager si IsOwner)
        // Ou plus simple : on attendra l'Update pour le trouver
    }

    void Update()
    {
        // 1. GESTION DU TIMER (Serveur Seulement)
        if (IsServer)
        {
            if (!netGameStarted.Value)
            {
                netTimer.Value -= Time.deltaTime;

                if (netTimer.Value <= 0f)
                {
                    netTimer.Value = 0f;
                    netGameStarted.Value = true; // Le jeu commence pour tout le monde !
                    SpawnEnemy(); // Le serveur fait apparaître l'ennemi
                }
            }
        }

        // 2. LOGIQUE CLIENT (Tout le monde exécute ça)
        
        // Si le jeu n'a pas commencé, on ne fait rien (on affiche juste le GUI)
        if (!netGameStarted.Value) return;

        // On essaie de trouver le joueur local si on ne l'a pas encore
        if (player == null)
        {
            var players = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
            foreach(var p in players)
            {
                // On cherche celui qui appartient au client local
                if (p.GetComponent<NetworkObject>().IsOwner) 
                {
                    player = p;
                    break;
                }
            }
        }

        // Vérification Mort / Victoire (Localement)
        if (player != null && !gameOver && !playerWon)
        {
            if (player.playerHP <= 0)
            {
                gameOver = true;
                // On pourrait envoyer un ServerRpc pour dire "Je suis mort"
            }
        }
        
        // Input Quitter
        if ((playerWon || gameOver) && Input.GetKeyDown(KeyCode.Space))
        {
             // Logique de fin de partie...
             // En multi, on se déconnecte plutôt que de quitter l'app
             NetworkManager.Singleton.Shutdown();
             #if UNITY_EDITOR
             UnityEditor.EditorApplication.isPlaying = false;
             #else
             Application.Quit();
             #endif
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Instantiation standard d'Unity
        GameObject enemy = Instantiate(enemyPrefab, randomPoint.position, Quaternion.identity);

        // --- MAGIE NETCODE ---
        // Il faut dire au réseau que cet objet existe pour que les clients le voient
        NetworkObject netObj = enemy.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(); // L'ennemi apparaît sur tous les écrans !
        }
        
        // Configurer le script de l'ennemi (s'il faut)
        IAEnemy npc = enemy.GetComponent<IAEnemy>();
        if (npc != null)
        {
            npc.es = this; 
            PlayerManager target = FindFirstObjectByType<PlayerManager>();
            if (target != null)
            {
                npc.player = target.transform; // On donne la cible !
                npc.playerTransform = target.transform;
            }
        }
    }

    void OnGUI()
    {
        // On n'affiche rien si on n'est pas connecté
        if (!IsSpawned) return;

        // Affichage Timer (synchronisé via netTimer.Value)
        if (!netGameStarted.Value)
        {
            GUI.Box(
                new Rect(Screen.width / 2 - 125, Screen.height / 4 - 12, 250, 25),
                "Game starts in " + Mathf.Ceil(netTimer.Value).ToString() + "..."
            );
            return;
        }

        // Affichage Game Over / Win
        if (gameOver)
        {
            GUI.Box(new Rect(Screen.width/2 - 85, Screen.height/2 - 20, 170, 40), "YOU DIED");
        }
        if (playerWon)
        {
            GUI.Box(new Rect(Screen.width/2 - 85, Screen.height/2 - 20, 170, 40), "VICTORY !");
        }
    }

    public void EnemyEliminated(IAEnemy enemy)
    {
        // L'ennemi doit appeler ça, et comme il est géré par le serveur, c'est bon.
        // Il faudrait idéalement une ClientRpc pour dire à tout le monde "Victoire !"
        VictoryClientRpc();
    }
    [ClientRpc]
    void VictoryClientRpc()
    {
        playerWon = true;
    }

}
