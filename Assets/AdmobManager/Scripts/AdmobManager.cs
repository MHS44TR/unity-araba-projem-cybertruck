//// ADMOB MANAGER V1.230529 ////////////////////////////////////////////
/// Built and tested with: Google Mobile Ads Unity Plugin v8.2.0 ///////
/// Google Mobile Ads Android SDK 22.0.0 //////////////////////////////
/// Google Mobile Ads iOS SDK 10.4 ///////////////////////////////////
/// 
/// DEVELOPED BY AlexQuesadaDev /////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class AdmobManager : MonoBehaviour
{
    [Header("Ads Settings")]
    [Tooltip("npa = 1 for NO personalized ADS || npa = 0 for personalized ads, need gdpr consent")]
    [SerializeField] private int _npaValue = 1;  //npa = 1 NO ADS PERSONALIZABLES
    [SerializeField] private bool _showTestAds = false;
    [SerializeField] private bool _bannerOnTopPosition = false;
    [SerializeField] private int _bannerClicksLimit = 2;
    [Tooltip("0 for not showing again")]
    [SerializeField] private float _timeToShowBannerAfterDestroy = 125f; // -1 = not showing again
    [SerializeField] private int _numberOfRequestTryWhenFailed = 5; //Note that this number (the number of request attempts) is reset each time an ad is shown successfully

    [Header("Android Ads Ids")]
    [SerializeField] private string _androidInterstitial;
    [SerializeField] private string _androidSBanner;
    [SerializeField] private string _androidRewarded;
    [Header("iOS Ads Ids")]
    [SerializeField] private string _iOSInterstitial;
    [SerializeField] private string _iOSBanner;
    [SerializeField] private string _iOSRewarded;

    private InterstitialAd _interstitial;
    private BannerView _banner;
    private RewardedAd _rewardedAd;

    private int _numberOfLoadTry = 0;
    private int _numberOfBannerClicks = 0;    

    private static AdmobManager _instance;
    public static AdmobManager Instance { get => _instance; }

    public Action OnGetReward;  // Te puedes suscribir a este delegado desde cualquier parte para saber cuando obtener un rewarded
    Queue<Action> _mainThreadJobs = new Queue<Action>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (this != _instance)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _npaValue = PlayerPrefs.GetInt("npa", 1);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
           // Debug.Log("El Sdk admob se ha iniciado");
        });
        RequestBanner();
        RequestInterstitial();
        RequestRewarded();
    }

    void Update()
    {
        while (_mainThreadJobs.Count > 0)
            _mainThreadJobs.Dequeue().Invoke();
    }

    private void AddJob(Action newJob)
    {
        _mainThreadJobs.Enqueue(newJob);
    }

    private void RequestInterstitial()
    {
        string idInterstitial = "unexpected_platform";
        if (_showTestAds)
        {
#if UNITY_ANDROID
            idInterstitial = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
            idInterstitial = "ca-app-pub-3940256099942544/4411468910";
#endif
        }
        else
        {
#if UNITY_ANDROID
            idInterstitial = _androidInterstitial;
#elif UNITY_IPHONE
            idInterstitial = _iOSInterstitial;
#endif
        }


        // Clean up the old ad before loading a new one.
        if (_interstitial != null)
        {
            _interstitial.Destroy();
            _interstitial = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Extras.Add("npa", _npaValue.ToString());
        adRequest.Keywords.Add("new-interstitial-ad");

        // send the request to load the ad.
        InterstitialAd.Load(idInterstitial, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    AddJob(() =>
                    {
                        _numberOfLoadTry++;
                        if (_numberOfLoadTry < _numberOfRequestTryWhenFailed)
                        {
                            RequestInterstitial();
                        }
                    });
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitial = ad;


                // Raised when an ad opened full screen content.
                _interstitial.OnAdFullScreenContentOpened += () =>
                {
                    AddJob(() =>
                    {
                        AudioListener.volume = 0f;
                    });
                };
                // Raised when the ad closed full screen content.
                _interstitial.OnAdFullScreenContentClosed += () =>
                {

                    AddJob(() =>
                    {
                        RequestInterstitial();
                        AudioListener.volume = 1f;

                    });
                };
                // Raised when the ad failed to open full screen content.
                _interstitial.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    AddJob(() =>
                    {
                        _numberOfLoadTry++;
                        if (_numberOfLoadTry < _numberOfRequestTryWhenFailed)
                        {
                            RequestInterstitial();
                        }
                    });
                };
            });

    }

    public bool ShowInterstitialdAd()
    {
        if (_interstitial != null && _interstitial.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _numberOfLoadTry = 0;
            _interstitial.Show();
            return true;
        }
        else
        {       
            Debug.LogError("Interstitial ad is not ready yet.");
            _numberOfLoadTry++;
            if (_numberOfLoadTry < _numberOfRequestTryWhenFailed)
            {
                RequestInterstitial();
            }
   
            return false;
        }
    }

    private void RequestBanner()
    {
        string idBanner = "unexpected_platform";
        if (_showTestAds)
        {
#if UNITY_ANDROID
            idBanner = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
             idBanner = "ca-app-pub-3940256099942544/2934735716";
#endif
        }
        else
        {
#if UNITY_ANDROID
            idBanner = _androidSBanner;
#elif UNITY_IPHONE
            idBanner = _iOSBanner;
#endif
        }

        if (_banner != null)
        {
            CloseBannerAd();
        }

        //Create a 320x50 banner at the bottom of the screen
        AdPosition bannerPosition = AdPosition.Bottom;
        if (_bannerOnTopPosition)
        {
            bannerPosition = AdPosition.Top;
        }
        _banner = new BannerView(idBanner, AdSize.Banner, bannerPosition);
        //_banner = new BannerView(idBanner, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), bannerPosition);
    }

    public void ShowBannerAd()
    {
        if (_banner == null)
        {
            RequestBanner();
        }
  
        AdRequest adRequest = new AdRequest();
        adRequest.Extras.Add("npa", _npaValue.ToString());
        adRequest.Keywords.Add("new-banner-ad");
        _banner.LoadAd(adRequest);

        // Raised when a click is recorded for an ad.
        _banner.OnAdClicked += () =>
        {
            AddJob(() =>
            {
                _numberOfBannerClicks++;

                if (_numberOfBannerClicks >= _bannerClicksLimit)
                {
                    CloseBannerAd();
                    if (_timeToShowBannerAfterDestroy > 0f)
                    {
                        _numberOfBannerClicks = 0;
                        Invoke(nameof(ShowBannerAd), _timeToShowBannerAfterDestroy);
                    }
                }
            });
        };
    }

    public void CloseBannerAd()
    {
        if (_banner != null)
        {
            _banner.Destroy();
            _banner = null;
        }
    }

    private void RequestRewarded()
    {
        string idRewarded = "unexpected_platform";
        if (_showTestAds)
        {
#if UNITY_ANDROID
            idRewarded = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
             idRewarded = "ca-app-pub-3940256099942544/1712485313";
#endif
        }
        else
        {
#if UNITY_ANDROID
            idRewarded = _androidRewarded;
#elif UNITY_IPHONE
            idRewarded = _iOSRewarded;
#endif
        }

        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Extras.Add("npa", _npaValue.ToString());
        adRequest.Keywords.Add("new-rewarded-ad");

        // send the request to load the ad.
        RewardedAd.Load(idRewarded, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    AddJob(() =>
                    {
                        _numberOfLoadTry++;
                        if (_numberOfLoadTry < _numberOfRequestTryWhenFailed)
                        {
                            RequestRewarded();
                        }
                    });
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;

                // Raised when an ad opened full screen content.
                _rewardedAd.OnAdFullScreenContentOpened += () =>
                {
                    AddJob(() =>
                    {
                        AudioListener.volume = 0f;
                    });
                };
                // Raised when the ad closed full screen content.
                _rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    AddJob(() =>
                    {
                        AudioListener.volume = 1f;
                        RequestRewarded();
                    });
                };
                // Raised when the ad failed to open full screen content.
                _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    AddJob(() =>
                    {
                        _numberOfLoadTry++;
                        if (_numberOfLoadTry < _numberOfRequestTryWhenFailed)
                        {
                            RequestRewarded();
                        }
                    });

                };
            });


    }

    public bool ShowRewardedAd()
    {
        const string rewardMsg =
        "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _numberOfLoadTry = 0;
            _rewardedAd.Show((Reward reward) =>
            {
                AddJob(() =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                    OnGetReward.Invoke();
                });

            });
            return true;
        }
        else
        {
            _numberOfLoadTry++;
            if (_numberOfLoadTry < _numberOfRequestTryWhenFailed)
            {
                RequestRewarded();
            }

        }
        return false;
    }

    public void UserAdsConsent(bool userConsent)
    {
        if (userConsent)
        {
            _npaValue = 0;
            PlayerPrefs.SetInt("npa", 0);
        }
        else
        {
            _npaValue = 1;
            PlayerPrefs.SetInt("npa", 1);
        }
    }
}
