using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void LoadMainMenu()
    {
        StartCoroutine(PlaySoundAndLoadScene("Scene1_MainMenu"));
    }

    public void LoadNormalAdsScene()
    {
        StartCoroutine(PlaySoundAndLoadScene("Scene2_NormalAds"));
    }

    public void LoadChooseAdsScene()
    {
        StartCoroutine(PlaySoundAndLoadScene("Scene3_ChooseAds"));
    }

    public void LoadGamifiedAdsScene()
    {
        StartCoroutine(PlaySoundAndLoadScene("Scene4_GamifiedAds"));
    }

    private IEnumerator PlaySoundAndLoadScene(string sceneName)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayConfirmClick();

            // Vänta på att ljudeffekten spelas klart (lägger till en kort fördröjning om klippet finns)
            float delay = AudioManager.Instance.confirmMenuClick != null ? AudioManager.Instance.confirmMenuClick.length : 0.2f;
            yield return new WaitForSeconds(delay);
        }

        SceneManager.LoadScene(sceneName);
    }
}