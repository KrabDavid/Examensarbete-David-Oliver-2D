using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using TMPro;

public class AdBreakManager : MonoBehaviour
{
    [System.Serializable]
    public class AdCategory
    {
        public string categoryName;
        public VideoClip[] categoryClips;
    }

    [Header("Video Players")]
    public VideoPlayer mainVideoPlayer;
    public VideoPlayer adVideoPlayer;

    [Header("UI Elements")]
    public GameObject adOverlayPanel;
    public TMP_Text adTimerText;

    [Header("Pop-Up UI Elements")]
    public GameObject categoryPopUpPanel;
    public TMP_Text popUpHeader;
    public TMP_Text optionAText;
    public TMP_Text optionBText;
    public TMP_Text optionCText;
    public TMP_Text optionDText;

    [Header("Settings")]
    public float timeUntilAdBreak = 10f; // Seconds before main video is interrupted
    public float adDuration = 45f;        // Total duration of ad break
    public float popUpDuration = 10f;     // Seconds user has to choose after pressing E

    [Header("Guaranteed Ads")]
    public VideoClip mcdonaldsAdClip;    // Drag McDonald's ad clip here (1st Ad)
    public VideoClip elgigantenAdClip;   // Drag Elgiganten ad clip here (2nd Ad)

    [Header("Ad Pool Settings")]
    public List<AdCategory> adCategories = new List<AdCategory>(); // Travel, Pets, Interior, Training
    public VideoClip[] uncategorizedAdClips; // General ads not tied to any category

    private bool adHasBeenTriggered = false;
    private bool isAdRunning = false;
    private float adTimer;
    private Coroutine adLoopCoroutine;
    private bool skipRequested = false;

    private int currentAdIndex = 0; // Tracks ad sequence (1 = McD, 2 = Elgiganten, 3+ = Random/Selected)

    // Pop-Up state tracking
    private bool isPopUpActive = false;
    private float popUpTimer;
    private AdCategory activeSelectedCategory = null;

    // Played tracking
    private List<VideoClip> playedAdClips = new List<VideoClip>();

    void Start()
    {
        adOverlayPanel.SetActive(false);
        if (categoryPopUpPanel != null) categoryPopUpPanel.SetActive(false);

        mainVideoPlayer.Play();
        adVideoPlayer.Stop();
    }

    void Update()
    {
        // 1. Trigger the main ad break after set time
        if (!adHasBeenTriggered && mainVideoPlayer.time >= timeUntilAdBreak)
        {
            StartAdBreak();
        }

        // 2. Handle overall 45s ad break countdown & Key Inputs
        if (isAdRunning)
        {
            adTimer -= Time.deltaTime;

            if (adTimerText != null)
            {
                adTimerText.text = "Ad ends in: " + Mathf.CeilToInt(adTimer) + "s";
            }

            if (Keyboard.current != null)
            {
                // Spacebar skip check (only when menu isn't active)
                if (Keyboard.current.spaceKey.wasPressedThisFrame && !isPopUpActive)
                {
                    skipRequested = true;
                }

                // Press 'E' to toggle Open Pop-up Menu
                if (Keyboard.current.eKey.wasPressedThisFrame && !isPopUpActive)
                {
                    OpenPopUp();
                }

                // Handle choices A, B, C, or D while Pop-Up is open
                if (isPopUpActive)
                {
                    popUpTimer -= Time.deltaTime;
                    if (popUpHeader != null)
                    {
                        popUpHeader.text = $"Choose Category ({Mathf.CeilToInt(popUpTimer)}s):";
                    }

                    if (Keyboard.current.aKey.wasPressedThisFrame && adCategories.Count > 0)
                    {
                        SelectCategory(adCategories[0]);
                    }
                    else if (Keyboard.current.bKey.wasPressedThisFrame && adCategories.Count > 1)
                    {
                        SelectCategory(adCategories[1]);
                    }
                    else if (Keyboard.current.cKey.wasPressedThisFrame && adCategories.Count > 2)
                    {
                        SelectCategory(adCategories[2]);
                    }
                    else if (Keyboard.current.dKey.wasPressedThisFrame && adCategories.Count > 3)
                    {
                        SelectCategory(adCategories[3]);
                    }

                    // Auto-close pop-up if timer reaches 0
                    if (popUpTimer <= 0f)
                    {
                        ClosePopUp();
                    }
                }
            }

            // End overall ad break when 45s duration finishes
            if (adTimer <= 0f)
            {
                EndAdBreak();
            }
        }
    }

    public void StartAdBreak()
    {
        adHasBeenTriggered = true;
        isAdRunning = true;
        adTimer = adDuration;
        activeSelectedCategory = null; // Reset category filter on new break
        currentAdIndex = 0;           // Reset sequence index for this break

        mainVideoPlayer.Pause();
        adOverlayPanel.SetActive(true);

        if (adLoopCoroutine != null) StopCoroutine(adLoopCoroutine);
        adLoopCoroutine = StartCoroutine(PlayAdSequence());
    }

