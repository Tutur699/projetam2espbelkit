using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControler : NetworkBehaviour
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

    //Armes,items
    public WPManager wpManager;

    // Physique
    public Rigidbody rb;

    private Vector3 direction = Vector3.zero; // x/z = déplacements, y = saut
    private bool isGrounded = true;

    public bool canMove = true;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // checher avec .find le body où y'a la capsule à colorer en rouge
            Renderer bodyRenderer = FindChildRenderer("body");
            Renderer gunRenderer  = FindChildRenderer("Cube"); // ton Gun visible

            // Teinte (MaterialPropertyBlock = propre et sans dupliquer les matériaux)
            Tint(bodyRenderer, Color.red);
            Tint(gunRenderer,  Color.red);
            }
    }
    Renderer FindChildRenderer(string childName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
            {
                return t.GetComponent<Renderer>();
            }
        Debug.LogWarning($"Renderer introuvable pour '{childName}' sous {name}");
        return null; //pas trouvé
    }

    void Tint(Renderer r, Color c)
    {
        if (r == null) return;
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", c); // URP/HDRP
        mpb.SetColor("_Color", c);     // Built-in
        //les deux cas peuvent être nécessaire malgré notre projet en urp....
        r.SetPropertyBlock(mpb);
    }

    void Start()
    {
        // init crouch
        originalScale = transform.localScale;
        originalSpeed = moveSpeed;
        if (playerCamera != null)
            originalCameraLocalPos = playerCamera.transform.localPosition;
        else
            originalCameraLocalPos = Vector3.zero;
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

        // Saut
        if (direction.y > 0f && isGrounded)
            Jump();
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector3 planarInput = new Vector3(direction.x, 0f, direction.z);
        Vector3 moveWorld = transform.TransformDirection(planarInput);
        Vector3 moveStep = moveWorld * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveStep);
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

    // --- Input handlers ---
    private void OnMoveActionPerformed(InputAction.CallbackContext context)
    {
        // Tu lis en Vector3 dans ton asset => on garde Vector3
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


    // --- Crouch ---
    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        if (isCrouching) return;

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

        // restaurer taille
        transform.localScale = originalScale;

        // restaurer caméra
        if (playerCamera != null)
            playerCamera.transform.localPosition = originalCameraLocalPos;

        // restaurer vitesse
        moveSpeed = originalSpeed;

        isCrouching = false;
    }

    // --- Physique ---
    private void Jump()
    {
        float g = -Physics.gravity.y;
        float v0 = Mathf.Sqrt(2f * g * desiredJumpHeight);

        Vector3 v = rb.linearVelocity; // <- au lieu de linearVelocity
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
}
