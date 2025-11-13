using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WhiteAdManager : MonoBehaviour
{
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
[SerializeField] private string _adBannerL = "ca-app-pub-3940256099942544/6300978111";     // Test Banner Ad
[SerializeField] private string _adBannerR = "ca-app-pub-3940256099942544/6300978111";     // Test Banner Ad
[SerializeField] private string _adBigBannerL = "ca-app-pub-3940256099942544/6300978111";  // Test Banner Ad (use adaptive size)
[SerializeField] private string _adBigBannerR = "ca-app-pub-3940256099942544/6300978111";  // Test Banner Ad (use adaptive size)
[SerializeField] private string _adUnitInter = "ca-app-pub-3940256099942544/1033173712";   // Test Interstitial Ad
[SerializeField] private string _adUnitRewarded = "ca-app-pub-3940256099942544/5224354917"; // Test Rewarded Ad
[SerializeField] private string _adUnitAppOpen = "ca-app-pub-3940256099942544/9257395921"; // Test App Open Ad

#elif UNITY_IPHONE
[SerializeField] private string _adBannerL = "ca-app-pub-3940256099942544/2934735716";     // Test Banner Ad
[SerializeField] private string _adBannerR = "ca-app-pub-3940256099942544/2934735716";     // Test Banner Ad
[SerializeField] private string _adBigBannerL = "ca-app-pub-3940256099942544/2934735716";  // Test Banner Ad
[SerializeField] private string _adBigBannerR = "ca-app-pub-3940256099942544/2934735716";  // Test Banner Ad
[SerializeField] private string _adUnitInter = "ca-app-pub-3940256099942544/4411468910";   // Test Interstitial Ad
[SerializeField] private string _adUnitRewarded = "ca-app-pub-3940256099942544/1712485313"; // Test Rewarded Ad
[SerializeField] private string _adUnitAppOpen = "ca-app-pub-3940256099942544/5662855259"; // Test App Open Ad

#else
    private string _adBannerL = "unused";
    private string _adBannerR = "unused";
    private string _adBigBannerL = "unused"; 
    private string _adBigBannerR = "unused"; 
    private string _adUnitInter = "unused";
    private string _adUnitRewarded = "unused";
    private string _adUnitAppOpen = "unused";

#endif

    BannerView _bannerViewL;
    BannerView _bannerViewR;
    BannerView _bigBannerL;
    BannerView _bigBannerR;

    InterstitialAd _interstitialAd;
    RewardedAd _rewardedAd;
    private DateTime _expireTime;
    AppOpenAd _appOpenAd;

    private bool _isFirstOpen;

    // Rewarded ad callbacks
    public System.Action OnRewardedAdWatched;
    public System.Action OnRewardedAdFailed;


    void Start()
    {

        ConsentRequestParameters request = new ConsentRequestParameters();

        ConsentInformation.Update(request, OnConsentInfoUpdated);

    }

    void OnConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            UnityEngine.Debug.LogError(consentError);
            return;
        }

        // If the error is null, the consent information state was updated.
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if (formError != null)
            {
                // Consent gathering failed.
                UnityEngine.Debug.LogError(consentError);
                return;
            }

            // Consent has been gathered.
            if (ConsentInformation.CanRequestAds())
            {
                MobileAds.RaiseAdEventsOnUnityMainThread = true;
                // Initialize the Google Mobile Ads SDK.
                MobileAds.Initialize((InitializationStatus initStatus) =>
                {
                    // This callback is called once the MobileAds SDK is initialized.
                    Debug.Log("Ads Initialised...");
                });

                // Check if it's the first time the game is opened
                if (PlayerPrefs.GetInt("FirstOpen", 1) == 1)
                {
                    // Mark that the app is being opened for the first time
                    _isFirstOpen = true;
                    PlayerPrefs.SetInt("FirstOpen", 0); // Set it to 0 so it's not considered the first time again
                    PlayerPrefs.Save();
                }
                else
                {
                    _isFirstOpen = false;
                }

                //LoadAppOpenAd();
                LoadInterstitialAd();
                LoadRewardedAd();
            }
        });
    }



    #region AppOpen

    void OnApplicationFocus(bool hasFocus)
    {
        ////// If the app is in focus, ad is available, and it is not the first time opening
        //if (hasFocus && _appOpenAd != null && !_isFirstOpen)
        //{
        //    ShowAppOpenAd();
        //}
    }

    /// <summary>
    /// Loads the app open ad.
    /// </summary>
    public void LoadAppOpenAd()
    {
        // Clean up the old ad before loading a new one.
        if (_appOpenAd != null)
        {
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }

        Debug.Log("Loading the app open ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(_adUnitAppOpen, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                   "with error : " + error);
                    LoadAppOpenAd();
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                          + ad.GetResponseInfo());

                // App open ads can be preloaded for up to 4 hours.
                _expireTime = DateTime.Now + TimeSpan.FromHours(4);

                _appOpenAd = ad;
                RegisterEventHandlersAppOpen(ad);
            });
    }

    public bool IsAdAvailable
    {
        get
        {
            return _appOpenAd != null
                   && _appOpenAd.CanShowAd()
                   && DateTime.Now < _expireTime;
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("App State changed to : " + state);

        // if the app is Foregrounded, ad is available, and not the first open
        if (state == AppState.Foreground && !_isFirstOpen)
        {
            if (IsAdAvailable)
            {
                ShowAppOpenAd();
            }
            else
            {
                LoadAppOpenAd();
            }
        }
    }

    public void ShowAppOpenAd()
    {
        if (_appOpenAd != null && _appOpenAd.CanShowAd())
        {
            Debug.Log("Showing app open ad.");
            _appOpenAd.Show();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
            LoadAppOpenAd();
        }
    }


    private void RegisterEventHandlersAppOpen(AppOpenAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            LoadAppOpenAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);

            LoadAppOpenAd();
        };
    }


    #endregion



    #region Banner_L

    public void OpenBanner_L()
    {
        LoadAdL();
    }

    public void CloseBanner_L()
    {
        DestroyAdL();
    }



    /// <summary>
    /// Creates a 320x50 banner view at top of the screen.
    /// </summary>
    public void CreateBannerViewL()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerViewL != null)
        {
            DestroyAdL();
        }

        // Create a 320x50 banner at top of the screen
        _bannerViewL = new BannerView(_adBannerL, AdSize.Banner, AdPosition.Top);
    }

    /// <summary>
    /// Creates the banner view and loads a banner ad.
    /// </summary>
    public void LoadAdL()
    {
        // create an instance of a banner view first.
        if (_bannerViewL == null)
        {
            CreateBannerViewL();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerViewL.LoadAd(adRequest);
    }

    /// <summary>
    /// Destroys the banner view.
    /// </summary>
    public void DestroyAdL()
    {
        if (_bannerViewL != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerViewL.Destroy();
            _bannerViewL = null;
        }
    }


    /// <summary>
    /// listen to events the banner view may raise.
    /// </summary>
    private void ListenToAdEventsL()
    {
        _bannerViewL.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bannerViewL.GetResponseInfo());
        };
        _bannerViewL.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        _bannerViewL.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        _bannerViewL.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        _bannerViewL.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        _bannerViewL.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        _bannerViewL.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    #endregion


    #region Banner_R

    public void OpenBanner_R()
    {
        LoadAdR();
    }

    public void CloseBanner_R()
    {
        DestroyAdR();
    }



    /// <summary>
    /// Creates a 320x50 banner view at top of the screen.
    /// </summary>
    public void CreateBannerViewR()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerViewR != null)
        {
            DestroyAdR();
        }

        // Create a 320x50 banner at top of the screen
        _bannerViewR = new BannerView(_adBannerR, AdSize.Banner, AdPosition.TopRight);
    }

    /// <summary>
    /// Creates the banner view and loads a banner ad.
    /// </summary>
    public void LoadAdR()
    {
        // create an instance of a banner view first.
        if (_bannerViewR == null)
        {
            CreateBannerViewR();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerViewR.LoadAd(adRequest);
    }

    /// <summary>
    /// Destroys the banner view.
    /// </summary>
    public void DestroyAdR()
    {
        if (_bannerViewR != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerViewR.Destroy();
            _bannerViewR = null;
        }
    }


    /// <summary>
    /// listen to events the banner view may raise.
    /// </summary>
    private void ListenToAdEventsR()
    {
        _bannerViewR.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bannerViewR.GetResponseInfo());
        };
        _bannerViewR.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        _bannerViewR.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        _bannerViewR.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        _bannerViewR.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        _bannerViewR.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        _bannerViewR.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    #endregion


    #region BigBannerL

    public void OpenBigBannerL()
    {
        LoadAdBL();
    }

    public void CloseBigBannerL()
    {
        DestroyAdBL();
    }


    /// <summary>
    /// Creates a 320x50 banner view at top of the screen.
    /// </summary>
    public void CreateBannerViewBL()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bigBannerL != null)
        {
            DestroyAdBL();
        }

        // Create a 320x50 banner at top of the screen
        _bigBannerL = new BannerView(_adBigBannerL, AdSize.MediumRectangle, AdPosition.BottomLeft);
    }


    /// <summary>
    /// Creates the banner view and loads a banner ad.
    /// </summary>
    public void LoadAdBL()
    {
        // create an instance of a banner view first.
        if (_bigBannerL == null)
        {
            CreateBannerViewBL();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bigBannerL.LoadAd(adRequest);
    }

    /// <summary>
    /// Destroys the banner view.
    /// </summary>
    public void DestroyAdBL()
    {
        if (_bigBannerL != null)
        {
            Debug.Log("Destroying banner view.");
            _bigBannerL.Destroy();
            _bigBannerL = null;
        }
    }


    /// <summary>
    /// listen to events the banner view may raise.
    /// </summary>
    private void ListenToAdEventsBL()
    {
        _bigBannerL.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bigBannerL.GetResponseInfo());
        };
        _bigBannerL.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        _bigBannerL.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        _bigBannerL.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        _bigBannerL.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        _bigBannerL.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        _bigBannerL.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }


    #endregion

    #region BigBannerR

    public void OpenBigBannerR()
    {
        LoadAdBR();
    }

    public void CloseBigBannerR()
    {
        DestroyAdBR();
    }


    /// <summary>
    /// Creates a 320x50 banner view at top of the screen.
    /// </summary>
    public void CreateBannerViewBR()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bigBannerR != null)
        {
            DestroyAdBR();
        }

        // Create a 320x50 banner at top of the screen
        _bigBannerR = new BannerView(_adBigBannerR, AdSize.MediumRectangle, AdPosition.BottomRight);
    }


    /// <summary>
    /// Creates the banner view and loads a banner ad.
    /// </summary>
    public void LoadAdBR()
    {
        // create an instance of a banner view first.
        if (_bigBannerR == null)
        {
            CreateBannerViewBR();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bigBannerR.LoadAd(adRequest);
    }

    /// <summary>
    /// Destroys the banner view.
    /// </summary>
    public void DestroyAdBR()
    {
        if (_bigBannerR != null)
        {
            Debug.Log("Destroying banner view.");
            _bigBannerR.Destroy();
            _bigBannerR = null;
        }
    }


    /// <summary>
    /// listen to events the banner view may raise.
    /// </summary>
    private void ListenToAdEventsBR()
    {
        _bigBannerR.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bigBannerR.GetResponseInfo());
        };
        _bigBannerR.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        _bigBannerR.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        _bigBannerR.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        _bigBannerR.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        _bigBannerR.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        _bigBannerR.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }


    #endregion

    #region InterstitialAd

    /// <summary>
    /// Loads the interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        var adRequest = new AdRequest();

        InterstitialAd.Load(_adUnitInter, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    LoadInterstitialAd();
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
                RegisterEventHandlers(ad);
            });
    }


    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            LoadInterstitialAd();
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            CloseBanner_L();
           
            Debug.Log("Interstitial ad full screen content opened.");
        };
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            OpenBanner_L();
           
            Debug.Log("Interstitial ad full screen content closed.");
            LoadInterstitialAd();
            
        };
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            LoadInterstitialAd();
        };
    }

    #endregion


    #region RewardedAd 

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        var adRequest = new AdRequest();

        RewardedAd.Load(_adUnitRewarded, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    LoadRewardedAd();
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
                RegisterEventHandlers(ad);
            });
    }

    public void ShowRewardedAd()
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
            {
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                OnRewardedAdWatched?.Invoke();
            });
        }
        else
        {
            Debug.Log("Rewarded ad not ready, loading...");
            LoadRewardedAd();
            // Notify that ad failed to load
            OnRewardedAdFailed?.Invoke();
        }
    }


    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            LoadRewardedAd();
            // Notify that ad failed
            OnRewardedAdFailed?.Invoke();
        };
    }


    #endregion
}
