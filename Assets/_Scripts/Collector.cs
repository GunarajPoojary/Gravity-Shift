using UnityEngine;
using UnityEngine.Events;

namespace GravityManipulationPuzzle
{
    /// <summary>
    /// Manages the collection of cubes in the game.
    /// </summary>
    public class Collector : MonoBehaviour
    {
        [SerializeField] private int _numberOfAvailableCubes = 10;

        private int _collectedCount = 0;

        [Space(10)]
        public UnityEvent OnCollectAllCubes;
        public UnityEvent<int, int> OnCollectedCountChanged;

        private void Start() => OnCollectedCountChanged?.Invoke(_collectedCount, _numberOfAvailableCubes);

        private void OnTriggerEnter(Collider other)
        {
            // Checks if the collided object has the "Collectable" tag.
            if (other.CompareTag("Collectable"))
            {
                _collectedCount++;

                OnCollectedCountChanged?.Invoke(_collectedCount, _numberOfAvailableCubes);

                other.gameObject.SetActive(false);

                // Checks if all cubes have been collected.
                if (_collectedCount >= _numberOfAvailableCubes)
                {
                    OnCollectAllCubes?.Invoke();
                }
            }
        }
    }
}