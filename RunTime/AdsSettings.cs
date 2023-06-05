using System;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace DGames.Ads
{
    public partial class AdsSettings : ScriptableObject
    {
        
        [SerializeField] private Vector2Int _minAndMaxGameOversBetweenInterstitialAds;
        [SerializeField] private AdmobSetting _iosAdmobSetting;
        [SerializeField] private AdmobSetting _androidAdmobSetting;
        [SerializeField] private UnityAdsSetting _iosUnityAdsSetting;
        [SerializeField] private UnityAdsSetting _androidUnityAdsSetting;
        [SerializeField] private ConsentSetting _consentSetting;
        
        public Vector2Int MinAndMaxGameOversBetweenInterstitialAds => _minAndMaxGameOversBetweenInterstitialAds;

        public AdmobSetting IOSAdmobSetting => _iosAdmobSetting;

        public AdmobSetting AndroidAdmobSetting => _androidAdmobSetting;

        public UnityAdsSetting IOSUnityAdsSetting => _iosUnityAdsSetting;

        public UnityAdsSetting AndroidUnityAdsSetting => _androidUnityAdsSetting;

        public ConsentSetting ConsentSetting => _consentSetting;

#if UNITY_EDITOR
        [MenuItem("MyGames/Settings/AdsSettings")]
        public static void Open()
        {
            ScriptableEditorUtils.OpenOrCreateDefault<AdsSettings>();
        }
#endif
    }

    public partial class AdsSettings
    {
        public static AdsSettings Default => Resources.Load<AdsSettings>(nameof(AdsSettings));
        public const string DEFAULT_NAME = nameof(AdsSettings);

        public const string MIN_AND_MAX_GAME_OVERS_BETWEEN_INTERSTITIAL_ADS_FIELD =
            nameof(_minAndMaxGameOversBetweenInterstitialAds);

        public const string IOS_ADMOB_SETTING_FIELD = nameof(_iosAdmobSetting);
        public const string ANDROID_ADMOB_SETTING_FIELD = nameof(_androidAdmobSetting);
        public const string IOS_UNITY_ADS_SETTING_FIELD = nameof(_iosUnityAdsSetting);
        public const string ANDROID_UNITY_ADS_SETTING_FIELD = nameof(_androidUnityAdsSetting);
        public const string CONSENT_SETTINGS_FIELD = nameof(_consentSetting);
    }
    
    [System.Serializable]
    public class AdmobSetting:AdsProviderSettings
    {
        public bool enable;
        public string interstitialId;
    

        public string admobRewardedId;

        public override string Id => "ADMOB";
    }
    
    
    [Serializable]
    public abstract class AdsProviderSettings
    {
        public abstract string Id { get; }
        [Tooltip("This ads will be select with in other Ads provider ads(Unity,Admob) based on priority. Higher priority will higher change to select.")]
        public float interstitialPriority;
        [Tooltip("This ads will be select with in other Ads provider ads(Unity,Admob) based on priority. Higher priority will higher change to select.")]
        public float rewardedPriority;

        public bool debug;
    }

    [Serializable]
    public class UnityAdsSetting:AdsProviderSettings
    {
        public bool enable;
        public UnityAdsSdkType sdkType;
        public string appId;
        public string interstitialId;
        public string rewardedId;
        public bool testMode;
        public override string Id => "UNITY";
    }

    public enum UnityAdsSdkType
    {
        WITH_MEDITATION, LEGACY
    }
    
    [System.Serializable]
    public struct ConsentSetting
    {
        public bool enable;
        public bool privatePolicy;
        public string privatePolicyUrl;
    }
    
    #if UNITY_EDITOR
    
    public static class ScriptableEditorUtils
    {
        public static void OpenOrCreateDefault<T>(string defaultName = null, string childrenPath = null,
            string parentFolderPath = null) where T : ScriptableObject
        {
            defaultName ??= typeof(T).Name;

            var asset = GetOrCreate<T>(defaultName, childrenPath, parentFolderPath);

            Selection.activeObject = asset;
        }

        public static T GetOrCreate<T>(string name = "", string childrenPath = "", string parentFolder = "")
            where T : ScriptableObject
        {
            name = string.IsNullOrEmpty(name) ? typeof(T).Name : name;
            var appendPath = string.IsNullOrEmpty(childrenPath) ? "" : $"/{childrenPath}";
            var folder =
                (string.IsNullOrEmpty(parentFolder) ? $"Assets/Resources" : $"Assets/{parentFolder}/Resources") +
                appendPath;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            return asset;
        }
    }
    
    #endif
}