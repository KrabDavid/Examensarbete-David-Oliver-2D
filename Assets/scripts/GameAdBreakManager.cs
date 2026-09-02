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

    [Header("Gameplay Objects")]
    public PlayerMovement2D playerMovement;
    public FoodSpawner foodSpawner;

    [Header("Stages Configuration")]
    public float stageDuration = 15f; // 15 seconds per stage
    public AdStageData[] adStages;    // Element 0: McD, Element 1: Nike, Element 2: Elgiganten

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
        // 1. Wait until ad break triggers
        yield return new WaitForSeconds(10f);

        // 2. Pause & Hide Main Video
        if (mainVideoPlayer != null) mainVideoPlayer.Pause();
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(false);

        if (adOverlayPanel != null) adOverlayPanel.SetActive(true);
        SetGameplayActive(true);

        // Calculate total ad duration (3 stages * 15s = 45s)
        float totalAdDuration = stageDuration * adStages.Length;
        float totalTimer = totalAdDuration;
        int currentStageIndex = -1;

        // 3. Single 45-second Countdown Loop
        while (totalTimer > 0)
        {
            if (adTimerText != null)
            {
                adTimerText.text = "Ad ends in: " + Mathf.CeilToInt(totalTimer) + "s";
            }

            float elapsedTime = totalAdDuration - totalTimer;
            int targetStageIndex = Mathf.FloorToInt(elapsedTime / stageDuration);
            targetStageIndex = Mathf.Clamp(targetStageIndex, 0, adStages.Length - 1);

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
        // First disable all stage backgrounds to ensure no overlap
        DisableAllBackgrounds();

        // Turn on the specific background for this stage
        if (stage.backgroundObject != null)
        {
            stage.backgroundObject.SetActive(true);
        }

        // Swap Player Animation Sprites
        if (playerMovement != null)
        {
            playerMovement.walkFrame1 = stage.playerWalkFrame1;
            playerMovement.walkFrame2 = stage.playerWalkFrame2;
        }

        // Swap Spawned Items
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