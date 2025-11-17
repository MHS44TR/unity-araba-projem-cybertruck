//// ADMOB MANAGER V1.21121 //////////////////////////////
/// DEVELOPED BY ALEX QUESADA ////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobFunctions : MonoBehaviour
{
    public void ShowBanner()
    {
        AdmobManager.Instance.ShowBannerAd();
    }

    public void CloseBanner()
    {
        AdmobManager.Instance.CloseBannerAd();
    }

    public void ShowIntersticial()
    {
        AdmobManager.Instance.ShowInterstitialdAd();
    }

    public void ShowRewarded()
    {
        AdmobManager.Instance.ShowRewardedAd();
    }

    public void AcceptUserConsent()
    {
        AdmobManager.Instance.UserAdsConsent(true);
    }

    public void CancelUserConsent()
    {
        AdmobManager.Instance.UserAdsConsent(false);
    }

}
