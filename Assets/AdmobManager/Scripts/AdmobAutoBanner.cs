using UnityEngine;

public class AdmobAutoBanner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ShowBannerAd();
    }

    private void ShowBannerAd()
    {
        // Ensure AdmobFunctions script is properly set up
        AdmobFunctions admobFunctions = FindObjectOfType<AdmobFunctions>();
        if (admobFunctions != null)
        {
            admobFunctions.ShowBanner();
            Debug.Log("Banner Ad Displayed");
        }
        else
        {
            Debug.LogError("AdmobFunctions instance not found in the scene. Please ensure AdmobFunctions prefab is present.");
        }   
    }
}
