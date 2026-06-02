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

    private bool _rewardedAdReady = false;
    private bool _testMode = true;

    private System.Action _onRewardEarned;

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
        Advertisement.Load(_interstitialAdUnitId, this);
    }

public void OnUnityAdsAdLoaded(string adUnitId)
{
    Debug.Log("Ad chargée : " + adUnitId);
    if (adUnitId == _rewardedAdUnitId)
        _rewardedAdReady = true;
}

public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
{
    Debug.Log("Ad FAILED : " + adUnitId + " - " + error + " - " + message);
    if (adUnitId == _rewardedAdUnitId)
        _rewardedAdReady = false;
}

    public bool IsRewardedReady()
    {
        return Advertisement.isInitialized && _rewardedAdReady;
    }

    public void ShowInterstitial()
    {
        Advertisement.Show(_interstitialAdUnitId, this);
    }

    public void ShowRewarded(System.Action onRewardEarned)
    {
        _onRewardEarned = onRewardEarned;
        Advertisement.Show(_rewardedAdUnitId, this);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
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

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) { }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }
}