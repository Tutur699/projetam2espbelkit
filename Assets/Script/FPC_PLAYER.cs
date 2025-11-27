using UnityEngine;
using Unity.Netcode;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FPC_PLAYER : NetworkBehaviour
    {
        [Header("Local Only Components")]
        [SerializeField] private MonoBehaviour[] localControllers;
        [SerializeField] private GameObject[] localOnlyObjects;
        [SerializeField] private GameObject worldModel;
        [SerializeField] private Cinemachine.CinemachineVirtualCamera cinemachineVirtualCamera;
        [Header("Weapon System")]
        [SerializeField] public WPManager manager;
        public InputActionReference SelectAction;
        private float _fireTimeoutDelta; // Le compte à rebours
        private bool _wasShootingLastFrame;
        private bool _triggerReleased = true; 

        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        public override void OnNetworkSpawn()
        {
            bool isLocal = IsOwner;
            if(IsOwner)
            {
                // Ceci est le joueur local
                cinemachineVirtualCamera.Priority = 1;
                // Activer les composants locaux
                foreach (var comp in localControllers)
                {
                    comp.enabled = true;
                }

                // Activer les objets locaux
                foreach (var obj in localOnlyObjects)
                {
                    obj.SetActive(true);
                }

                // Désactiver le modèle 3D dans le monde pour le joueur local
                if (worldModel != null)
                {
                    worldModel.SetActive(false);
                }
            }
            else
            {
                cinemachineVirtualCamera.Priority = 0;
                // Ceci est un autre joueur
                // Désactiver les composants locaux pour les autres joueurs
                foreach (var comp in localControllers)
                {
                    comp.enabled = false;
                }

                // Désactiver les objets locaux pour les autres joueurs
                foreach (var obj in localOnlyObjects)
                {
                    obj.SetActive(false);
                }
            }
            base.OnNetworkSpawn();

            _controller   = GetComponent<CharacterController>();
            _input        = GetComponent<StarterAssetsInputs>();
        #if ENABLE_INPUT_SYSTEM
            _playerInput  = GetComponent<PlayerInput>();
        #endif

            if (_controller != null) _controller.enabled = true;

            if (isLocal)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;

                if (Camera.main != null)
                    _mainCamera = Camera.main.gameObject;
            }

            Debug.Log($"[FPC_PLAYER] Spawn {name} - IsOwner={IsOwner}, IsServer={IsServer}, OwnerClientId={OwnerClientId}, LocalClientId={NetworkManager.Singleton.LocalClientId}");

            // --- INPUTS ---
        #if ENABLE_INPUT_SYSTEM
            if (_playerInput != null) _playerInput.enabled = isLocal;
        #endif
            if (_input      != null) _input.enabled      = isLocal;

            // Le CharacterController reste actif pour tout le monde
            if (_controller != null) _controller.enabled = true;

        }


        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
<<<<<<< Updated upstream
            if (!IsOwner) return;

=======
            //if (!IsOwner) return;
            if (_input == null) return;
>>>>>>> Stashed changes
            _hasAnimator = TryGetComponent(out _animator);
            Debug.Log($"[FPC_PLAYER] move={_input?.move}, look={_input?.look}");
            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleShooting();
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;
            CameraRotation();
        }

        private void HandleShooting()
{
    // 1. Gestion du Cooldown
    if (_fireTimeoutDelta > 0.0f)
    {
        _fireTimeoutDelta -= Time.deltaTime;
    }

    // 2. Vérification si on a une arme équipée
    if (manager.selectedItems != null && manager.selectedItems.isEquipped)
    {
        // On récupère les infos depuis le ScriptableObject
        Items currentItemData = manager.selectedItems.item;
        
        bool isAuto = currentItemData.isAutomatic; // Nouveau booléen
        float fireRate = currentItemData.useRate;
        bool isConsumable = currentItemData.singleUse; // Ton booléen "Usage unique"

        // 3. LOGIQUE D'INPUT
        if (_input.shoot) // Le joueur appuie
        {
            // On détermine si on a le droit de tirer
            bool canShoot = false;

            if (isAuto)
            {
                // AUTOMATIQUE : On tire tant que le cooldown est fini
                if (_fireTimeoutDelta <= 0.0f) canShoot = true;
            }
            else
            {
                // SEMI-AUTO (Coup par coup) : On tire seulement si cooldown fini ET gâchette relâchée avant
                if (_fireTimeoutDelta <= 0.0f && _triggerReleased) canShoot = true;
            }

            // 4. ACTION DE TIR
            if (canShoot)
            {
                manager.selectedItems.Use(); // Pan !
                
                _fireTimeoutDelta = fireRate; // On lance le timer
                _triggerReleased = false;     // On verrouille la gâchette

                // GESTION DU "SINGLE USE" (Usage unique)
                if (isConsumable)
                {
                    // L'item est consommé, on le retire de l'inventaire
                    // Tu devras peut-être adapter cette ligne selon ta méthode pour "jeter/supprimer"
                    Debug.Log("Item à usage unique consommé !");
                    // manager.RemoveItem(manager.selectedSlot); // Exemple imaginaire
                    
                    // Pour l'instant, on déséquipe juste pour éviter le crash
                    manager.selectedItems.gameObject.SetActive(false);
                    manager.selectedItems = null;
                }
            }
            else if (!isAuto)
            {
                // Si on est en semi-auto et qu'on maintient le clic, on s'assure que le flag reste false
                _triggerReleased = false;
            }
        }
        else // Le joueur RELÂCHE le bouton
        {
            _triggerReleased = true; // On réarme le mécanisme pour le prochain coup
        }
    }
}

         void OnEnable()
        {
            if (SelectAction?.action != null)
        {
            SelectAction.action.started += OnSelectStarted;
            SelectAction.action.Enable();
        }
        }
        void OnDisable()
        {
            if (SelectAction?.action != null)
        {
            SelectAction.action.started -= OnSelectStarted;
            SelectAction.action.Disable();
        }
    }
    private void OnSelectStarted(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
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
        if (select >= 0 && manager != null)
            manager.ChangeSelectedSlot(select);   
        }


        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z);

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // si on a un input de caméra et que la caméra n’est pas lock
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // souris : pas de Time.deltaTime, manette : avec Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw   += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // on clamp les angles
            _cinemachineTargetYaw   = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine suit ce target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }







        private void Move()
        {
            if (_mainCamera == null && Camera.main != null)
                _mainCamera = Camera.main.gameObject;
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
                targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y).normalized;

            // orientation du mouvement = orientation de la caméra
            _targetRotation = _mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetRotation,
                ref _rotationVelocity,
                RotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * inputDirection;

            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}

