#if UNITY_ADS_WITH_MEDITATION
using System;
using Unity.Services.Core;
using Unity.Services.Mediation;

public partial class UnityAdsProviderWithMeditation : IAdsProvider
{
    private readonly bool _debug;
    private readonly string _interstitialId;
    private readonly string _rewardedId;

    private Action<bool> _rewardedVideoAdsCallback;

    private IInterstitialAd _interstitialAd;

    public UnityAdsProviderWithMeditation(AdsProviderSettings settings)
    {
        var unityAdsSettings = (UnityAdsSetting)settings;

        _interstitialId = unityAdsSettings.interstitialId;
        _rewardedId = unityAdsSettings.rewardedId;
        _debug = unityAdsSettings.debug;

        
        
        Initialize(unityAdsSettings.appId);
    }

    private async void Initialize(string appId)
    {
        DebugIfCan($"Unity - Start Initialization");

        try
        {
            var opt = new InitializationOptions();
            opt.SetGameId(appId);
            await UnityServices.InitializeAsync(opt);
            OnInitializationComplete();
        }
        catch (Exception e)
        {
            OnInitializationFailed(e);
        }
    }
    
    public void OnInitializationComplete()
    {
        DebugIfCan($"Unity - Initialization Completed");
        LoadRewarded();
        LoadInterstitial();
    }

    public void OnInitializationFailed(Exception error)
    {
        DebugIfCan("Initialization Failed:" + error);
    }

    private void DebugIfCan(string message)
    {
        if (_debug)
        {
            UDebug.Debug(message);
        }
    }
  
}

public partial class UnityAdsProviderWithMeditation
{
    private IRewardedAd _rewardedAd;

    public bool IsRewardedAvailable()
    {
        return _rewardedAd?.AdState == AdState.Loaded;
    }

    public void ShowRewarded(Action<bool> completed = null)
    {
        if (!IsRewardedAvailable())
        {
            throw new InvalidOperationException();
        }

        _rewardedVideoAdsCallback = completed;
        _rewardedAd.ShowAsync();
    }

    private async void LoadRewarded()
    {
        _rewardedAd = MediationService.Instance.CreateRewardedAd(_rewardedId);
        _rewardedAd.OnLoaded += OnRewardedAdsLoaded;
        _rewardedAd.OnFailedLoad += OnRewardedAdsFailedLoad;
        _rewardedAd.OnFailedShow += OnRewardedAdsFailedShow;
        _rewardedAd.OnClosed += OnRewardedAdsClosed;
        _rewardedAd.OnUserRewarded += OnRewardedAdsRewarded;
        
        try
        {
            await _rewardedAd.LoadAsync();
        }
        catch (LoadFailedException)
        {
        }
    }

    private void OnRewardedAdsFailedShow(object sender, ShowErrorEventArgs e)
    {
        DebugIfCan($"Unity - Reward Ads Fail To Show");
        _rewardedVideoAdsCallback?.Invoke(false);
        _rewardedVideoAdsCallback = null;
    }

    private void OnRewardedAdsRewarded(object sender, RewardEventArgs e)
    {
        DebugIfCan($"Unity - Reward Ads Rewarded");
        _rewardedVideoAdsCallback?.Invoke(true);
        _rewardedVideoAdsCallback = null;
    }

    private void OnRewardedAdsClosed(object sender, EventArgs e)
    {
        DebugIfCan($"Unity - Reward Ads Closed");
        LoadRewarded();
    }
    private void OnRewardedAdsLoaded(object sender, EventArgs eventArgs)
    {
        DebugIfCan($"Unity - Reward Ads Loaded");
    }

    private void OnRewardedAdsFailedLoad(object sender, LoadErrorEventArgs loadErrorEventArgs)
    {
        DebugIfCan($"Unity - Rewarded Ads Load Failed : {loadErrorEventArgs.Message}");
        SimpleCoroutine.Create().Delay(5,LoadRewarded);
    }
}

public partial class UnityAdsProviderWithMeditation
{
    
    public bool IsInterstitialAvailable()
    {
        return _interstitialAd?.AdState == AdState.Loaded;
    }


    public void ShowInterstitial()
    {
        if (!IsInterstitialAvailable())
        {
            throw new InvalidOperationException();
        }

        _interstitialAd.ShowAsync();
    }
    
    private async void LoadInterstitial()
    {
        _interstitialAd = MediationService.Instance.CreateInterstitialAd(_interstitialId);
        _interstitialAd.OnLoaded += OnInterstitialLoaded;
        _interstitialAd.OnFailedLoad += OnInterstitialFailedLoad;
        _interstitialAd.OnClosed += OnInterstitialClosed;
        try
        {
            await _interstitialAd.LoadAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void OnInterstitialClosed(object sender, EventArgs e)
    {
        DebugIfCan($"Unity - Interstitial Closed");
        LoadInterstitial();
    }

    private void OnInterstitialFailedLoad(object sender, LoadErrorEventArgs e)
    {
        DebugIfCan($"Unity - Interstitial Fail To Load - {e.Message}");

        SimpleCoroutine.Create().Delay(5,LoadInterstitial);
    }

    private void OnInterstitialLoaded(object sender, EventArgs e)
    {
        DebugIfCan($"Unity - Interstitial Ads Loaded");
    }
}
#endif