using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip confirmMenuClick;
    public AudioClip openMenuClick;
    public AudioClip pickUpSFX;
    public AudioClip missSFX; // New clip added here!

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "Scene1_MainMenu")
            {
                SceneManager.LoadScene("Scene1_MainMenu");
            }
        }
    }

    public void PlayConfirmClick()
    {
        if (sfxSource != null && confirmMenuClick != null)
            sfxSource.PlayOneShot(confirmMenuClick);
    }

    public void PlayOpenClick()
    {
        if (sfxSource != null && openMenuClick != null)
            sfxSource.PlayOneShot(openMenuClick);
    }

    public void PlayPickUp()
    {
        if (sfxSource != null && pickUpSFX != null)
            sfxSource.PlayOneShot(pickUpSFX);
    }

    public void PlayMiss()
    {
        if (sfxSource != null && missSFX != null)
            sfxSource.PlayOneShot(missSFX);
    }
}