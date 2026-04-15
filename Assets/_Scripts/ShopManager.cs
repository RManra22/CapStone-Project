using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI creditsText;

    [Header("Spread Shot")]
    [SerializeField] private TextMeshProUGUI spreadStatusText;
    [SerializeField] private Button spreadBuyButton;
    [SerializeField] private Button spreadSelectButton;
    [SerializeField] private int spreadShotCost = 500;

    [Header("Burst Shot")]
    [SerializeField] private TextMeshProUGUI burstStatusText;
    [SerializeField] private Button burstBuyButton;
    [SerializeField] private Button burstSelectButton;
    [SerializeField] private int burstShotCost = 750;

    [Header("Selected Style Indicator")]
    [SerializeField] private TextMeshProUGUI selectedStyleText;

    private int totalCredits;
    private bool hasSpread;
    private bool hasBurst;

    private void Start() {
        LoadData();
        RefreshUI();
    }

    private void LoadData() {
        totalCredits = PlayerPrefs.GetInt("TotalCredits", 0);
        hasSpread = PlayerPrefs.GetInt("HasSpreadShot", 0) == 1;
        hasBurst = PlayerPrefs.GetInt("HasBurstShot", 0) == 1;
    }

    private void RefreshUI() {
        creditsText.text = "Credits: " + totalCredits;

        // Current selected style
        int selected = PlayerPrefs.GetInt("SelectedShootingStyle", 1);
        selectedStyleText.text = "Active: " + GetStyleName(selected);

        // Spread shot
        if (hasSpread) {
            spreadStatusText.text = "Owned";
            spreadBuyButton.interactable = false;
            spreadSelectButton.interactable = true;
        } else {
            spreadStatusText.text = spreadShotCost + " credits";
            spreadBuyButton.interactable = totalCredits >= spreadShotCost;
            spreadSelectButton.interactable = false;
        }

        // Burst shot
        if (hasBurst) {
            burstStatusText.text = "Owned";
            burstBuyButton.interactable = false;
            burstSelectButton.interactable = true;
        } else {
            burstStatusText.text = burstShotCost + " credits";
            burstBuyButton.interactable = totalCredits >= burstShotCost;
            burstSelectButton.interactable = false;
        }
    }

    public void BuySpreadShot() {
        if (hasSpread || totalCredits < spreadShotCost) return;
        totalCredits -= spreadShotCost;
        hasSpread = true;
        PlayerPrefs.SetInt("TotalCredits", totalCredits);
        PlayerPrefs.SetInt("HasSpreadShot", 1);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public void BuyBurstShot() {
        if (hasBurst || totalCredits < burstShotCost) return;
        totalCredits -= burstShotCost;
        hasBurst = true;
        PlayerPrefs.SetInt("TotalCredits", totalCredits);
        PlayerPrefs.SetInt("HasBurstShot", 1);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public void SelectStyle(int style) {
        PlayerPrefs.SetInt("SelectedShootingStyle", style);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    private string GetStyleName(int style) {
        switch (style) {
            case 1: return "Single Shot";
            case 2: return "Triple Spread";
            case 3: return "Burst Shot";
            default: return "Single Shot";
        }
    }
}