    private IEnumerator PlayAdSequence()
    {
        while (isAdRunning)
        {
            skipRequested = false;
            currentAdIndex++;
            VideoClip selectedClip = null;

            // 1. Always force McDonald's ad as 1st clip
            if (currentAdIndex == 1)
            {
                selectedClip = mcdonaldsAdClip;
            }
            // 2. Always force Elgiganten ad as 2nd clip
            else if (currentAdIndex == 2)
            {
                selectedClip = elgigantenAdClip;
            }
            // 3. 3rd+ Ad: If a specific category is chosen by user
            else if (activeSelectedCategory != null)
            {
                selectedClip = GetUnplayedAdFromCategory(activeSelectedCategory);

                // If ALL clips in this category were played, reset lock to all sources
                if (selectedClip == null)
                {
                    Debug.Log($"All ads in '{activeSelectedCategory.categoryName}' completed! Reverting back to random selection.");
                    activeSelectedCategory = null;
                    selectedClip = GetUnplayedAdFromAllSources();
                }
            }
            // 4. 3rd+ Ad: Fallback / Default random selection
            else
            {
                selectedClip = GetUnplayedAdFromAllSources();
            }

            // Track played clip
            if (selectedClip != null && !playedAdClips.Contains(selectedClip))
            {
                playedAdClips.Add(selectedClip);
            }

            // Prepare & Play
            if (selectedClip != null)
            {
                adVideoPlayer.clip = selectedClip;
                adVideoPlayer.Prepare();
                while (!adVideoPlayer.isPrepared)
                {
                    if (!isAdRunning) yield break;
                    yield return null;
                }

                adVideoPlayer.Play();
                yield return null;
            }

            // Wait until clip ends, spacebar skip occurs, or ad duration finishes
            while (adVideoPlayer.isPlaying && isAdRunning && !skipRequested)
            {
                yield return null;
            }

            adVideoPlayer.Stop();
            mainVideoPlayer.Pause();
        }
    }

    private void OpenPopUp()
    {
        if (categoryPopUpPanel == null) return;

        isPopUpActive = true;
        popUpTimer = popUpDuration;

        // Populate UI Text labels for A, B, C, and D based on adCategories list order
        if (optionAText != null && adCategories.Count > 0) optionAText.text = "[A] " + adCategories[0].categoryName;
        if (optionBText != null && adCategories.Count > 1) optionBText.text = "[B] " + adCategories[1].categoryName;
        if (optionCText != null && adCategories.Count > 2) optionCText.text = "[C] " + adCategories[2].categoryName;
        if (optionDText != null && adCategories.Count > 3) optionDText.text = "[D] " + adCategories[3].categoryName;

        categoryPopUpPanel.SetActive(true);
    }

    private void SelectCategory(AdCategory chosenCategory)
    {
        activeSelectedCategory = chosenCategory;
        skipRequested = true; // Instantly switch to chosen category's ad clip
        ClosePopUp();
    }

    private void ClosePopUp()
    {
        isPopUpActive = false;
        if (categoryPopUpPanel != null) categoryPopUpPanel.SetActive(false);
    }

    private VideoClip GetUnplayedAdFromCategory(AdCategory category)
    {
        List<VideoClip> unplayed = new List<VideoClip>();
        foreach (var clip in category.categoryClips)
        {
            if (clip != null &&
                !playedAdClips.Contains(clip) &&
                clip != mcdonaldsAdClip &&
                clip != elgigantenAdClip)
            {
                unplayed.Add(clip);
            }
        }

        if (unplayed.Count == 0) return null; // Category exhausted
        return unplayed[Random.Range(0, unplayed.Count)];
    }

    private VideoClip GetUnplayedAdFromAllSources()
    {
        List<VideoClip> unplayedPool = new List<VideoClip>();

        // Add unplayed clips from all categories
        foreach (var cat in adCategories)
        {
            if (cat.categoryClips != null)
            {
                foreach (var clip in cat.categoryClips)
                {
                    if (clip != null &&
                        !playedAdClips.Contains(clip) &&
                        clip != mcdonaldsAdClip &&
                        clip != elgigantenAdClip)
                    {
                        unplayedPool.Add(clip);
                    }
                }
            }
        }

        // Add unplayed clips from uncategorized pool
        if (uncategorizedAdClips != null)
        {
            foreach (var clip in uncategorizedAdClips)
            {
                if (clip != null &&
                    !playedAdClips.Contains(clip) &&
                    clip != mcdonaldsAdClip &&
                    clip != elgigantenAdClip)
                {
                    unplayedPool.Add(clip);
                }
            }
        }

        // Reset tracking if every single pool ad has been shown
        if (unplayedPool.Count == 0)
        {
            playedAdClips.Clear();

            // Re-populate unplayed list excluding McD and Elgiganten
            foreach (var cat in adCategories)
            {
                if (cat.categoryClips != null)
                {
                    foreach (var clip in cat.categoryClips)
                    {
                        if (clip != null && clip != mcdonaldsAdClip && clip != elgigantenAdClip)
                        {
                            unplayedPool.Add(clip);
                        }
                    }
                }
            }
            if (uncategorizedAdClips != null)
            {
                foreach (var clip in uncategorizedAdClips)
                {
                    if (clip != null && clip != mcdonaldsAdClip && clip != elgigantenAdClip)
                    {
                        unplayedPool.Add(clip);
                    }
                }
            }

            if (unplayedPool.Count == 0) return null;
        }

        return unplayedPool[Random.Range(0, unplayedPool.Count)];
    }

    public void EndAdBreak()
    {
        isAdRunning = false;
        ClosePopUp();

        if (adLoopCoroutine != null) StopCoroutine(adLoopCoroutine);

        adVideoPlayer.Stop();
        adOverlayPanel.SetActive(false);
        mainVideoPlayer.Play();
    }
}