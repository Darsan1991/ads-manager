using UnityEngine;
using UnityEngine.UI;

namespace DGames.Ads.Sample
{
    public class ConsentPanel : ShowHidable
    {

        [SerializeField] private Button _policyBtn;

        private IAdsManager _adsManager;

        public IAdsManager AdsManager => _adsManager ??= Services.Get<IAdsManager>();

        void Awake()
        {
            _policyBtn.gameObject.SetActive(AdsSettings.Default.ConsentSetting.privatePolicy);
        }



        public void OnClickYes()
        {
            AdsManager.ConsentActive = true;
            AdsManager.Init();
            Hide();
        }


        public void OnClickPrivacy()
        {
            Application.OpenURL(AdsSettings.Default.ConsentSetting.privatePolicyUrl);
        }

        public void OnClickNo()
        {
            AdsManager.ConsentActive = false;
            AdsManager.Init();
            Hide();
        }
    }

}