using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GravityManipulationPuzzle
{
    /// <summary>
    /// Manages game states such as game over conditions and displays related UI elements.
    /// </summary>
    [DefaultExecutionOrder(-2)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Text _gameOverText;

        [SerializeField] private CountdownTimer _timer;
        [SerializeField] private float _waitTimeAfterGameOver = 5f;

        private void Awake()
        {
            Instance = this;

            if (_gameOverText == null)
            {
                Debug.LogWarning("GameOverText is not assigned in the inspector.", this);
                return;
            }

            _gameOverText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        public void GameOver(string message)
        {
            _gameOverText.gameObject.SetActive(true);
            _gameOverText.text = message;

            StartCoroutine(QuitGame());
        }

#if UNITY_EDITOR
        private IEnumerator QuitGame()
        {
            yield return new WaitForSeconds(_waitTimeAfterGameOver);

            Application.Quit();
        }
#else
        private IEnumerator QuitGame()
        {
            yield return new WaitForSeconds(_waitTimeAfterGameOver);

            Application.Quit();
        }
#endif

        public void StopTimer() => _timer.StopTimer();
    }
}