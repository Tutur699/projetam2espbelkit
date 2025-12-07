using System.Collections;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float bulletSpeed = 345;
    public float hitForce = 50f;
    public float destroyAfter = 3.5f;

    float currentTime = 0;
    Vector3 newPos;
    Vector3 oldPos;
    bool hasHit = false;

    float damagePoints;

    IEnumerator Start()
    {
        newPos = transform.position;
        oldPos = newPos;

        while (currentTime < destroyAfter && !hasHit)
        {
            Vector3 velocity = transform.forward * bulletSpeed;
            newPos += velocity * Time.deltaTime;
            Vector3 direction = newPos - oldPos;
            float distance = direction.magnitude;
            RaycastHit hit;
            Debug.DrawLine(oldPos, newPos, Color.red, 1.0f);

            // Check if we hit anything on the way
            if (Physics.Raycast(oldPos, direction, out hit, distance))
            {
                hasHit = true;
                Debug.Log($"J'ai touché : {hit.transform.name} (Parent: {hit.transform.parent?.name})");
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(direction * hitForce);
                }
                IEntity enemy = hit.transform.GetComponentInParent<IEntity>();
                if (enemy != null)
                {//Apply damage to NPC
                    Debug.Log(">>> Dégâts appliqués !"); // Confirmateur de succès
                    enemy.ApplyDamage(damagePoints);
                }
                else
                {
                    Debug.Log(">>> Pas de script IEntity trouvé sur cet objet !");
                }
                

                newPos = hit.point; //Adjust new position
                StartCoroutine(DestroyBullet());
            }

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();

            transform.position = newPos;
            oldPos = newPos;
        }

        if (!hasHit)
        {
            StartCoroutine(DestroyBullet());
        }
    }

   IEnumerator DestroyBullet()
    {
        hasHit = true;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

     public void SetDamage(float points)
    {
        damagePoints = points;
    }
}
