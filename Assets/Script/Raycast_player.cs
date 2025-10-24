using UnityEngine;

public class Raycast_player : MonoBehaviour
{
    public KeyCode key = KeyCode.P;   // touche pour interagir
    public float distmax = 5f;        // distance maximale du raycast

    void Update()
    {
        // On trace le rayon dans la scène (visible dans l’onglet "Scene" pour déboguer)
        Debug.DrawRay(transform.position, transform.forward * distmax, Color.green);

        // Si le joueur appuie sur la touche d’interaction
        if (Input.GetKeyDown(key))
        {
            // On déclare une variable pour récupérer les infos du Raycast
            RaycastHit hit;

            // On lance le rayon vers l’avant du joueur
            if (Physics.Raycast(transform.position, transform.forward, out hit, distmax))
            {
                // Affiche le nom de l’objet touché dans la console
                Debug.Log("Objet touché : " + hit.transform.name);

                // On cherche le script DoorHingeInteraction sur l’objet touché ou ses parents
                DoorHingeInteraction door = hit.collider.GetComponentInParent<DoorHingeInteraction>();

                // Si l’objet touché est bien une porte, on la fait s’ouvrir ou se fermer
                if (door != null)
                {
                    door.Toggle();
                }
            }
        }
    }
}
