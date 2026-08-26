using UnityEngine;
using TMPro;

public class AdGameScoreManager : MonoBehaviour
{
    public static AdGameScoreManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text streakText;

    [Header("Score Values")]
    public int burgerPoints = 100;
    public int friesPoints = 50;

    private int currentScore = 0;
    private int currentStreak = 0;
    private int highestStreak = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddPoints(FallingFoodItem.FoodType foodType)
    {
        currentStreak++;
        if (currentStreak > highestStreak) highestStreak = currentStreak;

        // Calculate score with a multiplier based on current streak
        int basePoints = (foodType == FallingFoodItem.FoodType.Burger) ? burgerPoints : friesPoints;
        int multiplier = 1 + (currentStreak / 5); // Multiplier increases every 5 streak points
        int finalPoints = basePoints * multiplier;

        currentScore += finalPoints;
        UpdateUI();
    }

    public void ResetStreak()
    {
        currentStreak = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {currentScore}";
        if (streakText != null)
        {
            if (currentStreak > 1)
                streakText.text = $"Streak: {currentStreak}x!";
            else
                streakText.text = "Streak: 0";
        }
    }
}