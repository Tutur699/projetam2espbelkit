using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    [Header("Camera & Audio")]
    [SerializeField] Camera playerCamera;          // désactivée dans le prefab

    [Header("Input (New Input System)")]
    public InputActionReference MoveAction;
    public InputActionReference ShootAction;
    public InputActionReference SelectAction;
    public InputActionReference CrouchAction;
    public InputActionReference JumpAction;


    [Header("Move")]
    public float moveSpeed = 2f;
    public float mouseSensitivity = 1.5f;
    public float desiredJumpHeight = 1.5f;

    [Header("Crouch")]
    public bool  isCrouching = false;
    public float crouchHeight = 0.5f;
    public float crouchCameraOffset = 0.5f;
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Gameplay")]
    public WPManager wpManager;

    // runtime
    public Rigidbody rb;
    Vector3 direction = Vector3.zero;  // x/z = déplacement, y = saut (flag)
    bool isGrounded = true;
    public bool canMove = false;

    Vector3 originalScale;
    Vector3 originalCameraLocalPos;
    float   originalSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // tout “local-only” OFF par défaut dans le prefab
        if (playerCamera)   playerCamera.enabled = false;
        if (audioListener)  audioListener.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            EnableLocal(true);

            var bodyR = FindChildRenderer("body");
            var gunR  = FindChildRenderer("Cube");
            Tint(bodyR, Color.red);
            Tint(gunR,  Color.red);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else
        {
            EnableLocal(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        EnableLocal(false);
    }

    void Start()
    {
        originalScale          = transform.localScale;
        originalSpeed          = moveSpeed;
        originalCameraLocalPos = playerCamera ? playerCamera.transform.localPosition : Vector3.zero;
    }

    void Update()
    {
        if (!IsOwner)        return;
        if (!inputsEnabled)  return;

        // ----- Look souris -----
        float mouseX, mouseY;
        if (LookAction && LookAction.action != null)
        {
            Vector2 look = LookAction.action.ReadValue<Vector2>();
            mouseX = look.x * mouseSensitivity * Time.deltaTime * 10f;
            mouseY = look.y * mouseSensitivity * Time.deltaTime * 10f;
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        // yaw sur le corps
        transform.Rotate(0f, mouseX, 0f);

        // pitch sur la caméra
        if (playerCamera != null)
        {
            playerCamera.transform.Rotate(-mouseY, 0f, 0f);
            Vector3 r = playerCamera.transform.localEulerAngles;
            if (r.x > 180f) r.x -= 360f;
            r.x = Mathf.Clamp(r.x, -45f, 45f);
            r.y = 0f; r.z = 0f;
            playerCamera.transform.localEulerAngles = r;
        }

        // Saut si on a mis un flag direction.y ailleurs
        if (direction.y > 0f && isGrounded)
            Jump();
    }

    void FixedUpdate()
    {
        if (!IsOwner)       return;
        if (!inputsEnabled) return;

        Vector3 planarInput = new Vector3(direction.x, 0f, direction.z);
        Vector3 moveWorld = transform.TransformDirection(planarInput);
        Vector3 moveStep = moveWorld * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveStep);
    }

    // ---------- Enable/disable “local only” ----------
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
            playerCamera.enabled = enable;
            playerCamera.tag     = enable ? "MainCamera" : "Untagged";
        }

        if (audioListener)
            audioListener.enabled = enable;

        // liaisons d’inputs
        Bind(MoveAction,   enable, null,                 OnMoveActionPerformed, OnMoveActionCanceled);
        Bind(ShootAction,  enable, OnShootStarted);
        Bind(SelectAction, enable, OnSelectStarted);
        Bind(CrouchAction, enable, OnCrouchStarted,      null,                  OnCrouchCanceled);

        // LookAction séparée
        if (LookAction?.action != null)
        {
            if (enable) LookAction.action.Enable();
            else        LookAction.action.Disable();
        }

        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !enable;

    }

    /// <summary>
    /// Helper générique pour brancher/débrancher les callbacks du New Input System.
    /// </summary>
    void Bind(InputActionReference aref,
              bool enable,
              System.Action<InputAction.CallbackContext> onStarted  = null,
              System.Action<InputAction.CallbackContext> onPerformed = null,
              System.Action<InputAction.CallbackContext> onCanceled  = null)
    {
        if (aref == null || aref.action == null)
            return;

        var a = aref.action;

        if (enable)
        {
            if (onStarted  != null) a.started   += onStarted;
            if (onPerformed!= null) a.performed += onPerformed;
            if (onCanceled != null) a.canceled  += onCanceled;
            a.Enable();
        }
        if (ShootAction?.action != null)
        {
            if (onStarted  != null) a.started   -= onStarted;
            if (onPerformed!= null) a.performed -= onPerformed;
            if (onCanceled != null) a.canceled  -= onCanceled;
            a.Disable();
        }
    }

    // ---------- Input handlers ----------

    // Déplacement : lit un Vector2 (x = horizontal, y = vertical) et le stocke dans direction.x/z
    void OnMoveActionPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        Vector2 v = ctx.ReadValue<Vector2>();  // WASD / stick
        if (canMove)
            direction = new Vector3(v.x, direction.y, v.y);
        else
            direction = new Vector3(0f, direction.y, 0f);
    }

    void OnMoveActionCanceled(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        direction = new Vector3(0f, direction.y, 0f);
    }

    void OnShootStarted(InputAction.CallbackContext ctx)
    {
        //if (!IsOwner) return;
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

    void OnSelectStarted(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        int select = -1;
        switch (ctx.control.name)
        {
            case "1": select = 0; break;
            case "2": select = 1; break;
            case "3": select = 2; break;
            case "4": select = 3; break;
            case "5": select = 4; break;
            default: return;
        }
        if (select >= 0 && wpManager != null)
            wpManager.SelectItems(select);
    }

    // ---------- Crouch ----------
    void OnCrouchStarted(InputAction.CallbackContext ctx)
    {
        if (!IsOwner || isCrouching) return;

        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y * Mathf.Clamp01(crouchHeight),
            originalScale.z
        );

        if (playerCamera)
            playerCamera.transform.localPosition = originalCameraLocalPos - new Vector3(0f, crouchCameraOffset, 0f);

        moveSpeed   = originalSpeed * crouchSpeedMultiplier;
        isCrouching = true;
    }

    void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        if (!IsOwner || !isCrouching) return;

        transform.localScale = originalScale;

        if (playerCamera)
            playerCamera.transform.localPosition = originalCameraLocalPos;

        moveSpeed   = originalSpeed;
        isCrouching = false;
    }

    void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
            Jump();
    }

    // ---------- Physique ----------
    void Jump()
    {
        float g  = -Physics.gravity.y;
        float v0 = Mathf.Sqrt(2f * g * desiredJumpHeight);

        var v = rb.linearVelocity;
        v.y   = v0;
        rb.linearVelocity = v;

        isGrounded  = false;
        direction.y = 0f;
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // ---------- Utils ----------
    Renderer FindChildRenderer(string childName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
                return t.GetComponent<Renderer>();

        return null;
    }

    void Tint(Renderer r, Color c)
    {
        if (!r) return;

        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", c);
        mpb.SetColor("_Color",      c);
        r.SetPropertyBlock(mpb);
    }
}
