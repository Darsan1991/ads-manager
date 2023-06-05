using System;
using UnityEngine;

public interface IAdsProvider
{
    bool IsInterstitialAvailable();
    bool IsRewardedAvailable();

    void ShowInterstitial();

    void ShowRewarded(Action<bool> completed = null);

}


