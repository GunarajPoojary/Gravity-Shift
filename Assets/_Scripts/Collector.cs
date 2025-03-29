using System;
using UnityEngine;
using UnityEngine.Events;

namespace GravityManipulationPuzzle
{
    /// <summary>
    /// Manages the collection of items in the game.
    /// </summary>
    public class Collector : MonoBehaviour
    {
        [SerializeField] private int _numberOfAvailableCubes = 10;

        private int _collectedCount = 0;

        [Space(10)]
        public UnityEvent OnCollectAllCubes;
        public UnityEvent<int, int> OnCollectedCountChanged; // Event for UI updates

        private void Start() => OnCollectedCountChanged?.Invoke(_collectedCount, _numberOfAvailableCubes); // Initialize UI

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Collectable"))
            {
                _collectedCount++;

                // Fire the event when the count updates
                OnCollectedCountChanged?.Invoke(_collectedCount, _numberOfAvailableCubes);

                other.gameObject.SetActive(false);

                if (_collectedCount >= _numberOfAvailableCubes)
                {
                    OnCollectAllCubes?.Invoke();
                }
            }
        }
    }
}