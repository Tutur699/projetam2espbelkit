using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WeaponSway : MonoBehaviour
{
    [Header("Réglages de Position")]
    public float amount = 0.02f;   // Distance du déplacement (plus grand = plus de mouvement)
    public float maxAmount = 0.06f; // Limite max pour ne pas que l'arme sorte de l'écran
    public float smoothAmount = 6f; // Vitesse de retour (plus grand = plus rapide/léger)

    [Header("Réglages de Rotation (Tilt)")]
    public float rotationAmount = 4f;
    public float maxRotationAmount = 5f;
    public float smoothRotation = 12f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Variables pour stocker l'input de la souris
    private float InputX;
    private float InputY;

    void Start()
    {
        // On mémorise la position de départ (le centre)
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        CalculateInput();
        MoveSway();
        TiltSway();
    }

    void CalculateInput()
    {
        // On essaie de récupérer l'input de la souris
        // Cette méthode marche souvent même avec le nouveau Input System si l'option "Both" est activée
        // Sinon, on pourrait lire directement StarterAssetsInputs, mais restons génériques ici.
        
#if ENABLE_INPUT_SYSTEM
        // Si on utilise le nouveau système, on essaie de lire la souris directement
        var mouse = Mouse.current;
        if (mouse != null)
        {
            // On divise par un facteur pour simuler le comportement de l'ancien Input.GetAxis
            InputX = mouse.delta.x.ReadValue() * 0.05f; 
            InputY = mouse.delta.y.ReadValue() * 0.05f;
        }
#else
        // Ancien système
        InputX = Input.GetAxis("Mouse X");
        InputY = Input.GetAxis("Mouse Y");
#endif
    }

    void MoveSway()
    {
        // Calcul du mouvement opposé à la souris
        float moveX = -InputX * amount;
        float moveY = -InputY * amount;

        // On limite le mouvement (Clamp)
        moveX = Mathf.Clamp(moveX, -maxAmount, maxAmount);
        moveY = Mathf.Clamp(moveY, -maxAmount, maxAmount);

        // Position cible
        Vector3 finalPosition = new Vector3(moveX, moveY, 0);

        // Application fluide (Lerp)
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }

    void TiltSway()
    {
        // Calcul de la rotation (Tilt)
        float tiltX = -InputY * rotationAmount; // Haut/Bas
        float tiltY = InputX * rotationAmount;  // Gauche/Droite

        // On limite la rotation
        tiltX = Mathf.Clamp(tiltX, -maxRotationAmount, maxRotationAmount);
        tiltY = Mathf.Clamp(tiltY, -maxRotationAmount, maxRotationAmount);

        // Rotation cible
        Quaternion finalRotation = Quaternion.Euler(new Vector3(tiltX, tiltY, 0));

        // Application fluide (Slerp)
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * initialRotation, Time.deltaTime * smoothRotation);
    }
}
