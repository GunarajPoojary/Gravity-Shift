using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GravityManipulationPuzzle
{
    public class CountdownTimer : MonoBehaviour
    {
        [Tooltip("Initial countdown time in seconds")]
        [SerializeField] private int _initialTime = 10;

        private int _remainingTime;

        private Coroutine _countdownCoroutine;

        private bool _isPaused = false;

        public bool IsRunning => _countdownCoroutine != null;

        public bool IsPaused => _isPaused;

        public int RemainingTime => _remainingTime;

        [Space(20)]
        public UnityEvent<int> OnTimerUpdated;

        public UnityEvent OnTimerCompleted;

        private void Start()
        {
            StartTimer(_initialTime);
        }

        /// <summary>
        /// Starts the countdown timer with the specified duration.
        /// </summary>
        /// <param name="seconds">Duration of the timer in seconds.</param>
        public void StartTimer(int seconds)
        {
            // Prevent starting the timer if the duration is invalid
            if (seconds <= 0) return;

            _remainingTime = seconds;
            _isPaused = false;

            if (_countdownCoroutine != null)
                StopCoroutine(_countdownCoroutine);

            _countdownCoroutine = StartCoroutine(TimerCoroutine());
        }

        /// <summary>
        /// Stops the timer and resets the remaining time.
        /// </summary>
        public void StopTimer()
        {
            // If the countdown coroutine is active, stop it
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            // Reset the remaining time and update any UI listeners
            _remainingTime = 0;
            OnTimerUpdated?.Invoke(0);
        }

        /// <summary>
        /// Handles the countdown logic, decreasing the time every second.
        /// </summary>
        private IEnumerator TimerCoroutine()
        {
            while (_remainingTime > 0)
            {
                // Only decrement time if the timer is not paused
                if (!_isPaused)
                {
                    yield return new WaitForSeconds(1);
                    _remainingTime--;

                    // Notify listeners (e.g., UI) about the updated time
                    OnTimerUpdated?.Invoke(_remainingTime);
                }
                else
                {
                    // If paused, wait until resumed
                    yield return null;
                }
            }

            _remainingTime = 0;
            _countdownCoroutine = null;

            OnTimerUpdated?.Invoke(0);
            OnTimerCompleted?.Invoke();
        }
    }
}