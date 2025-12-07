using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class IAEnemy : MonoBehaviour, IEntity
{
    [Header("IA Enemy Stats")]
    public float npcHP = 100;

    [Header("IA Enemy Settings")]
    NavMeshAgent agent; //the nav mesh agent for the npc
    Animator anim; //the animator for the npc
    public Transform player; //the player transform
    public GameObject npcDeadPrefab;
    public WPManager em;

    private bool isDead = false;
    State currentState; //the current state the npc is in

    [HideInInspector] public SC_EnemySpawner es;
    [HideInInspector] public Transform playerTransform;

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
                // On active la première arme (Pistolet ou Fusil)
                em.EquipItemFromLibrary(0); 
                em.ChangeSelectedSlot(0);
            }
        }

        currentState = new IdleState(this.gameObject, agent, anim, player); //set the initial state to idle
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


