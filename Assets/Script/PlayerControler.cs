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
    public float moveSpeed = 2f;           // vitesse de déplacement
    public float mouseSensitivity = 1.5f;  // sensibilité souris
    public float desiredJumpHeight = 1.5f; // hauteur de saut

    // Crouch
    public bool isCrouching = false;
    public float crouchHeight = 0.5f;            // facteur d'échelle sur Y (0.5 = moitié)
    public float crouchCameraOffset = 0.5f;      // descente de la camera en m
    public float crouchSpeedMultiplier = 0.5f;   // multiplicateur de vitesse quand accroupi

    private Vector3 originalScale;               // <-- manquait
    private Vector3 originalCameraLocalPos;      // <-- manquait
    private float originalSpeed;                 // <-- manquait

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;       // hauteur de saut désirée (m)
    [SerializeField] private float gravity = -15f;          // gravité "propre" (négative)
    [SerializeField] private float jumpTimeout = 0.1f;      // anti-spam
    [SerializeField] private float fallTimeout = 0.15f;     // anti "tombe instantanément"
    [SerializeField] private float terminalVelocity = 53f;  // vmax de chute (m/s)


    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundedRadius = 0.5f;
    [SerializeField] private float groundedOffset = 0.2f;

    private bool grounded;
    private float verticalVelocity;
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;
    private Vector3 direction = Vector3.zero;

    //Armes,items
    public WPManager wpManager;

    // Physique
    public Rigidbody rb;

    private bool jumpPressed;

    public bool canMove = true;

    void Start()
    {
        // init crouch
        originalScale = transform.localScale;
        originalSpeed = moveSpeed;
        if (playerCamera != null)
            originalCameraLocalPos = playerCamera.transform.localPosition;
        else
            originalCameraLocalPos = Vector3.zero;

        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    void Update()
    {
        if (!canMove) return;

        // souris -> yaw/pitch
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        if (playerCamera != null)
        {
            playerCamera.transform.Rotate(-mouseY, 0f, 0f);

            // clamp pitch
            Vector3 r = playerCamera.transform.localEulerAngles;
            if (r.x > 180f) r.x -= 360f;
            r.x = Mathf.Clamp(r.x, -45f, 45f);
            r.y = 0f; r.z = 0f;
            playerCamera.transform.localEulerAngles = r;
        }

        GroundedCheck();
        JumpAndGravity();

    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector3 planarInput = new Vector3(direction.x, 0f, direction.z);
        Vector3 moveWorld = transform.TransformDirection(planarInput);
        Vector3 moveStep = moveWorld * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveStep);
    }

    private void Awake() {
    rb = GetComponent<Rigidbody>();
    if (rb != null) rb.useGravity = false; // on gère la gravité nous-mêmes
    }

    void OnEnable()
    {
        if (MoveAction?.action != null)
        {
            MoveAction.action.performed += OnMoveActionPerformed;
            MoveAction.action.canceled  += OnMoveActionCanceled;
            MoveAction.action.Enable();
        }

        if (ShootAction?.action != null)
        {
            ShootAction.action.started += OnShootStarted;
            ShootAction.action.Enable();
        }

        if (SelectAction?.action != null)
        {
            SelectAction.action.started += OnSelectStarted;
            SelectAction.action.Enable();
        }

        if (CrouchAction?.action != null)
        {
            CrouchAction.action.started += OnCrouchStarted;
            CrouchAction.action.canceled += OnCrouchCanceled;
            CrouchAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (MoveAction?.action != null)
        {
            MoveAction.action.performed -= OnMoveActionPerformed;
            MoveAction.action.canceled  -= OnMoveActionCanceled;
            MoveAction.action.Disable();
        }

        if (ShootAction?.action != null)
        {
            ShootAction.action.started -= OnShootStarted;
            ShootAction.action.Disable();
        }

        if (SelectAction?.action != null)
        {
            SelectAction.action.started -= OnSelectStarted;
            SelectAction.action.Disable();
        }

        if (CrouchAction?.action != null)
        {
            CrouchAction.action.started -= OnCrouchStarted;
            CrouchAction.action.canceled -= OnCrouchCanceled;
            CrouchAction.action.Disable();
        }
    }


    private void OnMoveActionPerformed(InputAction.CallbackContext context)
    {
       
        direction = canMove ? context.ReadValue<Vector3>() : Vector3.zero;
    }

    private void OnMoveActionCanceled(InputAction.CallbackContext context)
    {
        direction = Vector3.zero;
    }

    private void OnShootStarted(InputAction.CallbackContext context)
    {
        if (wpManager.selectedItems != null && wpManager.selectedItems.isEquipped)
        {
            Debug.Log("Using item: " + wpManager.selectedItems.item);
            wpManager.selectedItems.Use();
        }
        else
        {
            Debug.Log("No item selected to use.");
        }
    }

    private void OnSelectStarted(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            var touche = context.control.name;
            int select = wpManager.selectedSlot;
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
            wpManager.ChangeSelectedSlot(select);
            if (wpManager != null && wpManager.selectedItems != null)
            {
                wpManager.SelectItems(select); // Select item based on input value
            }
        }
    }


    private void GroundedCheck()
    {
        // position de la sphère sous le joueur
        Vector3 spherePos = transform.position + Vector3.down * groundedOffset;
        grounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            // reset fall timer
            fallTimeoutDelta = fallTimeout;

            // écrase la chute résiduelle quand on touche le sol
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            // saut si on a pressé et timeout écoulé
            if (jumpPressed && jumpTimeoutDelta <= 0f)
            {
                // v0 = sqrt(h * -2 * g)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // décrémente le jump timeout
            if (jumpTimeoutDelta > 0f)
                jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            // reset jump timeout quand on quitte le sol
            jumpTimeoutDelta = jumpTimeout;

            // fall timeout diminue
            if (fallTimeoutDelta > 0f)
                fallTimeoutDelta -= Time.deltaTime;

            // empêche le multi-saut tant qu'on est en l'air
            jumpPressed = false;
        }

        // applique la gravité jusqu’à la vitesse terminale
        if (verticalVelocity > -terminalVelocity)
        {
            verticalVelocity += gravity * Time.deltaTime; // gravity est négative
        }

        // pousse la vitesse verticale dans le Rigidbody sans toucher l'horizontal
        if (rb != null)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = verticalVelocity;
            rb.linearVelocity = vel;
        }

        // reset du flag de saut (on consomme l’input)
        jumpPressed = false;
    }
    
    private void OnCrouchStarted(InputAction.CallbackContext context)
{
    if (isCrouching) return; // déjà accroupi ? rien faire

    // réduire la taille (uniquement Y)
    transform.localScale = new Vector3(
        originalScale.x,
        originalScale.y * Mathf.Clamp01(crouchHeight),
        originalScale.z
    );

    // descendre la caméra
    if (playerCamera != null)
        playerCamera.transform.localPosition = originalCameraLocalPos - new Vector3(0f, crouchCameraOffset, 0f);

    // réduire la vitesse
    moveSpeed = originalSpeed * crouchSpeedMultiplier;

    isCrouching = true;
}

private void OnCrouchCanceled(InputAction.CallbackContext context)
{
    if (!isCrouching) return;

    // restaurer la taille du joueur
    transform.localScale = originalScale;

    // restaurer la position de la caméra
    if (playerCamera != null)
        playerCamera.transform.localPosition = originalCameraLocalPos;

    // restaurer la vitesse
    moveSpeed = originalSpeed;

    isCrouching = false;
}

}
