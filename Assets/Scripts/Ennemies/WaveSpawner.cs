using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class WaveSpawnerAdvanced : MonoBehaviour
{
    public enum PopupDirection { Up, Down, Left, Right }

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
        public List<Path> requiredPaths;
        public PopupDirection popupDirection = PopupDirection.Up;
    }

    public List<Wave> waves = new List<Wave>();
    public UIManager uiManager;
    public List<LaunchButton> launchButtons;
    public bool hasAllWavesEnded = false;
    public int enemiesRemainingToSpawn = 0;
    public int coinsPerSecondEarly = 1;
    public GameObject coinPopupPrefab;
    public float popupOffsetDistance = 80f;

    bool waveReady = false;
    bool countdownActive = false;
    int currentWaveIndex = 0;
    float countdownTimeRemaining = 0f;

    void Start()
    {
        SetAllFills(0f);
        SetAllAlpha(0f);
        StartCoroutine(SpawnLoop());
    }

    bool WaveContainsPath(Wave wave, Path path)
    {
        foreach (var salve in wave.salves)
            foreach (var info in salve.enemies)
                if (info.allowedPaths != null && info.allowedPaths.Contains(path))
                    return true;
        return false;
    }

    bool ButtonMatchesWave(LaunchButton btn, Wave wave)
    {
        if (btn.requiredPaths == null || btn.requiredPaths.Count == 0)
            return true;
        foreach (var path in btn.requiredPaths)
            if (WaveContainsPath(wave, path))
                return true;
        return false;
    }

    IEnumerator SpawnLoop()
    {
        if (waves == null || waves.Count == 0)
        {
            hasAllWavesEnded = true;
            yield break;
        }

        Wave firstWave = waves[0];

        foreach (var btn in launchButtons)
            if (ButtonMatchesWave(btn, firstWave))
                StartCoroutine(FadeButton(btn, 0f, 1f));

        foreach (var btn in launchButtons)
            if (btn.fillImage != null && ButtonMatchesWave(btn, firstWave))
                btn.fillImage.fillAmount = 1f;

        countdownActive = true;
        waveReady = false;
        countdownTimeRemaining = 0f;
        yield return new WaitUntil(() => waveReady);
        countdownActive = false;

        SetAllFills(0f);
        FadeMatchingButtons(1f, 0f, firstWave);
        yield return new WaitForSeconds(0.5f);

        while (currentWaveIndex < waves.Count)
        {
            Wave wave = waves[currentWaveIndex];

            if (uiManager != null)
                uiManager.UpdateWaveUI(currentWaveIndex + 1);

            int totalWaves = PlayerPrefs.GetInt("TotalWavesCleared", 0);
            PlayerPrefs.SetInt("TotalWavesCleared", totalWaves + 1);
            PlayerPrefs.Save();

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

            Wave nextWave = waves[currentWaveIndex];
            float countdown = wave.delayAfterWave;

            FadeMatchingButtons(0f, 1f, nextWave);
            yield return StartCoroutine(CountdownFill(countdown, nextWave));
            countdownActive = false;
            SetAllFills(0f);
            FadeMatchingButtons(1f, 0f, nextWave);
            yield return new WaitForSeconds(0.5f);
        }

        hasAllWavesEnded = true;
    }

    void FadeMatchingButtons(float from, float to, Wave wave)
    {
        foreach (var btn in launchButtons)
        {
            if (ButtonMatchesWave(btn, wave))
                StartCoroutine(FadeButton(btn, from, to));
            else
                StartCoroutine(FadeButton(btn, btn.image1 != null ? btn.image1.color.a : 0f, 0f));
        }
    }

    IEnumerator CountdownFill(float duration, Wave wave)
    {
        countdownActive = true;
        waveReady = false;
        float elapsed = 0f;
        countdownTimeRemaining = duration;

        while (elapsed < duration && !waveReady)
        {
            elapsed += Time.deltaTime;
            countdownTimeRemaining = duration - elapsed;
            float t = Mathf.Clamp01(elapsed / duration);
            foreach (var btn in launchButtons)
                if (btn.fillImage != null && ButtonMatchesWave(btn, wave))
                    btn.fillImage.fillAmount = t;
            yield return null;
        }

        countdownTimeRemaining = 0f;

        foreach (var btn in launchButtons)
            if (btn.fillImage != null && ButtonMatchesWave(btn, wave))
                btn.fillImage.fillAmount = 1f;
    }

    public void LaunchWave()
    {
        LaunchButton fallback = null;
        foreach (var btn in launchButtons)
        {
            if (btn.image1 != null && btn.image1.color.a > 0f)
            {
                fallback = btn;
                break;
            }
        }
        LaunchWaveInternal(fallback);
    }

    public void LaunchWave(int buttonIndex)
    {
        LaunchButton btn = null;
        if (launchButtons != null && buttonIndex >= 0 && buttonIndex < launchButtons.Count)
            btn = launchButtons[buttonIndex];
        LaunchWaveInternal(btn);
    }

    void LaunchWaveInternal(LaunchButton btn)
    {
        if (TutorialManager.Instance != null && !TutorialManager.Instance.ReinforcementsPlaced)
        {
        Debug.Log("Action impossible : Vous devez d'abord placer les renforts !");
        return; 
        }
        if (countdownActive && !waveReady)
        {
            int bonus = Mathf.FloorToInt(countdownTimeRemaining) * coinsPerSecondEarly;
            if (bonus > 0)
            {
                if (uiManager != null)
                    uiManager.EarnMoney(bonus);
                SpawnCoinPopup(btn, bonus);
            }
            waveReady = true;
        }
    }

