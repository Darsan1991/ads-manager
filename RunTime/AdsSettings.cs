using System;
using DGames.Essentials.Attributes;
using DGames.Essentials.Unity;
using UnityEngine;

namespace DGames.Ads
{
    [DashboardMessage("You can setup full Ads settings here. Check for appropriate tabs for settings")]
    [DashboardResourceItem(path: "Settings")]
    public partial class AdsSettings : ScriptableObject
    {

        [Tab("Basic", isDefault: true, allowAll: true)] [SerializeField]
        private Vector2Int _minAndMaxGameOversBetweenInterstitialAds;


        [Tab("Admob")]
        [HelpBox("Admob Settings")]
        [Box]
        [ScriptableSymbolsToggle(nameof(AdmobSetting.enable), "ADMOB", BuildTargetGroup.Android)]
        [ToggleGroup(nameof(AdmobSetting.enable), name: "Android", true)]
        [NoLabel()]
        [UseTab(nameof(AdmobSetting.enable))]
        [SerializeField]
        private AdmobSetting _androidAdmobSetting;

        [Tab("Admob")]
        [Box]
        [ScriptableSymbolsToggle(nameof(AdmobSetting.enable), "ADMOB", BuildTargetGroup.iOS)]
        [ToggleGroup(nameof(AdmobSetting.enable), name: "iOS", true)]
        [NoLabel()]
        [UseTab(nameof(AdmobSetting.enable))]
        [SerializeField]
        private AdmobSetting _iosAdmobSetting;


        [Tab("Unity")]
        [HelpBox("Unity Ads Settings")]
        [Box]
        [ScriptableSymbolsEnum(nameof(UnityAdsSetting.sdkType), typeof(UnityAdsSdkType), "UNITY_ADS_",
            BuildTargetGroup.Android, nameof(UnityAdsSetting.enable))]
        [ToggleGroup(nameof(UnityAdsSetting.enable), name: "Android", true)]
        [NoLabel()]
        [UseTab(nameof(UnityAdsSetting.enable))]
        [SerializeField]
        private UnityAdsSetting _androidUnityAdsSetting;

        [Tab("Unity")]
        [Box]
        // [ScriptableSymbolsEnum(nameof(UnityAdsSetting.sdkType), typeof(UnityAdsSdkType), "UNITY_ADS_",
        //     BuildTargetGroup.iOS, nameof(UnityAdsSetting.enable))]
        [NoLabel()]
        [ToggleGroup(nameof(UnityAdsSetting.enable), name: "iOS", true)]
        [UseTab(nameof(UnityAdsSetting.enable))]
        [SerializeField]
        private UnityAdsSetting _iosUnityAdsSetting;

        [Tab("Consent")]
        [HelpBox("Enable/Disable Consent Panel Show Up at Start.")]
        [ToggleGroup(nameof(Ads.ConsentSetting.enable), "Consent")]
        [SerializeField]
        private ConsentSetting _consentSetting;

        public Vector2Int MinAndMaxGameOversBetweenInterstitialAds => _minAndMaxGameOversBetweenInterstitialAds;

        public AdmobSetting IOSAdmobSetting => _iosAdmobSetting;

        public AdmobSetting AndroidAdmobSetting => _androidAdmobSetting;

        public UnityAdsSetting IOSUnityAdsSetting => _iosUnityAdsSetting;

        public UnityAdsSetting AndroidUnityAdsSetting => _androidUnityAdsSetting;

        public ConsentSetting ConsentSetting => _consentSetting;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("MyGames/Settings/AdsSettings")]
        public static void Open()
        {
            ScriptableEditorUtils.OpenOrCreateDefault<AdsSettings>();
        }
#endif
    }

    public partial class AdsSettings
    {
        public static AdsSettings Default => Resources.Load<AdsSettings>(nameof(AdsSettings));

        public const string MIN_AND_MAX_GAME_OVERS_BETWEEN_INTERSTITIAL_ADS_FIELD =
            nameof(_minAndMaxGameOversBetweenInterstitialAds);

        public const string IOS_ADMOB_SETTING_FIELD = nameof(_iosAdmobSetting);
        public const string ANDROID_ADMOB_SETTING_FIELD = nameof(_androidAdmobSetting);
        public const string IOS_UNITY_ADS_SETTING_FIELD = nameof(_iosUnityAdsSetting);
        public const string ANDROID_UNITY_ADS_SETTING_FIELD = nameof(_androidUnityAdsSetting);
        public const string CONSENT_SETTINGS_FIELD = nameof(_consentSetting);
    }

    [Serializable]
    public class AdmobSetting : AdsProviderSettings
    {
        public bool enable;
        [Tab("Ids", isDefault: true)] public string interstitialId;

        [Tab("Ids")] public string admobRewardedId;

        public override string Id => "ADMOB";
    }


    [Serializable]
    public abstract class AdsProviderSettings
    {
        public abstract string Id { get; }

        [Tooltip(
            "This ads will be select with in other Ads provider ads(Unity,Admob) based on priority. Higher priority will higher change to select.")]
        [Tab("Priority")]
        public float interstitialPriority;

        [Tooltip(
            "This ads will be select with in other Ads provider ads(Unity,Admob) based on priority. Higher priority will higher change to select.")]
        [Tab("Priority")]
        public float rewardedPriority;

        [Tab("Debug")] public bool debug;
    }

    [Serializable]
    public class UnityAdsSetting : AdsProviderSettings
    {
        public bool enable;
        [Tab("Basic")] public UnityAdsSdkType sdkType;
        [Tab("Ids", isDefault: true)] public string appId;
        [Tab("Ids")] public string interstitialId;
        [Tab("Ids")] public string rewardedId;
        [Tab("Debug", "", 200000000)] public bool testMode;
        public override string Id => "UNITY";
    }

    public enum UnityAdsSdkType
    {
        WITH_MEDITATION,
        LEGACY
    }

    [Serializable]
    public struct ConsentSetting
    {
        public bool enable;
        public bool privatePolicy;
        public string privatePolicyUrl;
    }
}