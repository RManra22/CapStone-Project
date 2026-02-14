using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance { get; private set; }
    [SerializeField] TextMeshProUGUI timerText;
    float elapsedTime;
    int minutes;
    int seconds;
    void Update()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Function that return the time when the player dies so it can be used in the Game Over screen
    // public string GetTime()
    // {
    //     return string.Format("{0:00}:{1:00}", minutes, seconds);
    // }
}
