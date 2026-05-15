using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions; // Nécessaire pour manipuler le nom de la scène

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public CameraController2D cameraController;
    public Text earnedMoneyText;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    public int victoryLevel;
    public int earnedMoneyWithThisLevel;

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

    earnedMoneyWithThisLevel =
        (victoryLevel * 33 + 1) -
        ((startHearts - CurrentHearts) * 5);

    earnedMoneyText.text = "+ " + earnedMoneyWithThisLevel.ToString();
}

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Récupère le nom de la scène actuelle, incrémente le chiffre à la fin et charge la suivante.
    /// Exemple : "Niveau1" -> "Niveau2"
    /// </summary>
    public void NextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // On cherche le nombre à la fin du nom via Regex
        Match match = Regex.Match(currentSceneName, @"\d+");

        if (match.Success)
        {
            // On récupère le texte avant le chiffre (ex: "Niveau")
            string baseName = currentSceneName.Substring(0, match.Index);
            
            // On convertit le chiffre trouvé, on ajoute 1
            int currentNumber = int.Parse(match.Value);
            int nextNumber = currentNumber + 1;

            // On recompose et charge (ex: "Niveau" + 2)
            string nextSceneName = baseName + nextNumber;
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Fail-safe : si pas de chiffre, on utilise l'index par défaut
            Debug.LogWarning("Aucun chiffre trouvé dans le nom de la scène, utilisation de l'index.");
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
        }
        else
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            cameraController.enabled = true;
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
}