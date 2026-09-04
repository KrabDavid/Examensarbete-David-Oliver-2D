using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class StandardAdBreakManager : MonoBehaviour
{
    [Header("Video Players")]
    public VideoPlayer mainVideoPlayer;
    public VideoPlayer adVideoPlayer;

    [Header("UI Elements")]
    public GameObject adOverlayPanel;
    public TMP_Text adTimerText;

    [Header("Settings")]
    public float timeUntilAdBreak = 10f;
    public float adDuration = 45f;

    [Header("Guaranteed Ads")]
    public VideoClip mcdonaldsAdClip;   // First Ad
    public VideoClip elgigantenAdClip;  // Second Ad
    public VideoClip nikeAdClip;        // Third Ad

    [Header("Remaining Ad Pool (Ad #4+)")]
    public VideoClip[] allAdClips;

    private bool adHasBeenTriggered = false;
    private bool isAdRunning = false;
    private float adTimer;
    private Coroutine adLoopCoroutine;

    private int currentAdIndex = 0; // Tracks whether this is ad 1, 2, 3...
    private List<VideoClip> playedAdClips = new List<VideoClip>();

    void Start()
    {
        adOverlayPanel.SetActive(false);
        mainVideoPlayer.Play();
        adVideoPlayer.Stop();
    }

    void Update()
    {
        // 1. Trigger ad break at 10s mark
        if (!adHasBeenTriggered && mainVideoPlayer.time >= timeUntilAdBreak)
        {
            StartAdBreak();
        }

        // 2. Countdown timer
        if (isAdRunning)
        {
            adTimer -= Time.deltaTime;

            if (adTimerText != null)
            {
                adTimerText.text = "Ad ends in: " + Mathf.CeilToInt(adTimer) + "s";
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
        currentAdIndex = 0; // Reset count for this ad break

        mainVideoPlayer.Pause();
        adOverlayPanel.SetActive(true);

        if (adLoopCoroutine != null) StopCoroutine(adLoopCoroutine);
        adLoopCoroutine = StartCoroutine(PlayAdSequence());
    }

    private IEnumerator PlayAdSequence()
    {
        while (isAdRunning)
        {
            currentAdIndex++;
            VideoClip selectedClip = null;

            // 1st Ad = Always McDonald's
            if (currentAdIndex == 1)
            {
                selectedClip = mcdonaldsAdClip;
            }
            // 2nd Ad = Always Elgiganten
            else if (currentAdIndex == 2)
            {
                selectedClip = elgigantenAdClip;
            }
            // 3rd Ad = Always Nike
            else if (currentAdIndex == 3)
            {
                selectedClip = nikeAdClip;
            }
            // 4th+ Ad = Random from pool
            else
            {
                selectedClip = GetUnplayedAd();
            }

            // Track played clips so they don't repeat unnecessarily
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

            // Runs strictly until the clip completes or overall break timer hits zero
            while (adVideoPlayer.isPlaying && isAdRunning)
            {
                yield return null;
            }

            adVideoPlayer.Stop();
            mainVideoPlayer.Pause();
        }
    }

    private VideoClip GetUnplayedAd()
    {
        List<VideoClip> unplayed = new List<VideoClip>();

        foreach (var clip in allAdClips)
        {
            // Ignore null, already played clips, and the fixed 1st/2nd/3rd clips
            if (clip != null &&
                !playedAdClips.Contains(clip) &&
                clip != mcdonaldsAdClip &&
                clip != elgigantenAdClip &&
                clip != nikeAdClip)
            {
                unplayed.Add(clip);
            }
        }

        // If all pool clips were played, reset pool history
        if (unplayed.Count == 0)
        {
            playedAdClips.Clear();

            // Re-populate valid options without McDonalds/Elgiganten/Nike
            foreach (var clip in allAdClips)
            {
                if (clip != null &&
                    clip != mcdonaldsAdClip &&
                    clip != elgigantenAdClip &&
                    clip != nikeAdClip)
                {
                    unplayed.Add(clip);
                }
            }

            // Fallback if allAdClips was empty
            if (unplayed.Count == 0) return null;
        }

        return unplayed[Random.Range(0, unplayed.Count)];
    }

    public void EndAdBreak()
    {
        isAdRunning = false;

        if (adLoopCoroutine != null) StopCoroutine(adLoopCoroutine);

        adVideoPlayer.Stop();
        adOverlayPanel.SetActive(false);
        mainVideoPlayer.Play();
    }
}