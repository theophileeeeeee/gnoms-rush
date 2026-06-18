using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class UIManager : MonoBehaviour
{
    [Header("Fade System (Direct UI)")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.2f;

    [Header("References")]
    public GameObject gameOverPanel;
    public MusicManager musicManager;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public CameraController2D cameraController;
    public Text earnedMoneyText;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    public int victoryLevel;
    public int earnedMoneyWithThisLevel;
    public bool hasTookMoneyFromThisLevel;

    [Header("Hardcore Reward UI")]
    public GameObject hardcoreBonusContainer;
    public Text hardcoreBonusText;
    public int hardcoreBonusAmount = 300;

    [Header("Starting Values")]
    public int startMoney;
    public int startHearts = 20;
    public int currentWave;
    public Text moneyText;
    public Text heartsText;

    [Header("Wave UI")]
    public Text waveText;
    public WaveSpawnerAdvanced waveSpawner;

    [Header("Detection Zone")]
    public Vector2 boxOffset;
    public Vector2 boxSize;
    public LayerMask ennemyLayer;

    [Header("Audio (SFX)")]
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip waveStartSound;
    public AudioClip pauseOpenSound;
    public AudioClip pauseCloseSound;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float pauseVolume = 0.5f;

    public AudioSource audioSource;

    public int CurrentMoney { get; private set; }
    public int CurrentHearts { get; private set; }

    private HashSet<Collider2D> trackedEnnemies = new HashSet<Collider2D>();

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        // Check if an ad is currently playing right when the scene loads
        if (AdsManager.Instance != null && AdsManager.Instance.IsShowingAd)
        {
            AudioListener.volume = 0f;
            AudioListener.pause = true;
        }

        CurrentMoney = startMoney;
        CurrentHearts = startHearts;
        UpdateUI();

        if (waveText != null && waveSpawner != null && waveSpawner.waves != null)
        {
            waveText.text = currentWave + "/" + waveSpawner.waves.Count;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(StartFadeInWithDelay());
        }
    }

    private IEnumerator StartFadeInWithDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        StartCoroutine(FadeCoroutine(1f, 0f));
    }

    public void UpdateWaveUI(int current)
    {
        currentWave = current;
        if (waveText != null && waveSpawner != null && waveSpawner.waves != null)
        {
            waveText.text = currentWave + "/" + waveSpawner.waves.Count;
        }

        if (waveStartSound != null)
            audioSource.PlayOneShot(waveStartSound, volume);
    }

    void Update()
    {
        // Maintain silence if an ad is running over the newly loaded scene
        if (AdsManager.Instance != null && AdsManager.Instance.IsShowingAd)
        {
            AudioListener.volume = 0f;
            AudioListener.pause = true;
        }

        Vector2 boxCenter = (Vector2)transform.position + boxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, ennemyLayer);

        HashSet<Collider2D> currentEnnemies = new HashSet<Collider2D>(hits);

        foreach (Collider2D ennemy in currentEnnemies)
        {
            if (!trackedEnnemies.Contains(ennemy))
            {
                if (ennemy.TryGetComponent<EnemyMovement>(out EnemyMovement em))
                {
                    if (!em.isABoss)
                        LoseHeart();
                    else
                        OnGameOver();
                }
                else if (ennemy.TryGetComponent<FlyingEnemyMovement>(out FlyingEnemyMovement fem))
                {
                    LoseHeart();
                }
                Destroy(ennemy.gameObject);
            }
        }

        trackedEnnemies.IntersectWith(currentEnnemies);
        trackedEnnemies = currentEnnemies;

        if (waveSpawner.hasAllWavesEnded &&
            waveSpawner.enemiesRemainingToSpawn <= 0 &&
            GameObject.FindGameObjectsWithTag("Ennemy").Length == 0 &&
            !gameOverPanel.activeSelf && !hasTookMoneyFromThisLevel)
        {
            Victory();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + boxOffset, boxSize);
    }

    private void UpdateUI()
    {
        moneyText.text = CurrentMoney.ToString();
        heartsText.text = CurrentHearts.ToString();
    }

    public void UseMoney(int amount)
    {
        CurrentMoney = Mathf.Max(0, CurrentMoney - amount);
        UpdateUI();
    }

    public void EarnMoney(int amount)
    {
        CurrentMoney += amount;
        UpdateUI();
    }

    public void LoseHeart()
    {
        CurrentHearts = Mathf.Max(0, CurrentHearts - 1);
        UpdateUI();

        if (CurrentHearts <= 0 && !gameOverPanel.activeSelf)
            OnGameOver();
    }

    private void OnGameOver()
    {
        if (defeatSound != null)
            audioSource.PlayOneShot(defeatSound, volume);

        gameOverPanel.SetActive(true);
        cameraController.enabled = false;

        if (waveSpawner != null)
        {
            waveSpawner.StopSpawner();
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Ennemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    private void Victory()
    {
        victoryPanel.SetActive(true);
        cameraController.enabled = false;

        float healthPercent = (float)CurrentHearts / startHearts;

        if (healthPercent >= 0.75f)
        {
            victoryLevel = 3;
            star1.SetActive(true);
            star2.SetActive(true);
            star3.SetActive(true);
        }
        else if (healthPercent >= 0.5f)
        {
            victoryLevel = 2;
            star1.SetActive(true);
            star2.SetActive(true);
            star3.SetActive(false);
        }
        else
        {
            victoryLevel = 1;
            star1.SetActive(true);
            star2.SetActive(false);
            star3.SetActive(false);
        }

        int baseMoney = 0;
        switch (victoryLevel)
        {
            case 3: baseMoney = 500; break;
            case 2: baseMoney = 350; break;
            case 1: default: baseMoney = 200; break;
        }

        int healthBonus = Mathf.RoundToInt(healthPercent * 150);
        earnedMoneyWithThisLevel = baseMoney + healthBonus;

        bool isHardcoreMode = PlayerPrefs.GetInt("HardcoreMode", 0) == 1;
        int totalGoldToGive = earnedMoneyWithThisLevel;

        if (isHardcoreMode)
        {
            totalGoldToGive += hardcoreBonusAmount;

            if (hardcoreBonusContainer != null) hardcoreBonusContainer.SetActive(true);
            if (hardcoreBonusText != null) hardcoreBonusText.text = "+ " + hardcoreBonusAmount.ToString();
        }
        else
        {
            if (hardcoreBonusContainer != null) hardcoreBonusContainer.SetActive(false);
        }

        if (victorySound != null)
            audioSource.PlayOneShot(victorySound, volume);

        earnedMoneyText.text = "+ " + earnedMoneyWithThisLevel.ToString();
        
        PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money", 0) + totalGoldToGive);
        
        hasTookMoneyFromThisLevel = true;
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (isHardcoreMode)
        {
            PlayerPrefs.SetInt("Hardcore_Passed_" + sceneName, 1);
        }

        int savedStars = PlayerPrefs.GetInt("Stars_" + sceneName, 0);
        if (victoryLevel > savedStars)
        {
            PlayerPrefs.SetInt("Stars_" + sceneName, victoryLevel);
        }

        PlayerPrefs.Save();
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Match match = Regex.Match(currentSceneName, @"\d+");

        string targetSceneName;

        if (match.Success)
        {
            string baseName = currentSceneName.Substring(0, match.Index);
            int nextNumber = int.Parse(match.Value) + 1;
            targetSceneName = baseName + nextNumber;
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            targetSceneName = NameFromIndex(nextIndex);
        }

        if (fadeCanvasGroup != null)
        {
            StartCoroutine(WaitForFadeAndLoadScene(targetSceneName));
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(targetSceneName);
        }
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            cameraController.enabled = false;
            musicManager.SetPauseEffect(true);
            if (pauseOpenSound != null)
                audioSource.PlayOneShot(pauseOpenSound, pauseVolume);
        }
        else
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            cameraController.enabled = true;
            musicManager.SetPauseEffect(false);
            if (pauseCloseSound != null)
                audioSource.PlayOneShot(pauseCloseSound, pauseVolume);
        }
    }

    public void QuitToMenu()
    {
        string menuSceneName = "Menu"; 

        if (fadeCanvasGroup != null)
        {
            StartCoroutine(WaitForFadeAndLoadScene(menuSceneName));
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }
    }

    private IEnumerator WaitForFadeAndLoadScene(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            yield return StartCoroutine(FadeCoroutine(0f, 1f));
        }
        
        Time.timeScale = 1f;

        bool isRetry = (sceneName == SceneManager.GetActiveScene().name || sceneName == NameFromIndex(SceneManager.GetActiveScene().buildIndex));

        if (AdsManager.Instance != null && !isRetry)
        {
            AdsManager.Instance.AttemptShowInterstitial();
            
            // CRITICAL STEP: Hold the coroutine right here until the ad is closed!
            // This prevents the new scene from firing up its audio systems prematurely.
            while (AdsManager.Instance.IsShowingAd)
            {
                yield return null;
            }
        }
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        fadeCanvasGroup.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Mathf.Min(Time.unscaledDeltaTime, 0.03f); 
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        int dot = path.LastIndexOf('.');
        return path.Substring(slash + 1, dot - slash - 1);
    }
}