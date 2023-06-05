#if UNITY_ADS_LEGACY
using System;
using DGames.DDebug;
using UnityEngine.Advertisements;
namespace DGames.Ads{
public partial class LegacyUnityAdsProvider : IAdsProvider, IUnityAdsInitializationListener
{
    private readonly bool _debug;
    private readonly string _interstitialId;
    private readonly string _rewardedId;

    private Action<bool> _rewardedVideoAdsCallback;


    public LegacyUnityAdsProvider(AdsProviderSettings settings)
    {
        var unityAdsSettings = (UnityAdsSetting)settings;

        _interstitialId = unityAdsSettings.interstitialId;
        _rewardedId = unityAdsSettings.rewardedId;
        _debug = unityAdsSettings.debug;

        
        Initialize(unityAdsSettings.appId,unityAdsSettings.testMode);
    }

    private void Initialize(string appId,bool testMode)
    {
        DebugIfCan($"Unity - Start Initialization");
        Advertisement.Initialize(appId,testMode,this);
    }
    
    public void OnInitializationComplete()
    {
        DebugIfCan($"Unity - Initialization Completed");
        LoadRewarded();
        LoadInterstitial();
    }
    
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
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

public partial class LegacyUnityAdsProvider
{

    private bool _rewardAdsAvailable;
    public bool IsRewardedAvailable()
    {
        return _rewardAdsAvailable;
    }

    public void ShowRewarded(Action<bool> completed = null)
    {
        if (!IsRewardedAvailable())
        {
            throw new InvalidOperationException();
        }

        _rewardedVideoAdsCallback = completed;
        Advertisement.Show(_rewardedId,this);
    }

    private  void LoadRewarded()
    {
        Advertisement.Load(_rewardedId,this);
    }

   

    private void OnRewardedAdsFailedShow(string message)
    {
        DebugIfCan($"Unity - Reward Ads Fail To Show:{message}");
        _rewardedVideoAdsCallback?.Invoke(false);
        _rewardedVideoAdsCallback = null;
    }

    private void OnRewardedAdsRewarded()
    {
        DebugIfCan($"Unity - Reward Ads Rewarded");
        _rewardedVideoAdsCallback?.Invoke(true);
        _rewardedVideoAdsCallback = null;
        LoadRewarded();
    }
    
    private void OnRewardedAdsLoaded()
    {
        DebugIfCan($"Unity - Reward Ads Loaded");
    }

    private void OnRewardedAdsFailedLoad(string message)
    {
        DebugIfCan($"Unity - Rewarded Ads Load Failed : {message}");
        SimpleCoroutine.Create().Delay(30,LoadRewarded);
    }
}

public partial class LegacyUnityAdsProvider
{
    private bool _interstitialAdsAvailable;

    public bool IsInterstitialAvailable()
    {
        return _interstitialAdsAvailable;
    }


    public void ShowInterstitial()
    {
        if (!IsInterstitialAvailable())
        {
            throw new InvalidOperationException();
        }

        Advertisement.Show(_interstitialId,this);
        _interstitialAdsAvailable = false;
    }
    
    private void LoadInterstitial()
    {
        Advertisement.Load(_interstitialId,this);
    }

    private void OnInterstitialClosed()
    {
        DebugIfCan($"Unity - Interstitial Closed");
        LoadInterstitial();
    }

    private void OnInterstitialFailedLoad(string message)
    {
        DebugIfCan($"Unity - Interstitial Fail To Load - {message}");

        SimpleCoroutine.Create().Delay(5,LoadInterstitial);
    }

    private void OnInterstitialLoaded()
    {
        DebugIfCan($"Unity - Interstitial Ads Loaded");
        _interstitialAdsAvailable = true;
    }
}

public partial class LegacyUnityAdsProvider:IUnityAdsShowListener
{
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if(placementId == _rewardedId)
            OnRewardedAdsFailedShow(message);
        
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if(placementId == _rewardedId)
        {
            OnRewardedAdsRewarded();
        }
        else if(placementId == _interstitialId)
            OnInterstitialClosed();
    }
}

public partial class LegacyUnityAdsProvider:IUnityAdsLoadListener
{
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if(placementId == _rewardedId)
            OnRewardedAdsLoaded();
        else if(placementId == _interstitialId)
            OnInterstitialLoaded();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if(placementId == _rewardedId)
            OnRewardedAdsFailedLoad(message);
        else if (placementId == _interstitialId)
            OnInterstitialFailedLoad(message);
    }
}
}

#endif