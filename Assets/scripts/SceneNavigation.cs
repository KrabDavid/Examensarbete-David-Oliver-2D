using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    // Load scenes by name
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Scene1_MainMenu");
    }

    public void LoadNormalAdsScene()
    {
        SceneManager.LoadScene("Scene2_NormalAds");
    }

    public void LoadChooseAdsScene()
    {
        SceneManager.LoadScene("Scene3_ChooseAds");
    }

    public void LoadGamifiedAdsScene()
    {
        SceneManager.LoadScene("Scene4_GamifiedAds");
    }
}