void SpawnCoinPopup(LaunchButton btn, int amount)
{
    if (coinPopupPrefab == null || btn == null || btn.image1 == null)
        return;

    Transform buttonTransform = btn.image1.transform;

    Transform canvasParent = buttonTransform;
    while (canvasParent.parent != null && canvasParent.GetComponent<Canvas>() == null)
        canvasParent = canvasParent.parent;

    GameObject anchor = new GameObject("PopupAnchor");
    anchor.transform.SetParent(canvasParent, false);

    RectTransform anchorRect = anchor.AddComponent<RectTransform>();
    RectTransform buttonRect = btn.image1.GetComponent<RectTransform>();

    if (buttonRect != null)
    {
        Vector2 buttonAnchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasParent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, buttonRect.position),
            null,
            out buttonAnchoredPos
        );

        Vector2 offset = Vector2.zero;
        switch (btn.popupDirection)
        {
            case PopupDirection.Up:    offset = new Vector2(0f,  popupOffsetDistance); break;
            case PopupDirection.Down:  offset = new Vector2(0f, -popupOffsetDistance); break;
            case PopupDirection.Left:  offset = new Vector2(-popupOffsetDistance, 0f); break;
            case PopupDirection.Right: offset = new Vector2( popupOffsetDistance, 0f); break;
        }

        anchorRect.anchoredPosition = buttonAnchoredPos + offset;
    }

    GameObject popup = Instantiate(coinPopupPrefab, anchor.transform);
    RectTransform popupRect = popup.GetComponent<RectTransform>();
    if (popupRect != null)
    {
        popupRect.anchoredPosition = Vector2.zero;
        popupRect.localScale = Vector3.one;
    }

    TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
    if (popupText != null)
        popupText.text = "+" + amount;
}
    IEnumerator FadeButton(LaunchButton btn, float from, float to)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / duration);
            SetButtonAlpha(btn, a);
            yield return null;
        }

        SetButtonAlpha(btn, to);
    }

    void SetButtonAlpha(LaunchButton btn, float a)
    {
        if (btn.image1 != null) { Color c = btn.image1.color; c.a = a; btn.image1.color = c; }
        if (btn.image2 != null) { Color c = btn.image2.color; c.a = a; btn.image2.color = c; }
        if (btn.fillImage != null) { Color c = btn.fillImage.color; c.a = a; btn.fillImage.color = c; }
    }

    void SetAllFills(float t)
    {
        foreach (var btn in launchButtons)
            if (btn.fillImage != null)
                btn.fillImage.fillAmount = t;
    }

    void SetAllAlpha(float a)
    {
        foreach (var btn in launchButtons)
            SetButtonAlpha(btn, a);
    }

    IEnumerator SpawnEnemy(SpawnInfo info)
    {
        yield return new WaitForSeconds(info.delay);
        enemiesRemainingToSpawn--;

        GameObject enemy = Instantiate(info.enemyPrefab, Vector3.zero, Quaternion.identity);

        EnemyMovement em = enemy.GetComponent<EnemyMovement>();
        FlyingEnemyMovement fem = enemy.GetComponent<FlyingEnemyMovement>();

        if (em != null)
        {
            if (info.allowedPaths != null && info.allowedPaths.Count > 0)
                em.SetAllowedPaths(info.allowedPaths);

            if (info.laneIndex >= 0)
                em.AssignLane(info.laneIndex);

            if (info.allowedPaths != null && info.allowedPaths.Count > 0)
                em.SetSpawnPosition(info.allowedPaths[0].GetPointAtDistance(0f));
        }
        else if (fem != null)
        {
            if (info.allowedPaths != null && info.allowedPaths.Count > 0)
                fem.SetAllowedPaths(info.allowedPaths);

            if (info.laneIndex >= 0)
                fem.AssignLane(info.laneIndex);

            if (info.allowedPaths != null && info.allowedPaths.Count > 0)
                fem.SetSpawnPosition(info.allowedPaths[0].GetPointAtDistance(0f));
        }
    }

    public void StopSpawner()
    {
        StopAllCoroutines();
        countdownActive = false;
        waveReady = false;

        foreach (var btn in launchButtons)
        {
            SetButtonAlpha(btn, 0f);
            if (btn.image1 != null && btn.image1.gameObject != null)
            {
                Button b = btn.image1.GetComponentInParent<Button>();
                if (b != null) b.interactable = false;
            }
        }
    }
}