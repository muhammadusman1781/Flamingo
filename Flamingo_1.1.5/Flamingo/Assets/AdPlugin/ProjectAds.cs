using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectAds : MonoBehaviour
{
    [SerializeField] WhiteAdManager whiteAdManager;
    public static ProjectAds instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public void ShowBanners()
    {
        whiteAdManager.OpenBanner_L();
        //whiteAdManager.OpenBanner_R();
    }

    public void HideBanners()
    {
        whiteAdManager.CloseBanner_L();
        //whiteAdManager.CloseBanner_R();
    }


    public void ShowBigBannerL()
    {
        whiteAdManager.OpenBigBannerL();
    }

    public void HideBigBannerL()
    {
        whiteAdManager.CloseBigBannerL();
    }

    public void ShowBigBannerR()
    {
        whiteAdManager.OpenBigBannerR();
    }

    public void HideBigBannerR()
    {
        whiteAdManager.CloseBigBannerR();
    }

    public void ShowInterstitial()
    {
        whiteAdManager.ShowInterstitialAd();
    }

    public void ShowRewardedAd()
    {
        whiteAdManager.ShowRewardedAd();
    }

  
}
