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

    [Header("Ad Pool")]
    public VideoClip mcdonaldsAdClip;
    public VideoClip[] allAdClips;

    private bool adHasBeenTriggered = false;
    private bool isAdRunning = false;
    private float adTimer;
    private Coroutine adLoopCoroutine;

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

        // 2. Countdown timer only (No input checks)
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

        mainVideoPlayer.Pause();
        adOverlayPanel.SetActive(true);

        if (adLoopCoroutine != null) StopCoroutine(adLoopCoroutine);
        adLoopCoroutine = StartCoroutine(PlayAdSequence());
    }

    private IEnumerator PlayAdSequence()
    {
        bool isFirstAd = true;

        while (isAdRunning)
        {
            VideoClip selectedClip = null;

            // Always start with McDonald's
            if (isFirstAd)
            {
                selectedClip = mcdonaldsAdClip;
                isFirstAd = false;
            }
            else
            {
                selectedClip = GetUnplayedAd();
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
            if (clip != null && !playedAdClips.Contains(clip))
            {
                unplayed.Add(clip);
            }
        }

        if (unplayed.Count == 0)
        {
            playedAdClips.Clear();
            return allAdClips[Random.Range(0, allAdClips.Length)];
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