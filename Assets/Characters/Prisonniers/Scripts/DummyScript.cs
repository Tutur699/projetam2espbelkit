using UnityEngine;

public class DummyScript : MonoBehaviour, IEntity
{
    public float npcHP = 100;
    public void ApplyDamage(float points)
    {
        npcHP -= points;
        /*if (npcHP <= 0)
        {
            //Destroy the NPC
            GameObject npcDead = Instantiate(npcDeadPrefab, transform.position, transform.rotation);
            //Slightly bounce the npc dead prefab up
            npcDead.GetComponent<Rigidbody>().linearVelocity = (-(playerTransform.position - transform.position).normalized * 8) + new Vector3(0, 5, 0);
            Destroy(npcDead, 10);
            Destroy(gameObject);*/
        }
    }

