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
    protected Transform player; //the player transform

    float vistDistance = 10.0f; //the distance the npc can see
    float visAngle = 30.0f; //the angle the npc can see
    float shootDistance = 7.0f; //the distance the npc can attack from

    public State(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        player = _player;
        stage = EVENT.ENTER; //set the initial stage to enter

        pm = npc.GetComponent<WPManager>(); //get the player manager component

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

    public bool CanSeePlayer() //check if the npc can see the player
    {
        Vector3 direction = player.position - npc.transform.position; //get the direction to the player
        float angle = Vector3.Angle(direction, npc.transform.forward); //get the angle to the player
        if (direction.magnitude < vistDistance && angle < visAngle) //if the player is within the vist distance and angle
        {
            return true; //return true
        }
        return false; //return false if the player is not seen
    }

    public bool CanAttackPlayer() //check if the player is in attack range
    {
        Vector3 direction = player.position - npc.transform.position; //get the direction to the player
        if (direction.magnitude < shootDistance) //if the player is within the shoot distance
        {
            return true; //return true
        }
        return false; //return false if the player is not in attack range
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
    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PATROL; //set the name of the state to patrol
        agent.speed = 10f; //set the speed of the agent to 2 if the agent has a path to follow
        agent.isStopped = false; //set the agent to not be stopped
    }

    public override void Enter()
    {
        //set destination to the player's position
        Vector3 patrolPoint = npc.transform.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        agent.SetDestination(patrolPoint);
        //anim.SetTrigger("isWalking"); //set the animator to walking
        base.Enter();
    }

    public override void Update()
    {
        if (agent.remainingDistance < 1) //if the agent is close to the destination or has no path
        {
            if (CanSeePlayer()) //if the npc can see the player
            {
                nextState = new Pursue(npc, agent, anim, player); //switch to pursue state
                stage = EVENT.EXIT; //set the stage to exit
            }
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
        agent.speed = 2; //set the speed of the agent to 4 if the agent has a path to follow
        agent.isStopped = false; //set the agent to not be stopped
    }

    public override void Enter()
    {
        //anim.SetTrigger("isRunning"); //set the animator to running
        base.Enter();
    }

    public override void Update()
    {
        agent.SetDestination(player.position); //set the agent's destination to the player's position
        if (agent.hasPath)
        {
            if (CanAttackPlayer()) //if the npc can attack the player
            {
                nextState = new Attack(npc, agent, anim, player); //switch to attack state
                stage = EVENT.EXIT; //set the stage to exit
            }
            else if (!CanSeePlayer()) //if the npc cannot see the player
            {
                nextState = new IdleState(npc, agent, anim, player); //switch to idle state
                stage = EVENT.EXIT; //set the stage to exit
            }
        }
    }

    public override void Exit()
    {
       // anim.ResetTrigger("isRunning"); //reset the running trigger on the animator
        base.Exit();
    }
}

public class Attack : State
{
    float rotationSpeed = 5.0f;
    float timeBetweenShots = 0.0f; // Timer pour la cadence

    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) 
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.ATTACK;
    }

    public override void Enter()
    {
       // anim.SetTrigger("isShooting"); // Animation de tir
        agent.isStopped = true;       // On s'arrête
        
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
        // 1. VISER : Regarder le joueur
        if (player != null)
        {
            Vector3 direction = player.position - npc.transform.position;
            direction.y = 0; // On reste à plat
            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
        }

        // 2. TIRER : Utiliser le WPManager
        if (pm != null && pm.selectedItems != null)
        {
            // Gestion du délai entre les tirs
            timeBetweenShots -= Time.deltaTime;
            
            if (timeBetweenShots <= 0f)
            {
                // PAN !
                pm.selectedItems.Use(); 
                
                // On reset le timer avec la cadence de l'arme (ex: 0.1s pour mitraillette, 0.5s pour pistolet)
                if (pm.selectedItems.item != null)
                    timeBetweenShots = pm.selectedItems.item.useRate;
                else
                    timeBetweenShots = 0.5f; // Valeur par défaut
            }
        }

        // 3. TRANSITION : Si le joueur s'éloigne ou meurt
        if (!CanAttackPlayer() || !CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, anim, player); // On repasse en poursuite
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        //anim.ResetTrigger("isShooting");
        agent.isStopped = false; // On autorise le mouvement à nouveau
        base.Exit();
    }
}