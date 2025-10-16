using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    // Camera
    public Camera playerCamera;

    // Input
    public InputActionReference MoveAction;
    public InputActionReference ShootAction;
    public InputActionReference SelectAction;
    public InputActionReference CrouchAction;
    // Mouvements
    public float moveSpeed = 2f;             // vitesse de déplacement
    public float mouseSensitivity = 1.5f;    // sensibilité souris (yaw/pitch)
    public float desiredJumpHeight = 1.5f;   // hauteur de saut

    public bool isCrouching = false;
    public float crouchHeight = 0.5f; // Hauteur du joueur en
    public float originalCameraPos = 0.5f; // Décalage de la caméra en position accroupie
    public float crouchSpeedMultiplier = 0.5f; // Multiplicateur de
    

    //Armes,items
    public WPManager wpManager;

    // Physique
    public Rigidbody rb;                     // drag ton Rigidbody ici dans l’inspector

    private Vector3 direction = Vector3.zero; // x/z = déplacements, y = saut
    private bool isGrounded = true;

    void Update()
    {
        // Si déplacement désactivé, ne pas traiter la rotation ni le saut
        if (!canMove) return;

        // --- Rotation du joueur (yaw) + de la caméra (pitch) avec la souris ---
        // (Garde Input.GetAxis si ton projet est en "Both" pour l'Input System)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Tourne le corps du joueur sur Y (gauche/droite)
        transform.Rotate(0f, mouseX, 0f);

        // Tourne la caméra sur X (haut/bas)
        if (playerCamera != null)
        {
            playerCamera.transform.Rotate(-mouseY, 0f, 0f);

            // Clamp du pitch caméra
            Vector3 currentRotation = playerCamera.transform.localEulerAngles;
            if (currentRotation.x > 180f) currentRotation.x -= 360f; // convertit en angle négatif
            currentRotation.x = Mathf.Clamp(currentRotation.x, -45f, 45f);
            currentRotation.y = 0f; // pas de roll/yaw local sur la cam
            currentRotation.z = 0f;
            playerCamera.transform.localEulerAngles = currentRotation;
        }

        // --- Saut ---
        if (direction.y > 0f && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        // --- Déplacement via Rigidbody, relatif à l'orientation du joueur/caméra ---
        // On transforme l'input local (x=right, z=forward) en direction monde selon le joueur
        Vector3 planarInput = new Vector3(direction.x, 0f, direction.z);
        Vector3 moveWorld = transform.TransformDirection(planarInput); // relatif au yaw du joueur
        Vector3 moveStep = moveWorld * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveStep);
    }

    void OnEnable()
    {
        MoveAction.action.performed += OnMoveActionPerformed;
        MoveAction.action.canceled += OnMoveActionCanceled;
        MoveAction.action.Enable();

        ShootAction.action.started += OnShootStarted;
        ShootAction.action.Enable();

        SelectAction.action.started += OnSelectStarted;
        SelectAction.action.Enable();

        CrouchAction.action.started += OnCrouchStarted;
        CrouchAction.action.canceled += OnCrouchCanceled;
        CrouchAction.action.Enable();
    }

    void OnDisable()
    {
        MoveAction.action.performed -= OnMoveActionPerformed;
        MoveAction.action.canceled -= OnMoveActionCanceled;
        MoveAction.action.Disable();

        ShootAction.action.started -= OnShootStarted;
        ShootAction.action.Disable();

        SelectAction.action.started -= OnSelectStarted;
        SelectAction.action.Disable();

        CrouchAction.action.started -= OnCrouchStarted;
        CrouchAction.action.canceled -= OnCrouchCanceled;
        CrouchAction.action.Disable();
    }

    private void OnMoveActionPerformed(InputAction.CallbackContext context)
    {
        if (canMove)
            direction = context.ReadValue<Vector3>();
        else
            direction = Vector3.zero;
    }

    private void OnMoveActionCanceled(InputAction.CallbackContext context)
    {
        direction = Vector3.zero;
    }

    private void OnShootStarted(InputAction.CallbackContext context)
    {
        if (wpManager != null && wpManager.selectedItems != null)
        {
            wpManager.selectedItems.Use();
        }
    }

    private void OnSelectStarted(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            var touche = context.control.name;
            int select = -1;
            switch (touche)
            {
                case "1":
                    select = 0;
                    break;
                case "2":
                    select = 1;
                    break;
                case "3":
                    select = 2;
                    break;
                case "4":
                    select = 3;
                    break;
                case "5":
                    select = 4;
                    break;  
                default:
                    select = -1;
                    break;
            }
            if (wpManager != null && wpManager.selectedItems != null)
            {
                wpManager.SelectItems(select); // Select item based on input value
            }
        }
    }

    private void Jump()
    {
        float g = -Physics.gravity.y;                 // gravité positive (~9.81)
        float v0 = Mathf.Sqrt(2f * g * desiredJumpHeight);

        Vector3 v = rb.linearVelocity;                 // utiliser linearVelocity à la place de velocity
        v.y = v0;
        rb.linearVelocity = v;

        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
  private void OnCrouchStarted(InputAction.CallbackContext context)
{
    if (!isCrouching)
    {
        // Réduit la taille du joueur
        transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchHeight, originalScale.z);

        // Descend la caméra
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPos - new Vector3(0, crouchCameraOffset, 0);
        }

        // Réduit la vitesse
        moveSpeed *= crouchSpeedMultiplier;
        isCrouching = true;
    }
}

private void OnCrouchCanceled(InputAction.CallbackContext context)
{
    if (isCrouching)
    {
        // Restaure la taille
        transform.localScale = originalScale;

        // Restaure la caméra
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPos;
        }

        // Restaure la vitesse
        moveSpeed = originalSpeed;
        isCrouching = false;
    }
}
  }

    
    public bool canMove = true;
}