using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public Slider loadingSlider;
    public TMP_Text percentText;
    public TMP_Text versionText;
    public TMP_Text loadingText;
    public float minDuration = 1.5f;
    public float maxDuration = 4f;
    public int versionMajor = 1;
    public int versionMinor = 0;
    public int versionPatch = 0;

    public DeviceBenchmark deviceBenchmark;

    private void Start()
    {
        versionText.text = "version " + versionMajor + "." + versionMinor + "." + versionPatch;
        StartCoroutine(Load());
        StartCoroutine(AnimateLoadingText());
    }

    private IEnumerator AnimateLoadingText()
    {
        string[] states = { "Chargement", "Chargement.", "Chargement..", "Chargement..." };
        int i = 0;
        while (true)
        {
            if (loadingText != null)
                loadingText.text = states[i % states.Length];
            i++;
            yield return new WaitForSeconds(0.4f);
        }
    }

    private IEnumerator Load()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            float step = Random.Range(0.01f, 0.08f);

            if (Random.value < 0.15f)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            }

            float speedFactor = 1f - Mathf.Pow(progress, 2f);
            progress += step * Mathf.Max(speedFactor, 0.05f);
            progress = Mathf.Clamp01(progress);

            loadingSlider.value = progress;
            percentText.text = Mathf.RoundToInt(progress * 100f) + "%";

            yield return new WaitForSeconds(Random.Range(0.02f, 0.12f));
        }

        loadingSlider.value = 1f;
        percentText.text = "100%";

        if (loadingText != null)
            loadingText.text = "Chargement...";

        if (deviceBenchmark != null)
        {
            while (!deviceBenchmark.IsDone)
                yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        Debug.Log("Chargement vers Menu");
        SceneManager.LoadScene("Menu");
    }
}