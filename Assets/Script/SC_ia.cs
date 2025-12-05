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

    Animator animator;

    void Start()
    {   
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackDistance;
        agent.speed = movementSpeed;
        agent.updateRotation = false;

        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

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

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("[SC_NPCEnemy] Aucun Animator trouvé sur l'ennemi ou ses enfants.");
        }
        else
        {
            // Avec un NavMeshAgent on laisse souvent le root motion désactivé
            animator.applyRootMotion = false;
            Debug.Log("[SC_NPCEnemy] Animator trouvé sur : " + animator.gameObject.name);
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // ATTACK
        if (agent.remainingDistance - attackDistance < 0.01f)
        {
            if (Time.time > nextAttackTime)
            {
                nextAttackTime = Time.time + attackRate;

                RaycastHit hit;
                if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, attackDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        Debug.DrawLine(
                            firePoint.position,
                            firePoint.position + firePoint.forward * attackDistance,
                            Color.cyan
                        );

                        IEntity player = hit.transform.GetComponent<IEntity>();
                        player.ApplyDamage(npcDamage);
                    }
                }
            }
        }

        // MOVE
        agent.destination = playerTransform.position;

        transform.LookAt(new Vector3(
            playerTransform.position.x,
            transform.position.y,
            playerTransform.position.z));

        // ANIM
        if (animator != null)
        {
            float rawSpeed = agent.velocity.magnitude;
            float speed = rawSpeed / movementSpeed;
            speed = Mathf.Clamp01(speed);

            animator.SetFloat("Speed", speed);
        }
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
