using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerMotor : MonoBehaviour
{
    private Vector3 velocity; // Vecteur de vélocité
    private float yRotation; // rotation autour de l'axe Y (en degrés par frame ou selon convention)

    private Rigidbody rb; // Référence au Rigidbody

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); // Récupère le Rigidbody attaché au même GameObject
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity; // Met à jour la vélocité
    }

    // Reçoit la rotation en Y (par ex. lookInput.x multiplié par une sensibilité)
    public void Rotate(float _yRotation)
    {
        yRotation = _yRotation;
    }

    private void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    private void PerformMovement()
    {
        if (velocity != Vector3.zero) // Si la vélocité n'est pas nulle
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime); // Déplace le Rigidbody en fonction de la vélocité
        }
    }

    private void PerformRotation()
    {
        if (Mathf.Abs(yRotation) > 0f)
        {
            // Calculer la rotation désirée
            Quaternion deltaRotation = Quaternion.Euler(0f, yRotation, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
            // Remise à zéro de yRotation pour éviter accumulation non voulue
            yRotation = 0f;
        }
    }
}
