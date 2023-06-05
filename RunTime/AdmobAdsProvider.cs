#if ADMOB
using System;
using GoogleMobileAds.Api;

public class AdmobAdsProvider : IAdsProvider
{
    private readonly bool _debug;
    private readonly string _interstitialId;
    private readonly string _rewardedId;
    private RewardedAd _rewardBaseVideo;
    private InterstitialAd _interstitialAd;
    private Action<bool> _rewardVideoAdsCallback;

    public AdmobAdsProvider(AdsProviderSettings settings)
    {
        var admobSetting = (AdmobSetting)settings;
        _interstitialId = admobSetting.interstitialId;
        _rewardedId = admobSetting.admobRewardedId;
        _debug = admobSetting.debug;
        MobileAds.Initialize(_ =>
        {
            RequestInterstitial();
            RequestRewardVideo();
        });
    }

    public bool IsInterstitialAvailable()
    {
        return _interstitialAd != null && _interstitialAd.IsLoaded();
    }

    public bool IsRewardedAvailable()
    {
        return _rewardBaseVideo != null && _rewardBaseVideo.IsLoaded();
    }

    public void ShowInterstitial()
    {
        if (!IsInterstitialAvailable())
            throw new InvalidOperationException();

        _interstitialAd.Show();
    }

    public void ShowRewarded(Action<bool> completed = null)
    {
        if (!IsRewardedAvailable())
            throw new InvalidOperationException();
        _rewardVideoAdsCallback = completed;
        _rewardBaseVideo.Show();
    }

    private void RequestRewardVideo()
    {
        _rewardBaseVideo = new RewardedAd(_rewardedId);
        _rewardBaseVideo.OnUserEarnedReward += RewardBaseVideoOnOnAdRewarded;
        _rewardBaseVideo.OnAdLoaded += (_, _) => { DebugIfCan("Admob - Reward Ads Loaded."); };

        _rewardBaseVideo.OnAdFailedToLoad += (_, args) =>
        {
            DebugIfCan($"Admob - Reward Ads Failed to Load:{args.LoadAdError}");
            SimpleCoroutine.Create().Delay(6, RequestRewardVideo);
        };
        _rewardBaseVideo.OnAdClosed += (_, _) =>
        {
            DebugIfCan("Admob - Reward Ads Closed.");
            RequestRewardVideo();
        };
        var request = new AdRequest.Builder().Build();
        _rewardBaseVideo.LoadAd(request);
    }

    private void DebugIfCan(string message)
    {
        if (_debug)
        {
            UDebug.Debug(message);
        }
    }

    private void RewardBaseVideoOnOnAdRewarded(object sender, Reward e)
    {
        _rewardVideoAdsCallback?.Invoke(true);
        _rewardVideoAdsCallback = null;
    }


    private void RequestInterstitial()
    {
        _interstitialAd = new InterstitialAd(_interstitialId);
        _interstitialAd.OnAdClosed += (_, _) =>
        {
            DebugIfCan($"Admob - Interstitial Closed");
            RequestInterstitial();
            
        };
        _interstitialAd.OnAdFailedToLoad += (_, args) =>
        {
            DebugIfCan($"Admob - Interstitial Load Fail : {args.LoadAdError}");
            SimpleCoroutine.Create().Delay(6, RequestInterstitial);
        };
        _interstitialAd.OnAdLoaded += (_, _) =>
        {
            DebugIfCan($"Admob - Interstitial Loaded");
        };

        var request = new AdRequest.Builder().Build();
        _interstitialAd.LoadAd(request);
    }
}

#endif