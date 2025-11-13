using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BigBannerCall : MonoBehaviour
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
            ProjectAds.instance.ShowBigBannerL();
        }
    }

    private void OnDisable()
    {
        if (ProjectAds.instance != null)
        {
            ProjectAds.instance.HideBigBannerL();
        }
    }
}
