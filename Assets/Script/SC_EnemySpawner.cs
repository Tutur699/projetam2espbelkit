using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SC_EnemySpawner : NetworkBehaviour
{
    [Header("Configuration")]
    public GameObject enemyPrefab;
    public PlayerManager player;
    public Transform[] spawnPoints;
    public float startDelay = 10f; 

    [Header("État du Jeu (NetworkVariable)")]
    public NetworkVariable<float> netTimer = new NetworkVariable<float>(10f);
    public NetworkVariable<bool> netGameStarted = new NetworkVariable<bool>(false);

    bool playerWon = false;
    bool gameOver = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            netTimer.Value = startDelay;
            netGameStarted.Value = false;
        }
    }

    void Update()
    {
        // GESTION DU TIMER (Serveur)
        if (IsServer)
        {
            if (!netGameStarted.Value)
            {
                netTimer.Value -= Time.deltaTime;
                if (netTimer.Value <= 0f)
                {
                    netTimer.Value = 0f;
                    netGameStarted.Value = true;
                    SpawnEnemy();
                }
            }
        }

        if (!netGameStarted.Value) return;

        // Trouver le joueur local
        if (player == null)
        {
            var players = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
            foreach(var p in players)
            {
                if (p.GetComponent<NetworkObject>().IsOwner) 
                {
                    player = p;
                    break;
                }
            }
        }

        // Vérification Mort / Game Over
        if (player != null && !gameOver && !playerWon)
        {
            if (player.playerHP.Value <= 0) gameOver = true;
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, randomPoint.position, Quaternion.identity);

        NetworkObject netObj = enemy.GetComponent<NetworkObject>();
        if (netObj != null) netObj.Spawn();
        
        IAEnemy npc = enemy.GetComponent<IAEnemy>();
        if (npc != null)
        {
            npc.es = this; 
            PlayerManager target = FindFirstObjectByType<PlayerManager>();
            if (target != null)
            {
                npc.player = target.transform;
                npc.playerTransform = target.transform;
            }
        }
    }

    public void EnemyEliminated(IAEnemy enemy)
    {
     
        VictoryClientRpc();
        
        
        if (IsServer)
        {
            
            PlayerManager winningPlayer = FindFirstObjectByType<PlayerManager>(); 

            if (winningPlayer != null)
            {
                winningPlayer.Win(); 
            }
        }
    }

    [ClientRpc]
    void VictoryClientRpc()
    {
        playerWon = true;
    }

    public void ResetRound()
    {
        gameOver = false;
        playerWon = false;
        
        // Relance le décompte/timer pour le spawn
        if (IsServer)
        {
            netTimer.Value = startDelay; 
            netGameStarted.Value = false; 
        }

        // Détruire l'ennemi existant
        IAEnemy[] enemies = FindObjectsByType<IAEnemy>(FindObjectsSortMode.None);
        foreach (IAEnemy enemy in enemies)
        {
            if (enemy != null)
            {
                if(IsServer && enemy.GetComponent<NetworkObject>() != null) 
                    enemy.GetComponent<NetworkObject>().Despawn(); 
                else 
                    Destroy(enemy.gameObject);
            }
        }
    }
}