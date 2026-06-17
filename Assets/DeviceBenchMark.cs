using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceBenchmark : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject projectilePrefab;
    public ParticleSystem effectPrefab;

    public int enemyCount = 40;
    public int projectileCount = 20;
    public int effectCount = 5;

    public int warmupFrames = 20;
    public int sampleFrames = 90;

    public float lowThreshold = 35f;
    public float highThreshold = 70f;

    public bool IsDone { get; private set; }

    private void Start()
    {
        StartCoroutine(RunBenchmark());
    }

    private IEnumerator RunBenchmark()
    {
        if (PlayerPrefs.HasKey("QualityLevel"))
        {
            IsDone = true;
            yield break;
        }

        var spawned = new List<GameObject>();

        for (int i = 0; i < enemyCount; i++)
        {
            var e = Instantiate(enemyPrefab, new Vector3((i % 10) * 0.6f, (i / 10) * 0.6f, 0), Quaternion.identity);
            spawned.Add(e);
        }

        for (int i = 0; i < projectileCount; i++)
        {
            var p = Instantiate(projectilePrefab, new Vector3((i % 10) * 0.4f, -1f - (i / 10) * 0.4f, 0), Quaternion.identity);
            spawned.Add(p);
        }

        for (int i = 0; i < effectCount; i++)
        {
            var fx = Instantiate(effectPrefab, new Vector3(i * 1.5f, 0, 0), Quaternion.identity);
            spawned.Add(fx.gameObject);
        }

        for (int i = 0; i < warmupFrames; i++)
            yield return null;

        var samples = new List<float>(sampleFrames);
        for (int i = 0; i < sampleFrames; i++)
        {
            yield return null;
            samples.Add(1f / Time.unscaledDeltaTime);
        }

        foreach (var go in spawned)
            Destroy(go);

        samples.Sort();

        float minFps = samples[0];
        float maxFps = samples[samples.Count - 1];
        float medianFps = samples[samples.Count / 2];
        float p25Fps = samples[Mathf.FloorToInt(samples.Count * 0.25f)];
        float p75Fps = samples[Mathf.FloorToInt(samples.Count * 0.75f)];

        float avgFps = 0f;
        foreach (float s in samples) avgFps += s;
        avgFps /= samples.Count;

        float trimmedAvg = 0f;
        int trimStart = Mathf.FloorToInt(samples.Count * 0.1f);
        int trimEnd = Mathf.FloorToInt(samples.Count * 0.9f);
        for (int i = trimStart; i < trimEnd; i++) trimmedAvg += samples[i];
        trimmedAvg /= (trimEnd - trimStart);

        float score = trimmedAvg * 0.6f + p25Fps * 0.4f;

        int level;
        string levelName;
        if (score < lowThreshold) { level = 0; levelName = "Faible (ex: A14)"; }
        else if (score < highThreshold) { level = 1; levelName = "Moyen (ex: A70)"; }
        else { level = 2; levelName = "Élevé (ex: S24)"; }

        PlayerPrefs.SetInt("QualityLevel", level);
        PlayerPrefs.SetInt("BenchmarkReady", 1);
        PlayerPrefs.Save();

        Debug.Log(
            "=== Benchmark Appareil ===\n" +
            $"Appareil : {SystemInfo.deviceModel}\n" +
            $"OS : {SystemInfo.operatingSystem}\n" +
            $"CPU : {SystemInfo.processorType} ({SystemInfo.processorCount} cœurs)\n" +
            $"RAM : {SystemInfo.systemMemorySize} Mo\n" +
            $"GPU : {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} Mo)\n" +
            $"Résolution écran : {Screen.currentResolution.width}x{Screen.currentResolution.height}\n" +
            $"Charge : {enemyCount} ennemis / {projectileCount} projectiles / {effectCount} effets\n" +
            $"Échantillons : {sampleFrames} (warmup {warmupFrames})\n" +
            $"FPS min/p25/médian/moyen/p75/max : {minFps:F1} / {p25Fps:F1} / {medianFps:F1} / {avgFps:F1} / {p75Fps:F1} / {maxFps:F1}\n" +
            $"Trimmed average (10-90%) : {trimmedAvg:F1}\n" +
            $"Score final (60% trimmed + 40% p25) : {score:F1}\n" +
            $"Seuils (low/high) : {lowThreshold} / {highThreshold}\n" +
            $"=> Niveau de qualité retenu : {level} ({levelName})"
        );

        IsDone = true;
    }
}