using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightBigBanner : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(BigBannerDelay());
    }

    private IEnumerator BigBannerDelay()
    {
        yield return new WaitForSeconds(0.03f);
        if (ProjectAds.instance != null)
        {
            ProjectAds.instance.ShowBigBannerR();
        }
    }

    private void OnDisable()
    {
        if (ProjectAds.instance != null)
        {
            ProjectAds.instance.HideBigBannerR();
        }
    }
}
