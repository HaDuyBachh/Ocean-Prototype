using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Init Water")]
        [Tooltip("Rùa đã ở sẵn dưới nước")]
        public bool _initWater = false;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 3f)]
        public float RotationSmoothTime = 0.6f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Movement")]
        public bool isSwiming = false;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float _gravity = -15.0f;

        public bool _isGround;

        [Header("Slope Handling")]
        [Tooltip("Maximum slope angle (degrees) before sliding")]
        public float MaxSlopeAngle = 45f; // Góc tối đa để trượt

        [Tooltip("Speed of sliding down slopes")]
        public float SlideSpeed = 5f; // Tốc độ trượt

        [Tooltip("Distance to check for slope detection")]
        public float SlopeCheckDistance = 0.5f; // Khoảng cách kiểm tra dốc

        [Header("Swim")]
        public Transform _swimLimitObject;
        public float _offsetLimit = -0.5f;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _yawVelocity;
        private float _pitchVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // animation IDs
        private int _animIDMove;
        private int _animIDSwim;

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


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
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
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            if (_initWater) ChangeToWaterEnvairoment();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            if (!isSwiming)
            {
                GroundedCheck();
                HandleGravity();
                Move();
                AlignToSlope();
            }

            else
            {
                GroundedCheck();
                Swim();
            }
                
        }

        public void ChangeToWaterEnvairoment()
        {
            isSwiming = true;
            _animator.SetBool(_animIDSwim, true);
        }

        public void ChangeToGroundEnvairoment()
        {
            isSwiming = false;
            _animator.SetBool(_animIDSwim, false);
        }

        private void HandleGravity()
        {
            _isGround = _controller.isGrounded;

            if (_controller.isGrounded || Grounded)
            {
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2.0f;

            }
            else
            {
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += _gravity * Time.deltaTime;
                }
            }


            //// Di chuyển nhân vật xuống theo trọng lực
            //_controller.Move(new Vector3(0.0f, _verticalVelocity * Time.deltaTime, 0.0f));
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDMove = Animator.StringToHash("Motion");
            _animIDSwim = Animator.StringToHash("Swim");
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation, transform.rotation.eulerAngles.z);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                //Tăng từ từ chuyển động
                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
                if (_animationBlend < 0.01f) _animationBlend = 0f;

                _animator.SetFloat(_animIDMove, _animationBlend);
            }
        }

        private void Swim()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = 1.5f * (_input.sprint ? SprintSpeed : MoveSpeed);

            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // calculate current speed in 3D
            float currentSpeed = _controller.velocity.magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed / 3, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // calculate movement direction relative to camera
            Vector3 moveDirection = Vector3.zero;

            if (_input.move != Vector2.zero)
            {
                // get camera's forward and right vectors
                Vector3 cameraForward = _mainCamera.transform.forward;
                Vector3 cameraRight = _mainCamera.transform.right;

                // project camera vectors to ignore pitch for horizontal movement
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();

                // calculate horizontal movement direction based on input relative to camera
                moveDirection = (cameraRight * _input.move.x + cameraForward * _input.move.y).normalized;
            }

            // handle vertical movement with jump and crouch
            if (_input.jump && transform.position.y + _offsetLimit < _swimLimitObject.position.y)
            {
                moveDirection.y = 1.0f; // Swim up
            }
            else if (_input.crouch)
            {
                moveDirection.y = -1.0f; // Swim down
            }

            
            //Nhân vật khi chạm đất lên bwờ thì không cần hạ xuống.
            if (!Grounded && !_isGround)
            {
                //Nhân vật phải chìm xuống khi vượt quá _offsetLimit
                if (transform.position.y + _offsetLimit / 2 > _swimLimitObject.position.y)
                {
                    moveDirection.y = -1.0f;
                }
            }

            // Handle pitch rotation (up/down) for jump/crouch
            float currentPitch = transform.eulerAngles.x;
            if (currentPitch > 180f) currentPitch -= 360f; // Normalize to [-180, 180]
            float targetPitch = 0.0f;
            if (_input.jump)
            {
                targetPitch = -20.0f; // Tilt up
            }
            else if (_input.crouch)
            {
                targetPitch = 20.0f; // Tilt down
            }
            float smoothPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref _pitchVelocity, RotationSmoothTime);

            // Handle yaw rotation (left/right) based on camera yaw
            float currentYaw = transform.eulerAngles.y;
            float smoothYaw = currentYaw;
            if (_input.move != Vector2.zero)
            {
                smoothYaw = Mathf.SmoothDampAngle(currentYaw, _cinemachineTargetYaw, ref _yawVelocity, RotationSmoothTime);
            }

            // Apply combined rotation (yaw and pitch)
            transform.rotation = Quaternion.Euler(
                ClampAngle(smoothPitch, -30.0f, 30.0f),
                smoothYaw,
                0.0f
            );

            // move the player
            _controller.Move(moveDirection.normalized * (_speed * Time.deltaTime));

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDMove, _animationBlend);
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void AlignToSlope()
        {
            // Vị trí kiểm tra ngay dưới chân nhân vật
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            RaycastHit hit;

            // Raycast xuống dưới để lấy normal của mặt đất
            if (Physics.Raycast(origin, Vector3.down, out hit, SlopeCheckDistance + 0.2f, GroundLayers))
            {
                // Lấy hướng di chuyển hiện tại
                Vector3 moveDirection = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z);
                if (moveDirection.sqrMagnitude > 0.01f)
                {
                    // Tính toán hướng forward mới dựa trên hướng di chuyển và normal mặt đất
                    Vector3 slopeForward = Vector3.Cross(transform.right, hit.normal).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(slopeForward, hit.normal);

                    // Chỉ xoay theo pitch (nghiêng lên xuống), giữ nguyên yaw (quay ngang)
                    Vector3 euler = targetRotation.eulerAngles;
                    euler.y = transform.eulerAngles.y;
                    targetRotation = Quaternion.Euler(euler);

                    // Xoay mượt mà
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}