using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class GameAdBreakManager : MonoBehaviour
{
    [Header("Main Video")]
    public VideoPlayer mainVideoPlayer;
    public GameObject mainVideoDisplay;

    [Header("Audio Settings")]
    public AudioSource adAudioSource;           // Drag an AudioSource component here
    public AudioClip adMusicClip;               // Drag your imported music track here
    public float targetMusicVolume = 0.5f;      // Max volume during the ad
    public float fadeDuration = 1.5f;           // Fade-in / Fade-out time in seconds

    [Header("UI Elements")]
    public GameObject adOverlayPanel;
    public TextMeshProUGUI adTimerText;

    [Header("End Screen Overlay")]
    public GameObject endScreenPanel;
    public TextMeshProUGUI rewardText;
    public Animator endScreenAnimator;
    public float endScreenDisplayDuration = 4f;

    [Header("Environment Controls")]
    public SpriteRenderer counterRenderer;

    [Header("Gameplay Objects")]
    public PlayerMovement2D playerMovement;
    public FoodSpawner foodSpawner;
    public TextMeshProUGUI gameScoreText;

    [Header("Stages Configuration")]
    public float timeUntilAdBreak = 10f;
    public float stageDuration = 15f;
    public AdStageData[] adStages;

    private void Start()
    {
        SetGameplayActive(false);
        DisableAllBackgrounds();

        if (adAudioSource != null)
        {
            adAudioSource.playOnAwake = false;
            adAudioSource.volume = 0f;
        }

        if (adOverlayPanel != null) adOverlayPanel.SetActive(false);
        if (endScreenPanel != null) endScreenPanel.SetActive(false);
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(true);
        if (mainVideoPlayer != null) mainVideoPlayer.Play();

        StartCoroutine(AdBreakRoutine());
    }

    private IEnumerator AdBreakRoutine()
    {
        // 1. Wait for break delay
        yield return new WaitForSeconds(timeUntilAdBreak);

        // 2. Pause Main Video & Start Minigame
        if (mainVideoPlayer != null) mainVideoPlayer.Pause();
        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(false);

        if (adOverlayPanel != null) adOverlayPanel.SetActive(true);
        SetGameplayActive(true);

        // Start background music with a smooth fade-in
        if (adAudioSource != null && adMusicClip != null)
        {
            adAudioSource.clip = adMusicClip;
            adAudioSource.Play();
            StartCoroutine(FadeAudio(adAudioSource, fadeDuration, targetMusicVolume));
        }

        float totalAdDuration = stageDuration * adStages.Length;
        float totalTimer = totalAdDuration;
        int currentStageIndex = -1;

        // 3. 45-Second Stage Loop
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

        // 4. Pause Gameplay & Fade Audio Out during End Screen
        SetGameplayActive(false);

        if (adAudioSource != null)
        {
            StartCoroutine(FadeAudio(adAudioSource, fadeDuration, 0f));
        }

        yield return StartCoroutine(ShowEndScreenSequence());

        // 5. Restore Main Video
        DisableAllBackgrounds();

        if (adAudioSource != null && adAudioSource.isPlaying)
        {
            adAudioSource.Stop();
        }

        if (adOverlayPanel != null) adOverlayPanel.SetActive(false);
        if (endScreenPanel != null) endScreenPanel.SetActive(false);

        if (mainVideoDisplay != null) mainVideoDisplay.SetActive(true);
        if (mainVideoPlayer != null) mainVideoPlayer.Play();
    }

    private IEnumerator FadeAudio(AudioSource source, float duration, float targetVolume)
    {
        float startVolume = source.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator ShowEndScreenSequence()
    {
        if (endScreenPanel != null)
        {
            string capturedScore = "0";
            if (gameScoreText != null)
            {
                capturedScore = System.Text.RegularExpressions.Regex.Match(gameScoreText.text, @"\d+").Value;
                if (string.IsNullOrEmpty(capturedScore)) capturedScore = gameScoreText.text;
            }

            if (rewardText != null)
            {
                rewardText.text = $"BRA JOBBAT !\nScore: {capturedScore}\nDU VANN 10% RABATT!";
            }

            endScreenPanel.SetActive(true);

            if (endScreenAnimator != null)
            {
                endScreenAnimator.SetTrigger("Show");
            }

            yield return new WaitForSeconds(endScreenDisplayDuration);
        }
    }

    private void ApplyStage(AdStageData stage)
    {
        DisableAllBackgrounds();

        if (stage.backgroundObject != null) stage.backgroundObject.SetActive(true);
        if (counterRenderer != null) counterRenderer.color = stage.counterColor;

        if (playerMovement != null)
        {
            playerMovement.walkFrame1 = stage.playerWalkFrame1;
            playerMovement.walkFrame2 = stage.playerWalkFrame2;
        }

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