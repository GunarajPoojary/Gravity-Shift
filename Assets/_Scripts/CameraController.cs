using Cinemachine;
using UnityEngine;

namespace GravityManipulationPuzzle
{
    /// <summary>
    /// Controls the camera alignment to match the player's gravity direction.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        private CinemachineVirtualCamera _virtualCamera;

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        private void LateUpdate()
        {
            AlignCameraWithPlayerGravity();
        }

        // Aligns the camera to match the player's gravity direction.
        private void AlignCameraWithPlayerGravity()
        {
            if (_player == null) return;

            Vector3 gravityUp = -_player.up.normalized;
            Vector3 cameraForward = Vector3.ProjectOnPlane(_player.forward, gravityUp).normalized;

            if (cameraForward == Vector3.zero) return;

            _virtualCamera.transform.rotation = Quaternion.LookRotation(cameraForward, gravityUp);
        }
    }
}