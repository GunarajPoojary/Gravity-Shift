using UnityEngine;

namespace GravityManipulationPuzzle
{
    public class FreeFallDetector : MonoBehaviour
    {
        [SerializeField] private float _checkRadius = 20f; // Radius of the sphere used to check for ground presence

        [SerializeField] private LayerMask _groundLayer;

        [SerializeField] private float _groundCheckDistance = 5f; 
        private bool _isNearGround;

        private void Update()
        {
            CheckGround();
        }

        private void CheckGround()
        {
            Vector3 start = transform.position;

            // Perform a CheckSphere at the character's position to detect ground within the radius
            bool isGrounded = Physics.CheckSphere(start, _checkRadius, _groundLayer);

            // If the character was previously near the ground but now isn't, trigger Game Over
            if (!isGrounded && _isNearGround) 
            {
                // Call the GameOver function from GameManager with a message
                GameManager.Instance.GameOver("Game Over! Free Fall");
            }

            // Update the ground status for the next frame
            _isNearGround = isGrounded;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isNearGround ? Color.green : Color.red;

            Gizmos.DrawWireSphere(transform.position, _checkRadius);
        }
    }
}