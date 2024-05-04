#if ADMOB
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using DGames.DDebug;
using GoogleMobileAds.Ump.Api;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace DGames.Ads
{


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
           Init();
        }

        private async void Init()
        {
            try
            {
                await AdmobConsent.GatherConsent(_debug);
                if (!AdmobConsent.CanRequestAds) return;
                
                MobileAds.Initialize(_ =>
                {
                    RequestInterstitial();
                    RequestRewardVideo();
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        }

        public bool IsInterstitialAvailable()
        {
            return _interstitialAd != null;
        }

        public bool IsRewardedAvailable()
        {
            return _rewardBaseVideo != null;
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
            _rewardBaseVideo.Show(_ =>
            {
                _rewardVideoAdsCallback?.Invoke(true);
                _rewardVideoAdsCallback = null;
            });
        }

        private void RequestRewardVideo()
        {
            _rewardBaseVideo = null;

            RewardedAd.Load(_rewardedId, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    DebugIfCan($"Admob - Reward Ads Failed to Load:{error}");
                    SimpleCoroutine.Create().Delay(60, RequestRewardVideo);
                    return;
                }

                OnRewardAdsLoaded(ad);
            });
        }

        private void OnRewardAdsLoaded(RewardedAd ad)
        {
            _rewardBaseVideo = ad;
            DebugIfCan("Admob - Reward Ads Loaded.");

            _rewardBaseVideo.OnAdFullScreenContentFailed += _ =>
            {
                _rewardVideoAdsCallback?.Invoke(false);
                _rewardVideoAdsCallback = null;
            };

            _rewardBaseVideo.OnAdFullScreenContentClosed += () =>
            {
                DebugIfCan("Admob - Reward Ads Closed.");
                RequestRewardVideo();
            };
            DebugIfCan("Admob - Reward Ads Loaded.");
        }

        private void DebugIfCan(string message)
        {
            if (_debug)
            {
                UDebug.Debug(message);
            }
        }



        private void RequestInterstitial()
        {
            _interstitialAd = null;
            InterstitialAd.Load(_interstitialId, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    DebugIfCan($"Admob - Interstitial Load Fail : {error}");
                    SimpleCoroutine.Create().Delay(60, RequestInterstitial);
                    return;
                }

                OnInterstitialLoaded(ad);
            });
        }

        private void OnInterstitialLoaded(InterstitialAd ad)
        {
            _interstitialAd = ad;
            DebugIfCan("Admob - Interstitial Loaded.");

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                DebugIfCan("Admob - Interstitial Closed.");
                RequestInterstitial();
            };
        }


    }

    public static class AdmobConsent
    {

        public static bool CanRequestAds => ConsentInformation.CanRequestAds();
        
        
        public static Task GatherConsent(bool debug = false)
        {
            var consentRequestParameters = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = debug ? DebugGeography.EEA : DebugGeography.Disabled,
                    TestDeviceHashedIds = new List<string>(),
                }
            };
            var tcs = new TaskCompletionSource<bool>();

            ConsentInformation.Update(consentRequestParameters, OnConsentInfoUpdated);
            
            return tcs.Task;

            async void OnConsentInfoUpdated(FormError error)
            {
                if (error != null)
                {
                    tcs.SetException(new Exception(error.Message));
                    return;
                }

                if (CanRequestAds)
                {
                    tcs.SetResult(true);
                    return;
                }

                await LoadAndShowConsentIfRequired();
                tcs.SetResult(true);
            }
        }

        private static Task  LoadAndShowConsentIfRequired()
        {
            var tcs = new TaskCompletionSource<bool>();
            ConsentForm.LoadAndShowConsentFormIfRequired(error=>
            {
                if (error != null)
                {
                    tcs.SetException(new Exception(error.Message));
                    return;
                }
                tcs.SetResult(true);
                
            });

            return tcs.Task;
        }
    }



#if UNITY_EDITOR

    public static class AdmobExtensions
    {
        [MenuItem("Assets/External Dependency Manager/"+nameof(DeletePluginsFolder))]
        public static void DeletePluginsFolder()
        {
            DeleteFolders(new []{"Plugins/Android","Plugins/iOS"});
        }

        [MenuItem("MyGames/Reset Consent")]
        public static void ResetConsent()
        {
            ConsentInformation.Reset();
        }
        
        private static void DeleteFolders(string[] folders)
        {
            foreach (var asset in AssetDatabase.FindAssets("", folders.Select(f => Path.Combine("Assets", f)).ToArray()))
            {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                AssetDatabase.DeleteAsset(p);
            }

            foreach (var path in folders.Select(p => $"{Path.Combine(Application.dataPath, p)}.meta").Where(File.Exists))
            {
                File.Delete(path);
            }

            foreach (var path in folders.Select(p => Path.Combine(Application.dataPath, p)).Where(Directory.Exists))
            {
                Directory.Delete(path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    #endif
}

#endif