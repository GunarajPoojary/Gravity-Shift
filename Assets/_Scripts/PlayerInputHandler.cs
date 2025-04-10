using GravityManipulationPuzzle.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityManipulationPuzzle
{
    public interface IPlayerInputHandler
    {
        InputAction MoveAction { get; }
        InputAction JumpAction { get; }
        InputAction LookAction { get; }
        InputAction GravityShiftPreviewAction { get; }
        InputAction ShiftGravityAction { get; }
    }

    /// <summary>
    /// Manages player input actions and provides access to input events.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class PlayerInputHandler : MonoBehaviour, IPlayerInputHandler
    {
        private PlayerInputActions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _lookAction;
        private InputAction _gravityShiftPreviewAction;
        private InputAction _shiftGravityAction;

        public InputAction MoveAction => _moveAction;
        public InputAction JumpAction => _jumpAction;
        public InputAction LookAction => _lookAction;
        public InputAction GravityShiftPreviewAction => _gravityShiftPreviewAction;
        public InputAction ShiftGravityAction => _shiftGravityAction;

        private void Awake()
        {
            _inputActions = new PlayerInputActions(); // Instantiates the input actions class.

            PlayerInputActions.PlayerActions PlayerActions = _inputActions.Player; // Stores the player input actions.

            // Assigning specific input actions to their corresponding variables.
            _moveAction = PlayerActions.Movement;
            _jumpAction = PlayerActions.Jump;
            _lookAction = PlayerActions.Look;
            _gravityShiftPreviewAction = PlayerActions.GravityShiftPreview;
            _shiftGravityAction = PlayerActions.ShiftGravity;
        }

        private void OnEnable() => _inputActions.Enable();

        private void OnDisable() => _inputActions.Disable();
    }
}