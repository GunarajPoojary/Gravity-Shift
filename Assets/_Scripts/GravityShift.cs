using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityManipulationPuzzle
{
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
        [SerializeField][Range(1f, 3f)] private float _gravityMultiplier = 1f;

        private IPlayerInputHandler _playerInput;
        private Rigidbody _rb;
        private Transform _cam;

        private static readonly Vector3[] _worldAxes = new Vector3[]
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        private Vector3 _gravityVector; // Vector representing the direction of the desired gravity shift
        private Vector3 _previousPosition; // Stores the previous position of the player
        private Quaternion _previousRotation; // Stores the previous position of the player
        private Vector3 _hologramGravityDirection;

        private const float GRAVITY = -9.81f;
        private Vector3 _gravityDirection;

        public Vector3 GravityDirection => _gravityDirection;

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

        private void InitializeComponents()
        {
            _playerInput = GetComponent<IPlayerInputHandler>();
            _rb = GetComponent<Rigidbody>();
            _cam = Camera.main.transform;
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
        }

        // Update the position of the hologram to match the player's position
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

        // Activate and position the hologram at the player's current location
        private void SetHologram()
        {
            _hologramPlayer.SetActive(true);
            _hologramPlayer.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        // Align the hologram based on the player's input direction and camera orientation
        private void AlignHologram()
        {
            Vector3 gravityUp = -Physics.gravity.normalized;
            Vector3 projectedForward = Vector3.ProjectOnPlane(_cam.forward, gravityUp).normalized; // Project camera's forward direction onto the plane perpendicular to gravity

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

        // Aligns the hologram's Y-axis to the closest world axis based on the input direction
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

            _hologramPlayer.transform.rotation = Quaternion.FromToRotation(_hologramPlayer.transform.up, -closestWorldAxis) * _hologramPlayer.transform.rotation;
        }

        #region Input Methods
        private void OnGravityShiftPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            _gravityVector = new Vector3(input.x, 0, input.y); // Convert the input to a vector representing the gravity shift direction

            SetHologram(); // Activate the hologram and position it correctly
            AlignHologram(); // Align the hologram based on the camera direction and gravity vector
        }

        private void OnApplyGravityShiftPerformed(InputAction.CallbackContext ctx)
        {
            _gravityDirection = _hologramGravityDirection; // Set the new gravity direction based on the hologram's alignment

            transform.up = _hologramPlayer.transform.up;

            _hologramPlayer.SetActive(false);

            // Project the player's current velocity onto the new gravity direction
            Vector3 projectedVelocity = Vector3.Project(_rb.velocity, _gravityDirection);

            // Apply the projected velocity to maintain consistent motion with the new gravity direction
            _rb.velocity = projectedVelocity;

            Physics.gravity = _gravityMultiplier * GRAVITY * -_gravityDirection;
        }
        #endregion
    }
}