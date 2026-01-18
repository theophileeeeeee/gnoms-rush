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
    public class Wave
    {
        public List<SpawnInfo> enemies = new List<SpawnInfo>();
        public float delayAfterWave = 2f;
    }

    public List<Wave> waves = new List<Wave>();

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

            foreach (var info in wave.enemies)
            {
                StartCoroutine(SpawnEnemy(info));
            }

            yield return new WaitForSeconds(wave.delayAfterWave);
            currentWaveIndex++;
        }
    }

    IEnumerator SpawnEnemy(SpawnInfo info)
    {
        yield return new WaitForSeconds(info.delay);

        GameObject enemy = Instantiate(info.enemyPrefab, Vector3.zero, Quaternion.identity);

        EnemyMovement em = enemy.GetComponent<EnemyMovement>();
        if (em != null)
        {
            if (info.allowedPaths != null && info.allowedPaths.Count > 0)
            {
                em.SetAllowedPaths(info.allowedPaths);
            }

            if (info.laneIndex >= 0)
            {
                em.AssignLane(info.laneIndex);
            }
        }
        if (em != null && em.allowedPaths != null && em.allowedPaths.Count > 0)
        {
            Path path = em.allowedPaths[0];
            em.SetSpawnPosition(path.GetPointAtDistance(0f));
        }
    }
}
