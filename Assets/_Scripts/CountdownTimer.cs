using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GravityManipulationPuzzle
{
    /// <summary>
    /// Manages the game timer and handles end-of-time events.
    /// </summary>
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private int _initialTime = 10; // In seconds

        private int _remainingTime;
        private Coroutine _countdownCoroutine;
        private bool _isPaused = false;

        public bool IsRunning => _countdownCoroutine != null;
        public bool IsPaused => _isPaused;
        public int RemainingTime => _remainingTime;

        [Space(20)]
        public UnityEvent<int> OnTimerUpdated; // Event for UI updates
        public UnityEvent OnTimerCompleted; // Event when timer ends

        private void Start()
        {
            StartTimer(_initialTime);
        }

        /// <summary>
        /// Starts the countdown timer.
        /// </summary>
        /// <param name="seconds">Duration in seconds</param>
        public void StartTimer(int seconds)
        {
            if (seconds <= 0) return;

            _remainingTime = seconds;
            _isPaused = false;

            if (_countdownCoroutine != null)
                StopCoroutine(_countdownCoroutine);

            _countdownCoroutine = StartCoroutine(TimerCoroutine());
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        public void PauseTimer()
        {
            if (IsRunning && !_isPaused)
            {
                _isPaused = true;
            }
        }

        /// <summary>
        /// Resumes the timer.
        /// </summary>
        public void ResumeTimer()
        {
            if (IsRunning && _isPaused)
            {
                _isPaused = false;
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
            _remainingTime = 0;
            OnTimerUpdated?.Invoke(0);
        }

        private IEnumerator TimerCoroutine()
        {
            while (_remainingTime > 0)
            {
                if (!_isPaused)
                {
                    yield return new WaitForSeconds(1);
                    _remainingTime--;
                    OnTimerUpdated?.Invoke(_remainingTime);
                }
                else
                {
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