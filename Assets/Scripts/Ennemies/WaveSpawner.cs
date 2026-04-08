using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public List<Wave> waves = new List<Wave>();
    public UIManager uiManager;
    public bool hasAllWavesEnded = false;
    public int enemiesRemainingToSpawn = 0;
    int currentWaveIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
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

            yield return new WaitForSeconds(wave.delayAfterWave);
            currentWaveIndex++;
        }

        hasAllWavesEnded = true;
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
        }

        if (em != null && em.allowedPaths != null && em.allowedPaths.Count > 0)
        {
            Path path = em.allowedPaths[0];
            em.SetSpawnPosition(path.GetPointAtDistance(0f));
        }
    }
}