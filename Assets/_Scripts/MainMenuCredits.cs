using UnityEngine;
using TMPro;

public class MainMenuCredits : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI totalCreditsText;

    private void Start() {
        int totalCredits = PlayerPrefs.GetInt("TotalCredits", 0);
        totalCreditsText.text = "Credits: " + totalCredits;
    }
}