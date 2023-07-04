using System;
using System.Collections.Generic;
using System.Linq;
using DGames.ObjectEssentials.Scriptable;
using UnityEngine;
using Random = UnityEngine.Random;


namespace DGames.Ads
{
    // ReSharper disable once HollowTypeName

    public partial class AdsManager : Singleton<AdsManager>, IAdsManager
    {

        [SerializeField] private ValueField<bool> _premium = new("PREMIUM");
        public bool Initialized { get; private set; }
        

        public bool HaveSetupConsent => PrefManager.HasKey(nameof(ConsentActive));

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<IAdsProvider, AdsProviderSettings> _providerVsSettings = new();

        public bool ConsentActive
        {
            get => PrefManager.GetBool(nameof(ConsentActive));
            set => PrefManager.SetBool(nameof(ConsentActive), value);
        }

        public bool EnableAds => _premium;


        private void Start()
        {
            if (HaveSetupConsent || !AdsSettings.Default.ConsentSetting.enable)
                Init();

        }


        public void Init()
        {
            if (Initialized)
                return;

#if ADMOB
            var admobSettings = Application.platform == RuntimePlatform.Android
                ? AdsSettings.Default.AndroidAdmobSetting
                : AdsSettings.Default.IOSAdmobSetting;
            var admobAdsProvider = new AdmobAdsProvider(admobSettings);
            _providerVsSettings.Add(admobAdsProvider, admobSettings);
#endif

#if UNITY_ADS_WITH_MEDITATION
            var unityAdsSettings = Application.platform == RuntimePlatform.Android
                ? AdsSettings.Default.AndroidUnityAdsSetting
                : AdsSettings.Default.IOSUnityAdsSetting;
            var unityAdsProvider = new UnityAdsProviderWithMeditation(unityAdsSettings);
            _providerVsSettings.Add(unityAdsProvider, unityAdsSettings);


#elif UNITY_ADS_LEGACY
            var unityAdsSettings = Application.platform == RuntimePlatform.Android
                ? AdsSettings.Default.AndroidUnityAdsSetting
                : AdsSettings.Default.IOSUnityAdsSetting;
            var unityAdsProvider = new LegacyUnityAdsProvider(unityAdsSettings);
            _providerVsSettings.Add(unityAdsProvider, unityAdsSettings);
#endif



            Initialized = true;
        }

    }



// ReSharper disable once HollowTypeName
    public partial class AdsManager
    {
        public void ShowInterstitial(string providerId = null)
        {
            var keyValuePairs = Instance._providerVsSettings.Where(p =>
                (string.IsNullOrEmpty(providerId) || p.Value.Id == providerId) && p.Value.interstitialPriority > 0 &&
                p.Key.IsInterstitialAvailable()).ToList();


            if (!keyValuePairs.Any())
            {
                return;
            }

            var adsProvider = GetRandomWithPriority(keyValuePairs.Select(p => p.Key),
                keyValuePairs.Select(p => p.Value.interstitialPriority));
            adsProvider.ShowInterstitial();
        }


        //    // ReSharper disable once FlagArgument
        //    // ReSharper disable once MethodTooLong
        public void ShowRewarded(Action<bool> completed = null, string providerId = null)
        {
            var keyValuePairs = Instance._providerVsSettings.Where(p =>
                (string.IsNullOrEmpty(providerId) || p.Value.Id == providerId) && p.Value.rewardedPriority > 0 &&
                p.Key.IsRewardedAvailable()).ToList();

            if (keyValuePairs.Count <= 0)
            {
                completed?.Invoke(false);
                return;
            }

            var adsProvider = GetRandomWithPriority(keyValuePairs.Select(p => p.Key),
                keyValuePairs.Select(p => p.Value.rewardedPriority));
            adsProvider.ShowRewarded(completed);
        }


        public bool IsRewardedAvailable(string providerId = null)
        {
            return Instance._providerVsSettings
                .Where(p => (string.IsNullOrEmpty(providerId) || p.Value.Id == providerId) &&
                            p.Value.rewardedPriority > 0).Any(p => p.Key.IsRewardedAvailable());


        }




        public bool IsInterstitialAvailable(string providerId = null)
        {
            return Instance._providerVsSettings.Where(p => p.Value.interstitialPriority > 0)
                .Any(p => p.Key.IsInterstitialAvailable());
        }


        private static T GetRandomWithPriority<T>(IEnumerable<T> items, IEnumerable<float> probabilities)
        {
            var itemList = items.ToList();
            var probabilitiesList = probabilities.ToList();

            var total = probabilitiesList.Sum();

            var rand = Random.Range(0, total);

            var elapsed = 0f;

            var item = itemList.First();
            for (var index = 0; index < probabilitiesList.Count; index++)
            {
                var p = probabilitiesList[index];
                elapsed += p;

                if (elapsed >= rand)
                {
                    item = itemList[index];
                    break;
                }
            }

            return item;
        }
    }


    public partial class AdsManager
    {
        private static int AdsPassLeftCount
        {
            get
            {
                if (!PlayerPrefs.HasKey(nameof(AdsPassLeftCount)))
                {
                    SetForNextAds();
                }

                return PlayerPrefs.GetInt(nameof(AdsPassLeftCount));
            }
            set => PlayerPrefs.SetInt(nameof(AdsPassLeftCount), value);
        }


        public void ShowOrPassInterstitialIfCan(out bool showing)
        {
            if (!EnableAds)
            {
                showing = false;
                return;
            }

            if (AdsPassLeftCount <= 0 && IsInterstitialAvailable())
            {
                ShowAdsIfPassedIfCan();
                showing = true;
            }
            else
            {
                PassInterstitialIfCan();
                showing = false;
            }
        }

        public void ShowAdsIfPassedIfCan()
        {
            if (!EnableAds)
                return;
            if (AdsPassLeftCount <= 0 && IsInterstitialAvailable())
            {
                ShowInterstitial();
                SetForNextAds();
            }
        }

        private static void SetForNextAds()
        {
            AdsPassLeftCount =
                Random.Range(AdsSettings.Default.MinAndMaxGameOversBetweenInterstitialAds.x,
                    AdsSettings.Default.MinAndMaxGameOversBetweenInterstitialAds.y + 1);
        }

        public void PassInterstitialIfCan()
        {
            if (!EnableAds)
                return;

            AdsPassLeftCount = Mathf.Max(AdsPassLeftCount - 1, 0);
        }
    }


    public interface IAdsManager : IInitializable
    {
        bool HaveSetupConsent { get; }
        bool ConsentActive { get; set; }
        void ShowOrPassInterstitialIfCan(out bool showing);
        void PassInterstitialIfCan();
        bool IsInterstitialAvailable(string providerId = null);
        bool IsRewardedAvailable(string providerId = null);
        void ShowInterstitial(string providerId = null);
        void ShowRewarded(Action<bool> onComplete, string providerId = null);

    }

    public static class AdsManagerExtension
    {
        public static void ShowOrPassInterstitialIfCan(this IAdsManager manager) =>
            manager.ShowOrPassInterstitialIfCan(out _);
    }
}