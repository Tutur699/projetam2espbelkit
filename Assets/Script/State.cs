using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
public class State
{
    public enum STATE
    {
        IDLE,PATROL, PURSUE, ATTACK, SLEEP //describe the state
    };
    public enum EVENT
    {
        ENTER, UPDATE, EXIT //describe the state event
    };

    public STATE name; //name of the state based on the enum
    protected EVENT stage; //current event of the state
    protected GameObject npc; //the game object the state is associated with
    protected Animator anim; //the animator for the npc
    protected State nextState; //the next state the npc will be in
    protected NavMeshAgent agent; //the nav mesh agent for the npc

    protected WPManager pm; //the player manager
    protected IAEnemy enemyScript;
    protected Transform player; //the player transform

    public State(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        player = _player;
        stage = EVENT.ENTER; //set the initial stage to enter

        pm = npc.GetComponent<WPManager>(); //get the player manager component
        enemyScript = npc.GetComponent<IAEnemy>();

    }

    public virtual void Enter() { stage = EVENT.UPDATE; } //when the state is entered, set the stage to update
    public virtual void Update() { stage = EVENT.UPDATE; } //when the state is updated, keep the stage as update
    public virtual void Exit() { stage = EVENT.EXIT; } //when the state is exited, set the stage to exit

    public State Process() //process the state
    {
        if (stage == EVENT.ENTER) Enter(); //if the stage is enter, call the enter method
        if (stage == EVENT.UPDATE) Update(); //if the stage is update, call the update method
        if (stage == EVENT.EXIT) //if the stage is exit, call the exit method and return the next state
        {
            Exit();
            return nextState;
        }
        return this; //return the current state
    }

  public bool CanSeePlayer() 
    {
        if (player == null) return false;

        Vector3 direction = player.position - npc.transform.position;
        if (direction.magnitude < enemyScript.viewRadius)
        {
            float angle = Vector3.Angle(direction, npc.transform.forward);
            if (angle < enemyScript.viewAngle / 2) 
            {
                RaycastHit hit;
                
                // --- CORRECTION HAUTEUR ICI AUSSI ---
                // On part de (Position + 1.6m) pour partir des YEUX
                Vector3 origin = npc.transform.position + Vector3.up * 1.6f;
                
                // On vise le TORSE du joueur (Player + 1.3m) pour ne pas viser ses pieds
                Vector3 target = player.position + Vector3.up * 1.3f;
                Vector3 dirToTarget = (target - origin).normalized;
                // ------------------------------------

                if (Physics.Raycast(origin, dirToTarget, out hit, enemyScript.viewRadius))
                {
                    if (hit.transform.CompareTag("Player") || hit.transform.GetComponent<PlayerManager>())
                    {
                        return true;
                    }
                }
            }
        }
        return false; 
    }

    public bool CanAttackPlayer() 
    {
        if (player == null) return false;

        Vector3 direction = player.position - npc.transform.position;
        
        // --- CHANGEMENT ICI ---
        // Au lieu d'utiliser la variable fixe 'shootDistance',
        // on appelle la fonction dynamique de l'IA.
        float dynamicRange = enemyScript.GetCurrentWeaponRange();

        if (direction.magnitude < dynamicRange) 
        {
            return true;
        }
        return false; 
    }
        
        
}

public class IdleState : State
{  

    public IdleState(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.IDLE; //set the name of the state to idle
    }

    public override void Enter()
    {
        //anim.SetTrigger("isIdle"); //set the animator to idle
        base.Enter();
    }

    public override void Update()
    {
        if(CanSeePlayer()) //if the npc can see the player
        {
            nextState = new Pursue(npc, agent, anim, player); //switch to pursue state
            stage = EVENT.EXIT; //set the stage to exit
        }
        else 
        {
            nextState = new Patrol(npc, agent, anim, player); //stay in idle state
            stage = EVENT.EXIT; //set the stage to exit
        }
    }

    public override void Exit()
    {
        //anim.ResetTrigger("isIdle"); //reset the idle trigger on the animator
        base.Exit();
    }
}

public class Patrol : State
{
    int currentIndex = 0;
    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PATROL; //set the name of the state to patrol
        agent.speed = 2f; //set the speed of the agent to 2 if the agent has a path to follow
        agent.isStopped = false; //set the agent to not be stopped
    }

    public override void Enter()
    {
        if (enemyScript.waypoints.Count == 0) return;
        float lastDist = Mathf.Infinity;
        for (int i = 0; i < enemyScript.waypoints.Count; i++)
        {
            float dist = Vector3.Distance(npc.transform.position, enemyScript.waypoints[i].position);
            if (dist < lastDist)
            {
                lastDist = dist;
                currentIndex = i;
            }
        }
        agent.SetDestination(enemyScript.waypoints[currentIndex].position);
        //anim.SetTrigger("isWalking"); //set the animator to walking
        base.Enter();
    }

    public override void Update()
    {
        // --- PRIORITÉ ABSOLUE : LA VISION ---
        if (CanSeePlayer())
        {
            // Raccourci intelligent :
            // Si on est DÉJÀ à portée de tir, on attaque direct !
            // Sinon, on poursuit.
            if (CanAttackPlayer())
            {
                nextState = new Attack(npc, agent, anim, player);
            }
            else
            {
                nextState = new Pursue(npc, agent, anim, player);
            }
            
            stage = EVENT.EXIT;
            return; // On arrête tout le reste, on change d'état immédiatement
        }
        // ------------------------------------

        // La suite (le déplacement vers les Waypoints) ne s'exécute que si on ne voit PERSONNE
        if (enemyScript.waypoints.Count == 0) return;

        if (agent.remainingDistance < 1.0f && !agent.pathPending)
        {
            currentIndex++;
            if (currentIndex >= enemyScript.waypoints.Count) currentIndex = 0;
            agent.SetDestination(enemyScript.waypoints[currentIndex].position);
        }
    }
    

    public override void Exit()
    {
       // anim.ResetTrigger("isWalking"); //reset the walking trigger on the animator
        base.Exit();
    }
}

