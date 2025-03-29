using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Text _timerText;

    public void UpdateUI(int remainingTime)
    {
        int minutes = remainingTime / 60;
        int seconds = remainingTime % 60;
        _timerText.text = $"{minutes:00}:{seconds:00}";
    }
}