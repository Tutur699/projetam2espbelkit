using UnityEngine;

public class SC_EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public PlayerManager player;
    public Transform[] spawnPoints;

    public float startDelay = 10f; // 10 secondes avant le début de la partie

    bool enemyAlive = false;
    bool playerWon = false;
    bool gameStarted = false;
    bool gameOver = false;

    float startTimer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startTimer = startDelay;
    }

    void Update()
    {
        // ⏳ Tant que le jeu n'a pas commencé, on fait le compte à rebours
        if (!gameStarted)
        {
            startTimer -= Time.deltaTime;

            if (startTimer <= 0f)
            {
                gameStarted = true;
                SpawnEnemy();
            }

            return; // On ne fait rien d'autre tant que le jeu n'a pas commencé
        }

        // 💀 Gestion de la mort du joueur
        if (!gameOver && !playerWon && player.playerHP <= 0)
        {
            gameOver = true;
        }

        // 🏁 Si la partie est finie (victoire ou défaite) et qu'on appuie sur Espace → quitter
        if ((playerWon || gameOver) && Input.GetKeyDown(KeyCode.Space))
        {
            #if UNITY_EDITOR
            // Arrête le mode Play dans l'éditeur
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // Quitte le jeu dans un build
            Application.Quit();
            #endif
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, randomPoint.position, Quaternion.identity);

        SC_NPCEnemy npc = enemy.GetComponent<SC_NPCEnemy>();
        if (npc != null)
        {
            // Pour que l'ennemi puisse appeler EnemyEliminated quand il meurt
            npc.es = this;
        }

        enemyAlive = true;
    }

    void OnGUI()
    {
        // ⏳ Affichage du compte à rebours avant le début
        if (!gameStarted)
        {
            GUI.Box(
                new Rect(Screen.width / 2 - 125, Screen.height / 4 - 12, 250, 25),
                "Game starts in " + Mathf.Ceil(startTimer).ToString() + "..."
            );
            return;
        }

        if (gameOver)
        {
            GUI.Box(
                new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40),
                "Game Over"
            );
            GUI.Label(
                new Rect(Screen.width / 2 - 70, Screen.height / 2 + 30, 200, 40),
                "Press SPACE to quit"
            );
        }

        if (playerWon)
        {
            GUI.Box(
                new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40),
                "YOU WIN !"
            );
            GUI.Label(
                new Rect(Screen.width / 2 - 80, Screen.height / 2 + 30, 200, 40),
                "Press SPACE to quit"
            );
        }
    }

    public void EnemyEliminated(SC_NPCEnemy enemy)
    {
        enemyAlive = false;
        playerWon = true;
    }
}
