using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityManipulationPuzzle
{
    /// <summary>
    /// Provides the gravity direction for the gravity shift system.
    /// </summary>
    public interface IGravityDirectionProvider
    {
        Vector3 GravityDirection { get; }
    }

    /// <summary>
    /// Handles gravity shifting mechanics, including hologram preview and alignment.
    /// </summary>
    public class GravityShift : MonoBehaviour, IGravityDirectionProvider
    {
        [SerializeField] private GameObject _hologramPlayer;

        private IPlayerInputHandler _playerInput;
        private Rigidbody _rb;
        private Transform _cam;

        // Predefined world axes for aligning gravity.
        private static readonly Vector3[] _worldAxes =
        {
            Vector3.right, Vector3.left, Vector3.up,
            Vector3.down, Vector3.forward, Vector3.back
        };

        private Vector3 _gravityVector; // Stores the desired gravity shift direction.
        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _hologramGravityDirection;

        private const float GRAVITY = -9.81f; // Default gravity constant.
        private Vector3 _gravityDirection; // Current applied gravity direction.

        public Vector3 GravityDirection => _gravityDirection; // Property to access gravity direction.

        private void Awake()
        {
            InitializeComponents();
            _gravityDirection = Vector3.down;
            _hologramPlayer.SetActive(false);
        }

        private void OnEnable()
        {
            _playerInput.GravityShiftPreviewAction.started += OnGravityShiftPerformed;
            _playerInput.ShiftGravityAction.performed += OnApplyGravityShiftPerformed;
        }

        private void OnDisable()
        {
            _playerInput.GravityShiftPreviewAction.started -= OnGravityShiftPerformed;
            _playerInput.ShiftGravityAction.performed -= OnApplyGravityShiftPerformed;
        }

        private void Update()
        {
            UpdateHologramPosition();
        }

        /// <summary>
        /// Initializes required components.
        /// </summary>
        private void InitializeComponents()
        {
            _playerInput = GetComponent<IPlayerInputHandler>();
            _rb = GetComponent<Rigidbody>();
            _cam = Camera.main.transform;
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
        }

        /// <summary>
        /// Updates the hologram's position to match the player's.
        /// </summary>
        private void UpdateHologramPosition()
        {
            if (transform.position != _previousPosition)
            {
                _hologramPlayer.transform.position = transform.position;
                _previousPosition = transform.position;
            }

            if (transform.rotation != _previousRotation)
            {
                Quaternion rotationOffset = Quaternion.FromToRotation(transform.up, _hologramPlayer.transform.up);
                _hologramPlayer.transform.rotation = rotationOffset * transform.rotation;

                _previousRotation = transform.rotation;
            }
        }

        /// <summary>
        /// Activates and positions the hologram at the player's current location.
        /// </summary>
        private void SetHologram()
        {
            _hologramPlayer.SetActive(true);
            _hologramPlayer.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        /// <summary>
        /// Aligns the hologram based on the player's input direction and camera orientation.
        /// </summary>
        private void AlignHologram()
        {
            Vector3 gravityUp = -Physics.gravity.normalized;
            Vector3 projectedForward = Vector3.ProjectOnPlane(_cam.forward, gravityUp).normalized;

            if (_gravityVector == Vector3.right)
            {
                AlignYAxisToClosestWorldAxis(_cam.transform.right);
            }
            else if (_gravityVector == Vector3.left)
            {
                AlignYAxisToClosestWorldAxis(-_cam.transform.right);
            }
            else if (_gravityVector == Vector3.forward)
            {
                AlignYAxisToClosestWorldAxis(projectedForward);
            }
            else if (_gravityVector == Vector3.back)
            {
                AlignYAxisToClosestWorldAxis(-projectedForward);
            }
        }

        /// <summary>
        /// Aligns the hologram's Y-axis to the closest world axis.
        /// </summary>
        /// <param name="alignAxis">The axis to align towards.</param>
        private void AlignYAxisToClosestWorldAxis(Vector3 alignAxis)
        {
            float smallestAngle = float.MaxValue;
            Vector3 closestWorldAxis = Vector3.up;

            foreach (var worldAxis in _worldAxes)
            {
                float angle = Vector3.Angle(alignAxis, worldAxis);
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    closestWorldAxis = worldAxis;
                }
            }

            _hologramGravityDirection = closestWorldAxis;

            if (_hologramGravityDirection == -transform.up) return;

            _hologramPlayer.transform.rotation = Quaternion.FromToRotation(
                _hologramPlayer.transform.up, -closestWorldAxis
            ) * _hologramPlayer.transform.rotation;
        }

        #region Input Methods

        /// <summary>
        /// Handles gravity shift preview input.
        /// </summary>
        /// <param name="ctx">Input action callback context.</param>
        private void OnGravityShiftPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            _gravityVector = new Vector3(input.x, 0, input.y);

            SetHologram();
            AlignHologram();
        }

        /// <summary>
        /// Applies the gravity shift when the input is performed.
        /// </summary>
        /// <param name="ctx">Input action callback context.</param>
        private void OnApplyGravityShiftPerformed(InputAction.CallbackContext ctx)
        {
            _gravityDirection = _hologramGravityDirection;

            transform.up = _hologramPlayer.transform.up;
            _hologramPlayer.SetActive(false);

            // Preserve motion in the new gravity direction.
            Vector3 projectedVelocity = Vector3.Project(_rb.velocity, _gravityDirection);
            _rb.velocity = projectedVelocity;

            // Apply new gravity.
            Physics.gravity = GRAVITY * -_gravityDirection;
        }

        #endregion
    }
}