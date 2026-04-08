using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;

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

    public int CurrentMoney { get; private set; }
    public int CurrentHearts { get; private set; }

    private HashSet<Collider2D> trackedEnnemies = new HashSet<Collider2D>();

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
            !gameOverPanel.activeSelf)
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

        if (CurrentHearts <= 0)
            OnGameOver();
    }

    private void OnGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void Victory()
    {
        victoryPanel.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}