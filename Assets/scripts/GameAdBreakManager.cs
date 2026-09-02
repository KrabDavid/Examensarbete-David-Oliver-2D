using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class GameAdBreakManager : MonoBehaviour
{
    [Header("Main Video")]
    public VideoPlayer mainVideoPlayer;
    public GameObject mainVideoDisplay;

    [Header("UI Elements")]
    public GameObject adOverlayPanel;
    public TextMeshProUGUI adTimerText;

    [Header("Environment Controls")]
    public SpriteRenderer counterRenderer; // Drag 'Counter BK' GameObject here

    [Header("Gameplay Objects")]
    public PlayerMovement2D playerMovement;
    public FoodSpawner foodSpawner;

    [Header("Stages Configuration")]
    public float timeUntilAdBreak = 10f; // Delay before the interactive ad triggers
    public float stageDuration = 15f;    // Duration per brand stage
    public AdStageData[] adStages;

    private void Start()
    {
        SetGameplayActive(false);
        DisableAllBackgrounds();

        if (adOverlayPanel != null) adOverlayPanel.SetActive(false);
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(true);
        if (mainVideoPlayer != null) mainVideoPlayer.Play();

        StartCoroutine(AdBreakRoutine());
    }

    private IEnumerator AdBreakRoutine()
    {
        // 1. Wait for configured break delay
        yield return new WaitForSeconds(timeUntilAdBreak);

        // 2. Pause & Hide Main Video
        if (mainVideoPlayer != null) mainVideoPlayer.Pause();
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(false);

        if (adOverlayPanel != null) adOverlayPanel.SetActive(true);
        SetGameplayActive(true);

        // Calculate total ad duration (e.g., 3 stages * 15s = 45s)
        float totalAdDuration = stageDuration * adStages.Length;
        float totalTimer = totalAdDuration;
        int currentStageIndex = -1;

        // 3. Single continuous countdown loop
        while (totalTimer > 0)
        {
            if (adTimerText != null)
            {
                adTimerText.text = "Ad ends in: " + Mathf.CeilToInt(totalTimer) + "s";
            }

            float elapsedTime = totalAdDuration - totalTimer;
            int targetStageIndex = Mathf.FloorToInt(elapsedTime / stageDuration);
            targetStageIndex = Mathf.Clamp(targetStageIndex, 0, adStages.Length - 1);

            // Swap stage whenever we hit a new interval
            if (targetStageIndex != currentStageIndex)
            {
                currentStageIndex = targetStageIndex;
                ApplyStage(adStages[currentStageIndex]);
            }

            totalTimer -= Time.deltaTime;
            yield return null;
        }

        // 4. End Ad & Restore Main Video
        SetGameplayActive(false);
        DisableAllBackgrounds();

        if (adOverlayPanel != null) adOverlayPanel.SetActive(false);
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(true);
        if (mainVideoPlayer != null) mainVideoPlayer.Play();
    }

    private void ApplyStage(AdStageData stage)
    {
        DisableAllBackgrounds();

        // 1. Activate stage background
        if (stage.backgroundObject != null)
        {
            stage.backgroundObject.SetActive(true);
        }

        // 2. Tint Counter BK color
        if (counterRenderer != null)
        {
            counterRenderer.color = stage.counterColor;
        }

        // 3. Swap Player Sprites
        if (playerMovement != null)
        {
            playerMovement.walkFrame1 = stage.playerWalkFrame1;
            playerMovement.walkFrame2 = stage.playerWalkFrame2;
        }

        // 4. Swap Prefabs
        if (foodSpawner != null)
        {
            foodSpawner.foodPrefabs = stage.stageItemPrefabs;
        }
    }

    private void DisableAllBackgrounds()
    {
        if (adStages == null) return;

        foreach (var stage in adStages)
        {
            if (stage.backgroundObject != null)
            {
                stage.backgroundObject.SetActive(false);
            }
        }
    }

    private void SetGameplayActive(bool active)
    {
        if (playerMovement != null) playerMovement.gameObject.SetActive(active);
        if (foodSpawner != null) foodSpawner.enabled = active;
    }
}