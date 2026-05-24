using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class UIManager : MonoBehaviour
{
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

    [Header("Starting Values")]
    public int startMoney;
    public int startHearts;
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

    [Header("Audio")]
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip waveStartSound;
    public AudioClip pauseOpenSound;
    public AudioClip pauseCloseSound;
    public float volume = 1f;
    public float pauseVolume = 0.5f;

    private AudioSource audioSource;

    public int CurrentMoney { get; private set; }
    public int CurrentHearts { get; private set; }

    private HashSet<Collider2D> trackedEnnemies = new HashSet<Collider2D>();

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        CurrentMoney = startMoney;
        CurrentHearts = startHearts;
        UpdateUI();
    }

    public void UpdateWaveUI(int current)
    {
        waveText.text = current + "/" + waveSpawner.waves.Count;
        currentWave = current;

        if (waveStartSound != null)
            AudioSource.PlayClipAtPoint(waveStartSound, Camera.main.transform.position, volume);
    }

    void Update()
    {
        Vector2 boxCenter = (Vector2)transform.position + boxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, ennemyLayer);

        HashSet<Collider2D> currentEnnemies = new HashSet<Collider2D>(hits);

        foreach (Collider2D ennemy in currentEnnemies)
        {
            if (!trackedEnnemies.Contains(ennemy))
            {
                LoseHeart();
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
            AudioSource.PlayClipAtPoint(defeatSound, Camera.main.transform.position, volume);

        gameOverPanel.SetActive(true);
        cameraController.enabled = false;
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

        earnedMoneyWithThisLevel = Mathf.Max(1, (victoryLevel * 33 + 1) - ((startHearts - CurrentHearts) * 5));
        if (victorySound != null)
            AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position, volume);
        earnedMoneyText.text = "+ " + earnedMoneyWithThisLevel.ToString();
        PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money", 0) + earnedMoneyWithThisLevel);
        PlayerPrefs.Save();
        hasTookMoneyFromThisLevel = true;
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Match match = Regex.Match(currentSceneName, @"\d+");

        if (match.Success)
        {
            string baseName = currentSceneName.Substring(0, match.Index);
            int nextNumber = int.Parse(match.Value) + 1;
            SceneManager.LoadScene(baseName + nextNumber);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
}