using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] //Pour le faire apparaitre dans l'inspecteur
    private float speed;

    private PlayerMotor motor;

    private void Start()
    {
        motor = GetComponent<PlayerMotor>(); //Récupère le PlayerMotor attaché au même GameObject
    }

    private void Update()
    {
        // Calculer la velocité basée sur l'entrée utilisateur
        float xMove = Input.GetAxisRaw("Horizontal"); // Mouvement horizontal 
        float zMove = Input.GetAxis("Vertical");   // Mouvement vertical 

        Vector3 moveHorizontal = transform.right * xMove; // Mouvement horizontal relatif à la rotation du joueur
        Vector3 moveVertical = transform.forward * zMove; // Mouvement vertical relatif à la rotation du joueur

        Vector3 velocity = (moveHorizontal + moveVertical).normalized * speed; // Vecteur de vélocité final
    }
}
