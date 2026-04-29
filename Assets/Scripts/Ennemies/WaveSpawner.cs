using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WaveSpawnerAdvanced : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject enemyPrefab;
        public List<Path> allowedPaths;
        public int laneIndex = -1;
        public float delay = 0f;
    }

    [System.Serializable]
    public class Salve
    {
        public List<SpawnInfo> enemies = new List<SpawnInfo>();
        public float delayAfterSalve = 1f;
    }

    [System.Serializable]
    public class Wave
    {
        public List<Salve> salves = new List<Salve>();
        public float delayAfterWave = 3f;
    }

    [System.Serializable]
    public class LaunchButton
    {
        public Image image1;
        public Image image2;
        public Image fillImage;
    }

    public List<Wave> waves = new List<Wave>();
    public UIManager uiManager;
    public List<LaunchButton> launchButtons;
    public float firstWaveDelay = 5f;
    public bool hasAllWavesEnded = false;
    public int enemiesRemainingToSpawn = 0;

    bool waveReady = false;
    bool countdownActive = false;
    int currentWaveIndex = 0;

    void Start()
    {
        SetAllFills(0f);
        SetAllAlpha(0f);
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        StartCoroutine(FadeButtons(0f, 1f));
        yield return StartCoroutine(CountdownFill(firstWaveDelay));
        countdownActive = false;
        SetAllFills(0f);
        yield return StartCoroutine(FadeButtons(1f, 0f));

        while (currentWaveIndex < waves.Count)
        {
            Wave wave = waves[currentWaveIndex];

            if (uiManager != null)
                uiManager.UpdateWaveUI(currentWaveIndex + 1);

            foreach (var salve in wave.salves)
            {
                foreach (var info in salve.enemies)
                {
                    enemiesRemainingToSpawn++;
                    StartCoroutine(SpawnEnemy(info));
                }
                yield return new WaitForSeconds(salve.delayAfterSalve);
            }

            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count) break;

            float countdown = wave.delayAfterWave;

            StartCoroutine(FadeButtons(0f, 1f));
            yield return StartCoroutine(CountdownFill(countdown));
            countdownActive = false;
            SetAllFills(0f);
            yield return StartCoroutine(FadeButtons(1f, 0f));
        }

        hasAllWavesEnded = true;
    }

    IEnumerator CountdownFill(float duration)
    {
        countdownActive = true;
        waveReady = false;
        float elapsed = 0f;

        while (elapsed < duration && !waveReady)
        {
            elapsed += Time.deltaTime;
            SetAllFills(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        SetAllFills(1f);
    }

    public void LaunchWave()
    {
        if (countdownActive && !waveReady)
            waveReady = true;
    }

    IEnumerator FadeButtons(float from, float to)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAllAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetAllAlpha(to);
    }

    void SetAllAlpha(float a)
    {
        foreach (var btn in launchButtons)
        {
            if (btn.image1 != null) { Color c = btn.image1.color; c.a = a; btn.image1.color = c; }
            if (btn.image2 != null) { Color c = btn.image2.color; c.a = a; btn.image2.color = c; }
            if (btn.fillImage != null) { Color c = btn.fillImage.color; c.a = a; btn.fillImage.color = c; }
        }
    }

    void SetAllFills(float t)
    {
        foreach (var btn in launchButtons)
            if (btn.fillImage != null)
                btn.fillImage.fillAmount = t;
    }

    IEnumerator SpawnEnemy(SpawnInfo info)
    {
        yield return new WaitForSeconds(info.delay);
        enemiesRemainingToSpawn--;

        GameObject enemy = Instantiate(info.enemyPrefab, Vector3.zero, Quaternion.identity);
        EnemyMovement em = enemy.GetComponent<EnemyMovement>();

        if (em != null)
        {
            if (info.allowedPaths != null && info.allowedPaths.Count > 0)
                em.SetAllowedPaths(info.allowedPaths);

            if (info.laneIndex >= 0)
                em.AssignLane(info.laneIndex);

            if (em.allowedPaths != null && em.allowedPaths.Count > 0)
                em.SetSpawnPosition(em.allowedPaths[0].GetPointAtDistance(0f));
        }
    }
}