public class Pursue : State
{
    public Pursue(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PURSUE; //set the name of the state to pursue
        agent.speed = 5; //set the speed of the agent to 4 if the agent has a path to follow
        agent.isStopped = false; //set the agent to not be stopped
    }

    public override void Enter()
    {
        //anim.SetTrigger("isRunning"); //set the animator to running
        base.Enter();
    }

    public override void Update()
    {
        // 1. Est-ce qu'on peut DÉJÀ tirer ? (Priorité)
        if (CanAttackPlayer())
        {
            nextState = new Attack(npc, agent, anim, player);
            stage = EVENT.EXIT;
            return;
        }

        // 2. Est-ce qu'on a perdu le joueur de vue ?
        if (!CanSeePlayer())
        {
            nextState = new Patrol(npc, agent, anim, player); // Ou Idle
            stage = EVENT.EXIT;
            return;
        }

        // 3. Sinon, on court vers lui
        agent.SetDestination(player.position);
    }

    public override void Exit()
    {
       // anim.ResetTrigger("isRunning"); //reset the running trigger on the animator
        base.Exit();
    }
}

public class Attack : State
{
    float currentReactionTimer = 0.0f;
    bool hasReacted = false;

    float currentBurstTimer = 0.0f;
    bool isBursting = true; // Est-ce qu'on est en train de tirer ou en pause ?
    float shotTimer = 0.0f;

    Vector3 currentAimPoint;

    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) 
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.ATTACK;
    }

    public override void Enter()
    {
       // anim.SetTrigger("isShooting"); // Animation de tir
        agent.isStopped = true;       // On s'arrête

        currentReactionTimer = 0.0f;
        hasReacted = false;
        isBursting = true;
        currentBurstTimer = enemyScript.burstTime;
        shotTimer = 0.0f;

        // Au début, l'IA vise là où elle regardait déjà (elle ne snap pas sur le joueur)
        currentAimPoint = npc.transform.position + npc.transform.forward * 5f;
        
        // Sécurité : Si l'arme n'est pas sortie, on la sort maintenant
        if (pm != null && pm.selectedItems == null)
        {
            pm.EquipItemFromLibrary(0);
            pm.ChangeSelectedSlot(0);
        }

        base.Enter();
    }

    public override void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(npc.transform.position, player.position);
        enemyScript.ManageWeaponChoice(dist);

        // 1. TEMPS DE RÉACTION
        if (!hasReacted)
        {
            currentReactionTimer += Time.deltaTime;
            SmoothLookAtPlayer();

            if (currentReactionTimer >= enemyScript.reactionTime)
            {
                hasReacted = true;
                // On s'assure que les timers sont prêts pour le tir
                isBursting = true;
                currentBurstTimer = enemyScript.burstTime;
            }
            return;
        }

        // 2. VISÉE ORGANIQUE
        Vector3 idealTarget = player.position + Vector3.up * 1.3f;
        
        // Tremblement
        float noiseX = (Mathf.PerlinNoise(Time.time * 2, 0) - 0.5f) * enemyScript.accuracyError;
        float noiseY = (Mathf.PerlinNoise(0, Time.time * 2) - 0.5f) * enemyScript.accuracyError;
        Vector3 noisyTarget = idealTarget + new Vector3(noiseX, noiseY, 0);

        // Tracking
        currentAimPoint = Vector3.Lerp(currentAimPoint, noisyTarget, Time.deltaTime * enemyScript.trackingSpeed);

        // Rotation du corps
        Vector3 direction = currentAimPoint - npc.transform.position;
        direction.y = 0;
        npc.transform.rotation = Quaternion.LookRotation(direction);

        // 3. GESTION DU TIR (Rafales + Cadence)
        
        // On met à jour les timers
        currentBurstTimer -= Time.deltaTime;
        shotTimer -= Time.deltaTime; // Le timer de l'arme descend tout le temps

        if (isBursting)
        {
            // On est en phase "Gâchette appuyée"
            
            // --- CORRECTION : ON VÉRIFIE LA CADENCE DE L'ARME ---
            if (shotTimer <= 0f)
            {
                if (pm != null && pm.selectedItems != null)
                {
                     pm.selectedItems.Use(); // PAN !
                     
                     // On remet le timer à la cadence de l'arme (ex: 0.1s)
                     if (pm.selectedItems.item != null)
                        shotTimer = pm.selectedItems.item.useRate;
                     else
                        shotTimer = 0.1f; // Sécurité par défaut
                }
            }
            // ----------------------------------------------------

            // Fin de la rafale ?
            if (currentBurstTimer <= 0)
            {
                isBursting = false;
                currentBurstTimer = enemyScript.burstPause; // On passe en pause
            }
        }
        else
        {
            // On est en phase de pause
            if (currentBurstTimer <= 0)
            {
                isBursting = true;
                currentBurstTimer = enemyScript.burstTime; // On reprend le tir
                shotTimer = 0.0f; // On peut tirer tout de suite au début de la nouvelle rafale
            }
        }

        // 4. SORTIE
        if (!CanAttackPlayer() || !CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    void SmoothLookAtPlayer()
    {
        Vector3 direction = player.position - npc.transform.position;
        direction.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRot, Time.deltaTime * 2.0f);
    }

    public override void Exit()
    {
        //anim.ResetTrigger("isShooting");
        agent.isStopped = false; // On autorise le mouvement à nouveau
        base.Exit();
    }
}