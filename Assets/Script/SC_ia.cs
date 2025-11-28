using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SC_NPCEnemy : MonoBehaviour, IEntity
{
    public float attackDistance = 3f;
    public float movementSpeed = 4f;
    public float npcHP = 100;
    public float npcDamage = 5;
    public float attackRate = 0.5f;
    public Transform firePoint;
    public GameObject npcDeadPrefab;

    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public SC_EnemySpawner es;

    NavMeshAgent agent;
    float nextAttackTime = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackDistance;
        agent.speed = movementSpeed;

        //Set Rigidbody to Kinematic to prevent hit register bug
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        // 👇 AJOUT : si le spawner n’a pas déjà mis playerTransform,
        // on cherche l’instance réelle du joueur dans la scène
        if (playerTransform == null)
        {
            PlayerManager pm = FindFirstObjectByType<PlayerManager>();
            if (pm != null)
            {
                playerTransform = pm.transform;
            }
            else
            {
                Debug.LogWarning("[SC_NPCEnemy] Aucun PlayerManager trouvé dans la scène.");
            }
        }
    }

    void Update()
    {
        // 👇 AJOUT : sécurité, si on n’a pas encore trouvé le joueur → on attend
        if (playerTransform == null)
            return;

        if (agent.remainingDistance - attackDistance < 0.01f)
        {
            if (Time.time > nextAttackTime)
            {
                nextAttackTime = Time.time + attackRate;

                //Attack
                RaycastHit hit;
                if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, attackDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        Debug.DrawLine(firePoint.position,
                                       firePoint.position + firePoint.forward * attackDistance,
                                       Color.cyan);

                        IEntity player = hit.transform.GetComponent<IEntity>();
                        player.ApplyDamage(npcDamage);
                    }
                }
            }
        }

        //Move towards the player
        agent.destination = playerTransform.position;

        //Always look at player
        transform.LookAt(new Vector3(
            playerTransform.position.x,
            transform.position.y,
            playerTransform.position.z));
    }

    public void ApplyDamage(float points)
    {
        npcHP -= points;
        if (npcHP <= 0)
        {
            GameObject npcDead = Instantiate(npcDeadPrefab, transform.position, transform.rotation);
            npcDead.GetComponent<Rigidbody>().linearVelocity =
                (-(playerTransform.position - transform.position).normalized * 8) +
                new Vector3(0, 5, 0);

            Destroy(npcDead, 10);
            es.EnemyEliminated(this);
            Destroy(gameObject);
        }
    }
}
