using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance;

    private string _androidGameId = "6126039";
    private string _iosGameId = "6126038";
    private string _gameId;

    private string _interstitialAdUnitId;
    private string _rewardedAdUnitId;

    private bool _interstitialAdReady = false;
    private bool _rewardedAdReady = false;
    private bool _testMode = true;

    private System.Action _onRewardEarned;

    [Header("Fréquence des Pubs Interstitielles")]
    public int levelsBetweenAds = 2; 
    private int _levelCounter = 0;   

    // Public flag to tell the game if an ad is currently covering the screen
    public bool IsShowingAd { get; private set; } = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAds()
    {
#if UNITY_IOS
        _gameId = _iosGameId;
        _interstitialAdUnitId = "Interstitial_iOS";
        _rewardedAdUnitId = "Rewarded_iOS";
#else
        _gameId = _androidGameId;
        _interstitialAdUnitId = "Interstitial_Android";
        _rewardedAdUnitId = "Rewarded_Android";
#endif
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialisé avec succès");
        LoadInterstitial();
        LoadRewarded();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("Unity Ads init FAILED : " + error + " - " + message);
    }

    void LoadRewarded()
    {
        _rewardedAdReady = false;
        Advertisement.Load(_rewardedAdUnitId, this);
    }

    void LoadInterstitial()
    {
        _interstitialAdReady = false;
        Advertisement.Load(_interstitialAdUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad chargée : " + adUnitId);
        
        if (adUnitId == _rewardedAdUnitId)
            _rewardedAdReady = true;
        else if (adUnitId == _interstitialAdUnitId)
            _interstitialAdReady = true;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log("Ad FAILED : " + adUnitId + " - " + error + " - " + message);
        
        if (adUnitId == _rewardedAdUnitId)
            _rewardedAdReady = false;
        else if (adUnitId == _interstitialAdUnitId)
            _interstitialAdReady = false;
    }

    public bool IsInterstitialReady()
    {
        return Advertisement.isInitialized && _interstitialAdReady;
    }

    public bool IsRewardedReady()
    {
        return Advertisement.isInitialized && _rewardedAdReady;
    }

    public void AttemptShowInterstitial()
    {
        _levelCounter++;

        if (_levelCounter >= levelsBetweenAds)
        {
            if (IsInterstitialReady())
            {
                Debug.Log("[AdsManager] Compteur atteint, affichage de l'interstitiel.");
                _levelCounter = 0; 
                IsShowingAd = true;
                AudioListener.volume = 0f;
                AudioListener.pause = true;
                Advertisement.Show(_interstitialAdUnitId, this);
            }
            else
            {
                Debug.Log("[AdsManager] La pub était prévue mais elle n'est pas prête.");
            }
        }
        else
        {
            Debug.Log($"[AdsManager] Pas de pub ce coup-ci. Progression : {_levelCounter}/{levelsBetweenAds}");
        }
    }

    public void ShowInterstitial()
    {
        IsShowingAd = true;
        AudioListener.volume = 0f;
        AudioListener.pause = true;
        Advertisement.Show(_interstitialAdUnitId, this);
    }

    public void ShowRewarded(System.Action onRewardEarned)
    {
        _onRewardEarned = onRewardEarned;
        IsShowingAd = true;
        AudioListener.volume = 0f;
        AudioListener.pause = true;
        Advertisement.Show(_rewardedAdUnitId, this);
    }

    public void OnUnityAdsShowStart(string adUnitId) 
    {
        IsShowingAd = true;
        AudioListener.volume = 0f;
        AudioListener.pause = true;
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        IsShowingAd = false;
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        if (adUnitId == _rewardedAdUnitId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            _onRewardEarned?.Invoke();
            _onRewardEarned = null;
        }

        if (adUnitId == _interstitialAdUnitId)
            LoadInterstitial();
        else if (adUnitId == _rewardedAdUnitId)
            LoadRewarded();
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) 
    {
        IsShowingAd = false;
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        if (adUnitId == _interstitialAdUnitId)
            LoadInterstitial();
        else if (adUnitId == _rewardedAdUnitId)
            LoadRewarded();
    }

    public void OnUnityAdsShowClick(string adUnitId) { }
}