using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityManipulationPuzzle
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _turnSmoothTime = 0.1f;
        [SerializeField] private Animator _animator;

        [Header("Jump Settings")]
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private LayerMask _groundLayer;

        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private float _fallDistanceThreshold = 1f; // Threshold distance for detecting fall
        [SerializeField] private float _roomLength = 1000f; // Length of the room for free fall purposes

        private Rigidbody _rb;
        private Transform _cam;
        private IPlayerInputHandler _playerInput;
        private IGravityDirectionProvider _gravityShift;

        private Vector3 _movementInput;

        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");

        private bool _isGrounded;

        private bool _isFalling;

        private void Awake() => InitializeComponents();

        private void OnEnable()
        {
            _playerInput.MoveAction.performed += OnMovementPerformed;
            _playerInput.MoveAction.canceled += OnMovementCanceled;
            _playerInput.JumpAction.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            _playerInput.MoveAction.performed -= OnMovementPerformed;
            _playerInput.MoveAction.canceled -= OnMovementCanceled;
            _playerInput.JumpAction.performed -= OnJumpPerformed;
        }

        private void Update()
        {
            CheckGrounded();
            CheckFalling();
            IsFreeFalling();
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            if (_movementInput.sqrMagnitude > 0.01f)
            {
                Move();
            }
        }

        private void InitializeComponents()
        {
            _cam = Camera.main.transform;
            _rb = GetComponent<Rigidbody>();
            _playerInput = GetComponent<IPlayerInputHandler>();
            _gravityShift = GetComponent<GravityShift>();

            if (_animator == null)
                Debug.Log("Animator component is not assigned");
        }

        #region Input Methods
        private void OnMovementPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 inputVector = ctx.ReadValue<Vector2>();

            _movementInput = new Vector3(inputVector.x, 0, inputVector.y);
        }

        private void OnMovementCanceled(InputAction.CallbackContext ctx) => _movementInput = Vector3.zero;

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            if (_isGrounded)
            {
                Jump();
            }
        }
        #endregion

        private void Move()
        {
            Vector3 gravityUp = -_gravityShift.GravityDirection.normalized;
            Vector3 moveDir = Vector3.ProjectOnPlane(_cam.forward * _movementInput.z + _cam.right * _movementInput.x, gravityUp).normalized;

            if (moveDir.sqrMagnitude >= 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir, gravityUp);
                _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, _turnSmoothTime);
                _rb.MovePosition(_rb.position + _moveSpeed * Time.deltaTime * moveDir);
            }
        }

        private void Jump() => _rb.AddForce(-_gravityShift.GravityDirection * _jumpForce, ForceMode.Impulse);

        private void CheckGrounded()
        {
            _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        private void CheckFalling()
        {
            bool isGroundBelow = Physics.Raycast(_groundCheck.position, -_groundCheck.up, out RaycastHit hit, _fallDistanceThreshold, _groundLayer);

            _isFalling = !isGroundBelow && !_isGrounded;
        }

        // Method to check if the character is freely falling without any ground in any direction, since character can shift the gravity
        private void IsFreeFalling()
        {
            float rayDistance = _roomLength;
            Vector3 origin = transform.position;

            // Check below for ground
            if (!Physics.Raycast(origin, -transform.up, out RaycastHit hitDown, rayDistance, _groundLayer))
            {
                // If no ground below, check in cardinal directions
                bool isGroundAround = Physics.Raycast(origin, transform.right, rayDistance, _groundLayer) ||
                                      Physics.Raycast(origin, -transform.right, rayDistance, _groundLayer) ||
                                      Physics.Raycast(origin, transform.forward, rayDistance, _groundLayer) ||
                                      Physics.Raycast(origin, -transform.forward, rayDistance, _groundLayer);

                if (!isGroundAround)
                {
                    GameManager.Instance.GameOver("Game Over: Freely Falling!");
                }
            }
        }

        private void UpdateAnimations()
        {
            bool isRunning = _movementInput.magnitude >= 0.1f && !_isFalling;

            _animator.SetBool(IsRunningHash, isRunning);
            _animator.SetBool(IsFallingHash, _isFalling);
        }
    }
}