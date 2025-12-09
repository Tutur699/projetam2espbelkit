using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
[RequireComponent(typeof(NavMeshAgent))]
public class IAEnemy : MonoBehaviour, IEntity
{
    public enum Difficulty { BEGINNER, AVERAGE, PRO, BOT, HUMAN_LIKE }
    [Header("Configuration IA")]
    public Difficulty difficulty = Difficulty.HUMAN_LIKE;
    [HideInInspector] public float reactionTime;    // Temps avant de commencer à tirer (s)
    [HideInInspector] public float accuracyError;   // Rayon d'imprécision (mètres)
    [HideInInspector] public float trackingSpeed;   // Vitesse à laquelle il ajuste sa visée (Lerp)
    [HideInInspector] public float burstTime;       // Temps de tir continu avant pause
    [HideInInspector] public float burstPause;      // Temps de pause entre rafales

    [Header("Arsenal & Tactique")]
    public List<int> startingWeaponIndices = new List<int> { 0, 1, 2 };
    // Distance à partir de laquelle l'IA sort l'arme principale (Slot 1)
    public float switchWeaponDistance = 10.0f; 

    [Header("Portée des Armes")]
    public float shortRange = 15f; // Portée pour Slot 0 (Pistolet/Pompe)
    public float longRange = 30f;  // Portée pour Slot 1 (Fusil)
    public float verylongRange = 40f; //Portée pour Slot 2 (Sniper)

    // Timer pour éviter que l'IA change d'arme 10 fois par seconde (clignotement)
    [HideInInspector] public float weaponSwitchCooldown = 0f;

    [Header("Perception")]
    [Range(0, 360)] public float viewAngle = 60f; // Angle de vision (cône)
    public float viewRadius = 15f;


    [Header("IA Enemy Stats")]
    public float npcHP = 100;

    [Header("IA Enemy Settings")]
    NavMeshAgent agent; //the nav mesh agent for the npc
    Animator anim; //the animator for the npc
    public Transform player; //the player transform
    public GameObject npcDeadPrefab;
    public WPManager em;

    [Header("Navigation")]
    // On cache la liste dans l'inspector car elle se remplit toute seule
    [HideInInspector] public List<Transform> waypoints = new List<Transform>();
    [HideInInspector] public SC_EnemySpawner es;
    [HideInInspector] public Transform playerTransform;

    private bool isDead = false;
    State currentState; //the current state the npc is in

    

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>(); //get the nav mesh agent component
        anim = this.GetComponent<Animator>(); //get the animator component
       if (player == null)
        {
            PlayerManager target = FindFirstObjectByType<PlayerManager>();
            if (target != null) player = target.transform;
        }

        // --- CORRECTION : On équipe l'arme tout de suite ---
        if (em != null) // 'em' c'est ton WPManager sur l'IA
        {
            // On s'assure que la liste d'armes est prête
            if (em.weaponLibrary.Count > 0)
            {
                for (int i = 0; i < startingWeaponIndices.Count; i++)
                {
                // On prend l'arme X de la librairie et on la met dans le slot i
                // (Index Librairie, Index Slot)
                em.EquipItemFromLibrary(startingWeaponIndices[i], i);
                }
            em.ChangeSelectedSlot(0);
            }
        }

        SetupDifficulty();

