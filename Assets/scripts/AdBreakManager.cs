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
    public float timeUntilAdBreak = 10f;
    public float adDuration = 45f;
    public float popUpDuration = 10f;

    [Header("Guaranteed Ads")]
    public VideoClip mcdonaldsAdClip;   // 1st Ad
    public VideoClip elgigantenAdClip;  // 2nd Ad
    public VideoClip nikeAdClip;        // 3rd Ad

    [Header("Ad Pool Settings")]
    public List<AdCategory> adCategories = new List<AdCategory>();
    public VideoClip[] uncategorizedAdClips;

    private bool adHasBeenTriggered = false;
    private bool isAdRunning = false;
    private float adTimer;
    private Coroutine adLoopCoroutine;
    private bool skipRequested = false;

    private int currentAdIndex = 0;

    private bool isPopUpActive = false;
    private float popUpTimer;
    private AdCategory activeSelectedCategory = null;

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
        if (!adHasBeenTriggered && mainVideoPlayer.time >= timeUntilAdBreak)
        {
            StartAdBreak();
        }

        if (isAdRunning)
        {
            adTimer -= Time.deltaTime;

            if (adTimerText != null)
            {
                adTimerText.text = "Ad ends in: " + Mathf.CeilToInt(adTimer) + "s";
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame && !isPopUpActive)
                {
                    skipRequested = true;
                }

                if (Keyboard.current.eKey.wasPressedThisFrame && !isPopUpActive)
                {
                    OpenPopUp();
                }

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

                    if (popUpTimer <= 0f)
                    {
                        ClosePopUp();
                    }
                }
            }

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
        activeSelectedCategory = null;
        currentAdIndex = 0;

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

            // 1. Force McDonald's as 1st clip
            if (currentAdIndex == 1)
            {
                selectedClip = mcdonaldsAdClip;
            }
            // 2. Force Elgiganten as 2nd clip
            else if (currentAdIndex == 2)
            {
                selectedClip = elgigantenAdClip;
            }
            // 3. Force Nike as 3rd clip
            else if (currentAdIndex == 3)
            {
                selectedClip = nikeAdClip;
            }
            // 4. 4th+ Ad: User-selected category active
            else if (activeSelectedCategory != null)
            {
                selectedClip = GetUnplayedAdFromCategory(activeSelectedCategory);

                if (selectedClip == null)
                {
                    Debug.Log($"All ads in '{activeSelectedCategory.categoryName}' completed! Reverting back to random selection.");
                    activeSelectedCategory = null;
                    selectedClip = GetUnplayedAdFromAllSources();
                }
            }
            // 5. 4th+ Ad: Fallback random selection
            else
            {
                selectedClip = GetUnplayedAdFromAllSources();
            }

            if (selectedClip != null && !playedAdClips.Contains(selectedClip))
            {
                playedAdClips.Add(selectedClip);
            }

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOpenClick();
        }

        isPopUpActive = true;
        popUpTimer = popUpDuration;

        if (optionAText != null && adCategories.Count > 0) optionAText.text = "[A] " + adCategories[0].categoryName;
        if (optionBText != null && adCategories.Count > 1) optionBText.text = "[B] " + adCategories[1].categoryName;
        if (optionCText != null && adCategories.Count > 2) optionCText.text = "[C] " + adCategories[2].categoryName;
        if (optionDText != null && adCategories.Count > 3) optionDText.text = "[D] " + adCategories[3].categoryName;

        categoryPopUpPanel.SetActive(true);
    }

    private void SelectCategory(AdCategory chosenCategory)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayConfirmClick();
        }

        activeSelectedCategory = chosenCategory;
        skipRequested = true;
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
                clip != elgigantenAdClip &&
                clip != nikeAdClip)
            {
                unplayed.Add(clip);
            }
        }

        if (unplayed.Count == 0) return null;
        return unplayed[Random.Range(0, unplayed.Count)];
    }

    private VideoClip GetUnplayedAdFromAllSources()
    {
        List<VideoClip> unplayedPool = new List<VideoClip>();

        foreach (var cat in adCategories)
        {
            if (cat.categoryClips != null)
            {
                foreach (var clip in cat.categoryClips)
                {
                    if (clip != null &&
                        !playedAdClips.Contains(clip) &&
                        clip != mcdonaldsAdClip &&
                        clip != elgigantenAdClip &&
                        clip != nikeAdClip)
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
                if (clip != null &&
                    !playedAdClips.Contains(clip) &&
                    clip != mcdonaldsAdClip &&
                    clip != elgigantenAdClip &&
                    clip != nikeAdClip)
                {
                    unplayedPool.Add(clip);
                }
            }
        }

        if (unplayedPool.Count == 0)
        {
            playedAdClips.Clear();

            foreach (var cat in adCategories)
            {
                if (cat.categoryClips != null)
                {
                    foreach (var clip in cat.categoryClips)
                    {
                        if (clip != null &&
                            clip != mcdonaldsAdClip &&
                            clip != elgigantenAdClip &&
                            clip != nikeAdClip)
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
                    if (clip != null &&
                        clip != mcdonaldsAdClip &&
                        clip != elgigantenAdClip &&
                        clip != nikeAdClip)
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