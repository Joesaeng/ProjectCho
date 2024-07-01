using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsTest : MonoBehaviour
{
    private void Start()
    {
        Managers.Init();
    }
    public void ShowAds()
    {
        Managers.Ads.ShowRewardedAd(()=> Debug.Log("광고봄"));
    }
}