        GameObject[] pointsTrouves = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach (GameObject obj in pointsTrouves)
        {
            waypoints.Add(obj.transform);
        }
        waypoints = waypoints.OrderBy(w => w.name).ToList();
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("IA : Aucun objet avec le tag 'Waypoint' trouvé sur la scène !");
        }
        currentState = new IdleState(this.gameObject, agent, anim, player); //set the initial state to idle
    }

   private void OnDrawGizmos()
    {
        // --- CORRECTION HAUTEUR ---
        // On crée un point fictif à 1.6 mètre du sol (hauteur des yeux standard)
        Vector3 eyePos = transform.position + Vector3.up * 1.6f;

        // Si tu as bien assigné le WPManager et son AimPoint (EyesPos), on l'utilise pour être précis
        if (em != null && em.aimPoint != null)
        {
            eyePos = em.aimPoint.position;
        }
        // --------------------------

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePos, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePos, eyePos + viewAngleA * viewRadius);
        Gizmos.DrawLine(eyePos, eyePos + viewAngleB * viewRadius);
    }

    // Petite fonction utilitaire pour calculer les angles
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public float GetCurrentWeaponRange()
    {
        // Sécurité : si pas de manager, on retourne une valeur par défaut
        if (em == null) return shortRange;

        // Si on tient l'arme secondaire (Slot 0)
        if (em.selectedSlot == 0)
        {
            return shortRange;
        }
        if(em.selectedSlot == 2)
        {
            return verylongRange;
        }
        // Si on tient l'arme principale (Slot 1)
        else
        {
            return longRange;
        }
    }

    public void ManageWeaponChoice(float distanceToPlayer)
    {
        if (em == null) return;
        
        // On réduit le cooldown
        if (weaponSwitchCooldown > 0)
        {
            weaponSwitchCooldown -= Time.deltaTime;
            return;
        }

        int bestSlot = 0; // Par défaut, arme secondaire (Slot 0)

        // LOGIQUE SIMPLE :
        // Si la cible est loin (> 10m) ET qu'on a une arme principale (Slot 1)
        // Alors on prend l'arme principale.
        if (distanceToPlayer > switchWeaponDistance + 30.0f && em.allItems.Count > 1 && em.allItems[2] != null)
        {
            bestSlot = 2;
        }
        else if (distanceToPlayer > switchWeaponDistance && em.allItems.Count > 1 && em.allItems[1] != null)
        {
            bestSlot = 1;
        }
        else
        {
            bestSlot = 0; // Sinon (proche), on prend l'arme secondaire/corps à corps
        }

        // Si on n'a pas la bonne arme en main, on change !
        if (em.selectedSlot != bestSlot)
        {
            em.ChangeSelectedSlot(bestSlot);
            weaponSwitchCooldown = 1.0f; // On attend 2s avant de pouvoir rechanger (latence humaine)
        }
    }

    void SetupDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.BEGINNER: // "Le petit frère"
                reactionTime = 1.5f;   // Lent à la détente
                accuracyError = 3.0f;  // Tire partout sauf sur toi
                trackingSpeed = 2.0f;  // Tourne lentement
                burstTime = 0.5f;      // Tire peu
                burstPause = 2.0f;     // Attend longtemps
                break;

            case Difficulty.AVERAGE: // "Joueur occasionnel"
                reactionTime = 0.5f;
                accuracyError = 1.5f;
                trackingSpeed = 5.0f;
                burstTime = 1.0f;
                burstPause = 1.0f;
                break;

            case Difficulty.PRO: // "Tryhardeur"
                reactionTime = 0.2f;
                accuracyError = 0.5f;
                trackingSpeed = 10.0f;
                burstTime = 2.0f;
                burstPause = 0.2f;
                break;
            
            case Difficulty.BOT: // "Aimbot"
                reactionTime = 0.0f;
                accuracyError = 0.0f;
                trackingSpeed = 50.0f; // Instantané
                burstTime = 10.0f;
                burstPause = 0.0f;
                break;

            case Difficulty.HUMAN_LIKE: // "Test de Turing"
                // Un bon joueur humain réagit vite (0.2s - 0.3s)
                reactionTime = Random.Range(0.2f, 0.4f); 
                // Il vise globalement bien, mais avec une petite marge d'erreur naturelle
                accuracyError = 0.8f; 
                // Il suit la cible fluidement, pas robotiquement
                trackingSpeed = 6.0f; 
                // Il gère ses munitions (rafales contrôlées)
                burstTime = Random.Range(0.5f, 1.5f); 
                burstPause = Random.Range(0.1f, 0.4f); 
                break;
        }
    }
    void Update()
    {
        currentState = currentState.Process(); //process the current state
        
    }

    public bool isAlive()
    {
        return !isDead;
    }

   public void ApplyDamage(float points)
    {
        Debug.Log($"[IA] AIE ! J'ai reçu {points} dégâts. Ma vie avant : {npcHP}");
        //if (isDead) return;
        npcHP -= points;
        Debug.Log($"[IA] Ma vie après : {npcHP}");
        if (npcHP <= 0)
        {
            Debug.Log($"[IA] Je suis mort");
            DeadNPC();
        }
    }

    private void DeadNPC()
    {
        Debug.Log($"On m'appelle");
        isDead = true;
        GameObject npcDead = Instantiate(npcDeadPrefab, transform.position, transform.rotation);
            npcDead.GetComponent<Rigidbody>().linearVelocity =
                (-(playerTransform.position - transform.position).normalized * 8) + new Vector3(0, 5, 0);
            Destroy(npcDead, 10);
            es.EnemyEliminated(this);
            Destroy(gameObject);
    }

}


