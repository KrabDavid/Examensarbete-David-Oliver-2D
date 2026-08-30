using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class GameAdBreakManager : MonoBehaviour
{
    [Header("Main Video")]
    public VideoPlayer mainVideoPlayer;
    public GameObject mainVideoDisplay; // The RawImage or Screen object showing the main video

    [Header("UI Elements")]
    public GameObject adOverlayPanel;   // UI containing the timer text or ad UI frame
    public TextMeshProUGUI adTimerText;

    [Header("Gameplay Controls")]
    public GameObject playerObject;     // e.g., Player
    public GameObject foodSpawner;      // e.g., _FoodSpawner

    [Header("Settings")]
    public float timeUntilAdBreak = 10f; // Time in seconds before ad triggers
    public float adDuration = 45f;       // Total minigame duration

    private void Start()
    {
        // 1. Hide gameplay elements & ad UI at start
        SetGameplayActive(false);
        if (adOverlayPanel != null) adOverlayPanel.SetActive(false);

        // 2. Ensure main video display is active and playing
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(true);
        if (mainVideoPlayer != null) mainVideoPlayer.Play();

        // 3. Start timer to trigger the interactive ad break
        StartCoroutine(AdBreakRoutine());
    }

    private IEnumerator AdBreakRoutine()
    {
        // Wait until it's time for the ad break
        yield return new WaitForSeconds(timeUntilAdBreak);

        // --- STEP 1: Pause & Hide Main Video ---
        if (mainVideoPlayer != null)
        {
            mainVideoPlayer.Pause();
        }

        // Hide the main video screen so the game underneath is revealed
        if (mainVideoDisplay != null)
        {
            mainVideoDisplay.SetActive(false);
        }

        // --- STEP 2: Start Interactive Game Ad ---
        if (adOverlayPanel != null)
        {
            adOverlayPanel.SetActive(true);
        }

        SetGameplayActive(true);

        // Run the minigame timer
        float timer = adDuration;
        while (timer > 0)
        {
            if (adTimerText != null)
            {
                adTimerText.text = "Ad ends in: " + Mathf.CeilToInt(timer) + "s";
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        // --- STEP 3: End Game Ad & Restore Main Video ---
        SetGameplayActive(false);

        if (adOverlayPanel != null)
        {
            adOverlayPanel.SetActive(false);
        }

        // Show and Resume Main Video
        if (mainVideoDisplay != null)
        {
            mainVideoDisplay.SetActive(true);
        }

        if (mainVideoPlayer != null)
        {
            mainVideoPlayer.Play();
        }
    }

    private void SetGameplayActive(bool active)
    {
        if (playerObject != null) playerObject.SetActive(active);
        if (foodSpawner != null) foodSpawner.SetActive(active);
    }
}