using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public enum RewardResourceType { Gold, Dynamite, IceOrb }

[System.Serializable]
public struct DailyRewardVisuals
{
    public string dayName; 
    public RewardResourceType resourceType;
    public int amount;
    
    [Header("UI Components")]
    public Button dayButton;       
    public GameObject glowObject;  
    public GameObject checkIcon;   
}

[System.Serializable]
public struct Quest
{
    public string key;
    public int goal;
    public int reward;
    public Text progressText;
    public Text rewardText;
    public Slider progressSlider;
    public Button claimButton;
    public Text claimButtonText;
    public string unit;
}

[System.Serializable]
public struct LevelData
{
    public string sceneName;
    public string displayName;
    public Sprite previewSprite;
    public Button levelButton;
    public Image buttonImage; 
    public Image starsImage;  
    public Image numberImage; 
    public GameObject lockOverlay;
    public GameObject badgeIcon;
}

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    [Header("Fade System (Direct UI)")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.4f;

    [Header("Panels")]
    public GameObject settingsPanel;
    [Range(0f, 1f)]
    public float uiVolume = 1f;
    public Slider qualitySlider;
    public GameObject shopPanel;
    public GameObject statsPanel;
    public GameObject levelsPanel;

    [Header("Récompenses Journalières")]
    public GameObject dailyRewardsPanel;
    public GameObject rewardAvailableBubble;
    public GameObject rewardGlowObject;
    public DailyRewardVisuals[] dailyRewardsList;

    [Header("Level Preview Panel")]
    public GameObject levelLaunchWindow;
    public TextMeshProUGUI levelNameText;
    public Image levelPreviewImage;
    public Button launchButton;
    public LevelStarDisplay launchStarDisplay;
    public GameObject launchHardcoreBadge;

    [Header("Progression Globale (Quêtes)")]
    public Slider completionSlider;
    public Text completionText;

    public Text[] moneyTexts;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Niveaux")]
    public LevelData[] levels;

    [Header("Music UI")]
    public Image musicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    [Header("SFX UI")]
    public Image sfxButtonImage;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;

    [Header("Bruitages UI")]
    public AudioClip panelOpenClip;
    public AudioClip panelCloseClip;
    public AudioClip questClaimClip;
    public AudioClip sliderTickClip;
    public AudioClip toggleOffClip;

    [Header("Fin de Jeu & Mode Hardcore")]
    public GameObject victoryPanel;
    public Button enableHardcoreButton;

    public Quest[] quests;

    private const float normalVolume = 0f;
    private const float mutedVolume = -80f;
    public AudioSource uiAudioSource;

    private string pendingSceneName;

    private int devClickCount = 0;
    private float devClickTimer = 0f;
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (qualitySlider != null)
        {
            qualitySlider.minValue = 0f;
            qualitySlider.maxValue = 2f;
            qualitySlider.wholeNumbers = true;
        }

        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 0);
        
        if (qualitySlider != null)
            qualitySlider.value = savedQuality;

        SetQuality(savedQuality);

        ApplyMusic(PlayerPrefs.GetInt("MusicMuted", 0) == 0);
        ApplySFX(PlayerPrefs.GetInt("SFXMuted", 0) == 0);
        UpdateMoneyUI();

        SetupQuestButtons();

        if (levelLaunchWindow != null)
            levelLaunchWindow.SetActive(false);

        if (launchButton != null)
            launchButton.onClick.AddListener(LaunchPendingLevel);

        if (enableHardcoreButton != null)
            enableHardcoreButton.onClick.AddListener(ToggleHardcoreMode);

        RefreshLevelButtons();
        CheckGameCompletion();

        CheckDailyRewardsStreak(); 

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(StartFadeInWithDelay());
        }

        isInitialized = true;
    }

    private IEnumerator StartFadeInWithDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        StartCoroutine(FadeCoroutine(1f, 0f));
    }

    void PlayUI(AudioClip clip)
    {
        if (clip != null && uiAudioSource != null)
            uiAudioSource.PlayOneShot(clip, uiVolume);
    }

    string FormatMoney(int amount)
    {
        if (amount >= 1000000) return $"{amount / 1000000f:0.#}M";
        if (amount >= 1000)    return $"{amount / 1000f:0.#}K";
        return amount.ToString();
    }

    public void RefreshLevelButtons()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            LevelData data = levels[i];
            bool unlocked = i == 0 || PlayerPrefs.GetInt("Stars_" + levels[i - 1].sceneName, 0) > 0;

            Color targetColor = unlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);

            if (data.levelButton != null)
                data.levelButton.interactable = unlocked;

            if (data.buttonImage != null)
                data.buttonImage.color = targetColor;

            if (data.starsImage != null)
                data.starsImage.color = targetColor;

            if (data.numberImage != null)
                data.numberImage.color = targetColor;

            if (data.lockOverlay != null)
                data.lockOverlay.SetActive(!unlocked);

            if (data.badgeIcon != null)
            {
                bool hasPassedHardcore = PlayerPrefs.GetInt("Hardcore_Passed_" + data.sceneName, 0) == 1;
                data.badgeIcon.SetActive(unlocked && hasPassedHardcore);
            }
        }
    }

    void CheckGameCompletion()
    {
        if (levels != null && levels.Length > 0)
        {
            string lastLevelScene = levels[levels.Length - 1].sceneName; 
            bool lastLevelBeaten = PlayerPrefs.GetInt("Stars_" + lastLevelScene, 0) > 0;
            bool totalVictoryShown = PlayerPrefs.GetInt("GameCompleted_Shown", 0) == 1;

            if (lastLevelBeaten)
            {
                PlayerPrefs.SetInt("HardcoreUnlocked", 1);

                if (!totalVictoryShown && victoryPanel != null)
                {
                    bool wasAlreadyActive = victoryPanel.activeSelf;
                    
                    victoryPanel.SetActive(true);
                    PlayerPrefs.SetInt("GameCompleted_Shown", 1);
                    PlayerPrefs.Save();
                    
                    if (!wasAlreadyActive)
                    {
                        PlayUI(panelOpenClip);
                    }
                }
            }
        }
    }

    public void CloseVictoryPanel()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        PlayUI(panelCloseClip);
    }

    public void ToggleHardcoreMode()
    {
        PlayerPrefs.SetInt("HardcoreMode", 1);
        PlayerPrefs.Save();

        CloseVictoryPanel();

        HardcoreToggleController toggle = FindObjectOfType<HardcoreToggleController>(true);
        if (toggle != null)
        {
            toggle.NotifyUnlockAndRefresh();
        }
    }

    public void SetHardcoreModeDirect(bool active)
    {
        PlayerPrefs.SetInt("HardcoreMode", active ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OpenLevelPreview(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        bool unlocked = levelIndex == 0 || PlayerPrefs.GetInt("Stars_" + levels[levelIndex - 1].sceneName, 0) > 0;
        if (!unlocked) return;

        LevelData data = levels[levelIndex];
        pendingSceneName = data.sceneName;

        if (levelNameText != null)
            levelNameText.text = data.displayName;

        if (levelPreviewImage != null && data.previewSprite != null)
            levelPreviewImage.sprite = data.previewSprite;

        if (launchStarDisplay != null)
        {
            launchStarDisplay.sceneName = data.sceneName;
            launchStarDisplay.Refresh();
        }

        if (launchHardcoreBadge != null)
        {
            bool hasPassedHardcore = PlayerPrefs.GetInt("Hardcore_Passed_" + data.sceneName, 0) == 1;
            launchHardcoreBadge.SetActive(hasPassedHardcore);
        }

        if (levelLaunchWindow != null)
            levelLaunchWindow.SetActive(true);

        PlayUI(panelOpenClip);
    }

    public void CloseLevelPreview()
    {
        if (levelLaunchWindow != null)
            levelLaunchWindow.SetActive(false);

        pendingSceneName = null;
        PlayUI(panelCloseClip);
    }

    void LaunchPendingLevel()
    {
        if (!string.IsNullOrEmpty(pendingSceneName))
        {
            if (fadeCanvasGroup != null)
            {
                StartCoroutine(WaitForFadeAndLoadScene(pendingSceneName));
            }
            else
            {
                SceneManager.LoadScene(pendingSceneName);
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(WaitForFadeAndLoadScene(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator WaitForFadeAndLoadScene(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            yield return StartCoroutine(FadeCoroutine(0f, 1f));
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

    public void UpdateMoneyUI()
    {
        string money = FormatMoney(PlayerPrefs.GetInt("Money", 0));
        foreach (Text t in moneyTexts)
            t.text = money;
    }

    public void ToggleLevelsPanel()
    {
        bool opening = !levelsPanel.activeSelf;
        levelsPanel.SetActive(opening);
        if (opening) RefreshLevelButtons();
        PlayUI(opening ? panelOpenClip : panelCloseClip);
    }

    public void ToggleSettings()
    {
        bool opening = !settingsPanel.activeSelf;
        settingsPanel.SetActive(opening);
        PlayUI(opening ? panelOpenClip : panelCloseClip);
    }

    public void ToggleShop()
    {
        bool opening = !shopPanel.activeSelf;
        shopPanel.SetActive(opening);
        PlayUI(opening ? panelOpenClip : panelCloseClip);
    }

    public void ToggleShopPanelDirect()
    {
        ToggleShop();
    }

    public void ToggleStats()
    {
        bool opening = !statsPanel.activeSelf;
        statsPanel.SetActive(opening);
        PlayUI(opening ? panelOpenClip : panelCloseClip);
        if (opening)
        {
            UpdateQuests();
            UpdateCompletionPercentage();
        }
    }

    void SetupQuestButtons()
    {
        for (int i = 0; i < quests.Length; i++)
        {
            if (quests[i].claimButton != null)
            {
                int index = i;
                quests[i].claimButton.onClick.RemoveAllListeners();
                quests[i].claimButton.onClick.AddListener(() => ClaimReward(index));
            }
        }
    }

    void UpdateQuests()
    {
        for (int i = 0; i < quests.Length; i++)
        {
            Quest q = quests[i];
            int progress = PlayerPrefs.GetInt(q.key, 0);

            bool completed = progress >= q.goal;
            bool claimed = PlayerPrefs.GetInt($"Quest_{q.key}_{q.goal}_Claimed", 0) == 1;

            if (q.progressText != null)
            {
                string currentFormatted = FormatMoney(Mathf.Min(progress, q.goal));
                string goalFormatted = FormatMoney(q.goal);
                q.progressText.text = $"{currentFormatted} / {goalFormatted} {q.unit}";
            }

            if (q.rewardText != null)
                q.rewardText.text = $"+{FormatMoney(q.reward)}";

            if (q.progressSlider != null)
            {
                q.progressSlider.interactable = false;
                q.progressSlider.value = Mathf.Clamp01((float)progress / q.goal);
            }

            if (q.claimButton != null)
            {
                q.claimButton.interactable = completed && !claimed;
                if (q.claimButtonText != null)
                    q.claimButtonText.text = claimed ? "Récupéré" : "Récupérer";
            }
        }
    }

    public void ClaimReward(int questIndex)
    {
        Quest q = quests[questIndex];
        string claimedKey = $"Quest_{q.key}_{q.goal}_Claimed";

        if (PlayerPrefs.GetInt(claimedKey, 0) == 1) return;

        int progress = PlayerPrefs.GetInt(q.key, 0);
        if (progress < q.goal) return;

        int currentMoney = PlayerPrefs.GetInt("Money", 0);
        PlayerPrefs.SetInt("Money", currentMoney + q.reward);
        PlayerPrefs.SetInt(claimedKey, 1);
        PlayerPrefs.Save();

        PlayUI(questClaimClip);
        UpdateMoneyUI();

        UpdateQuests();
        UpdateCompletionPercentage();
    }

    void UpdateCompletionPercentage()
    {
        if (quests == null || quests.Length == 0)
        {
            if (completionSlider != null) completionSlider.value = 0f;
            if (completionText != null) completionText.text = "0%";
            return;
        }

        float totalRatioSum = 0f;

        foreach (Quest q in quests)
        {
            if (q.goal > 0)
            {
                int progress = PlayerPrefs.GetInt(q.key, 0);
                float questRatio = Mathf.Clamp01((float)progress / q.goal);
                totalRatioSum += questRatio;
            }
        }

        float finalCompletionRatio = totalRatioSum / quests.Length;

        if (completionSlider != null)
            completionSlider.value = finalCompletionRatio;

        if (completionText != null)
            completionText.text = Mathf.RoundToInt(finalCompletionRatio * 100f) + "%";
    }

    public void ResetStats()
    {
        PlayerPrefs.DeleteKey("Money");
        PlayerPrefs.DeleteKey("GoldSpentShop");
        PlayerPrefs.DeleteKey("TotalWavesCleared");
        PlayerPrefs.DeleteKey("QualityLevel");
        PlayerPrefs.DeleteKey("MusicMuted");
        PlayerPrefs.DeleteKey("SFXMuted");
        PlayerPrefs.DeleteKey("EnemiesKilled");
        PlayerPrefs.DeleteKey("TowersBuilt");
        PlayerPrefs.DeleteKey("TutorialDone");
        PlayerPrefs.DeleteKey("GameCompleted_Shown");
        PlayerPrefs.DeleteKey("HardcoreMode");
        PlayerPrefs.DeleteKey("HardcoreUnlocked");
        PlayerPrefs.DeleteKey("LastLoginDate");
        PlayerPrefs.DeleteKey("LoginStreak");
        PlayerPrefs.DeleteKey("RewardClaimedToday");

        if (dailyRewardsList != null)
        {
            for (int i = 0; i < dailyRewardsList.Length; i++)
            {
                PlayerPrefs.DeleteKey($"DailyReward_Claimed_Day_{i + 1}");
            }
        }

        foreach (Quest q in quests)
            PlayerPrefs.DeleteKey($"Quest_{q.key}_{q.goal}_Claimed");

        foreach (LevelData l in levels)
        {
            PlayerPrefs.DeleteKey("Stars_" + l.sceneName);
            PlayerPrefs.DeleteKey("Hardcore_Passed_" + l.sceneName);
        }

        PlayerPrefs.Save();

        if (fadeCanvasGroup != null)
        {
            StartCoroutine(WaitForFadeAndLoadScene("LoadingScene"));
        }
        else
        {
            SceneManager.LoadScene("LoadingScene");
        }
    }

    public void SetQuality(float qualityIndex)
    {
        int[] qualityMap = { 0, 2, 5 };
        int level = Mathf.Clamp(Mathf.RoundToInt(qualityIndex), 0, 2);
        QualitySettings.SetQualityLevel(qualityMap[level]);
        PlayerPrefs.SetInt("QualityLevel", level);
        PlayerPrefs.Save();
    }

    public void OnSliderChanged(float value)
    {
        if (!isInitialized) return;

        PlayUI(sliderTickClip);
        SetQuality(value);
    }

    public void ToggleMusic()
    {
        bool isOn = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        ApplyMusic(isOn);
        PlayerPrefs.SetInt("MusicMuted", isOn ? 0 : 1);
        PlayerPrefs.Save();
        PlayUI(isOn ? panelOpenClip : toggleOffClip);
    }

    public void ToggleSFX()
    {
        bool isOn = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        ApplySFX(isOn);
        PlayerPrefs.SetInt("SFXMuted", isOn ? 0 : 1);
        PlayerPrefs.Save();
        PlayUI(isOn ? panelOpenClip : toggleOffClip);
    }

    void ApplyMusic(bool isOn)
    {
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", isOn ? normalVolume : mutedVolume);
        if (musicButtonImage != null) musicButtonImage.sprite = isOn ? musicOnSprite : musicOffSprite;
    }

    void ApplySFX(bool isOn)
    {
        if (mainMixer != null) mainMixer.SetFloat("SFXVol", isOn ? normalVolume : mutedVolume);
        if (sfxButtonImage != null) sfxButtonImage.sprite = isOn ? sfxOnSprite : sfxOffSprite;
    }

    public void TriggerDevCode()
    {
        if (devClickCount == 0 || Time.time - devClickTimer > 1.5f)
        {
            devClickCount = 1;
            devClickTimer = Time.time;
        }
        else
        {
            devClickCount++;
        }

        if (devClickCount >= 3)
        {
            devClickCount = 0;
            ActivateDevMode();
        }
    }

    void ActivateDevMode()
    {
        PlayerPrefs.SetInt("Money", 1000000);

        foreach (LevelData l in levels)
        {
            if (!string.IsNullOrEmpty(l.sceneName))
            {
                PlayerPrefs.SetInt("Stars_" + l.sceneName, 3);
                PlayerPrefs.SetInt("Hardcore_Passed_" + l.sceneName, 1);
            }
        }

        PlayerPrefs.SetInt("HardcoreUnlocked", 1);
        PlayerPrefs.Save();

        UpdateMoneyUI();
        RefreshLevelButtons();

        DevMode devScript = FindObjectOfType<DevMode>();
        if (devScript != null)
        {
            devScript.OnDevModeActivated();
        }

        Debug.Log("[DevMode] 1M diamants ajoutés, niveaux et modes hardcore validés !");
    }

    void CheckDailyRewardsStreak()
    {
        string lastLoginStr = PlayerPrefs.GetString("LastLoginDate", "");
        System.DateTime today = System.DateTime.Today;

        int streak = PlayerPrefs.GetInt("LoginStreak", 1);
        bool claimedToday = PlayerPrefs.GetInt("RewardClaimedToday", 0) == 1;

        if (!string.IsNullOrEmpty(lastLoginStr))
        {
            System.DateTime lastLogin = System.DateTime.Parse(lastLoginStr);
            System.TimeSpan timePassed = today - lastLogin.Date;

            if (timePassed.Days == 1)
            {
                if (claimedToday)
                {
                    streak++;
                    claimedToday = false;
                }
            }
            else if (timePassed.Days > 1)
            {
                streak = 1;
                claimedToday = false;
            }
        }
        else
        {
            claimedToday = false;
        }

        if (dailyRewardsList != null && dailyRewardsList.Length > 0 && streak > dailyRewardsList.Length)
        {
            streak = 1;
        }

        PlayerPrefs.SetString("LastLoginDate", today.ToString());
        PlayerPrefs.SetInt("LoginStreak", streak);
        PlayerPrefs.SetInt("RewardClaimedToday", claimedToday ? 1 : 0);
        PlayerPrefs.Save();

        if (rewardAvailableBubble != null)
        {
            rewardAvailableBubble.SetActive(!claimedToday);
        }

        if (rewardGlowObject != null)
        {
            rewardGlowObject.SetActive(!claimedToday);
        }

        RefreshDailyRewardsUI();
    }

    public void ToggleDailyRewardsPanel()
    {
        bool opening = !dailyRewardsPanel.activeSelf;
        dailyRewardsPanel.SetActive(opening);
        PlayUI(opening ? panelOpenClip : panelCloseClip);

        if (opening)
        {
            RefreshDailyRewardsUI();
        }
    }

   public void RefreshDailyRewardsUI()
    {
        if (dailyRewardsList == null) return;

        int currentStreak = PlayerPrefs.GetInt("LoginStreak", 1);
        bool claimedToday = PlayerPrefs.GetInt("RewardClaimedToday", 0) == 1;

        for (int i = 0; i < dailyRewardsList.Length; i++)
        {
            DailyRewardVisuals rewardVisual = dailyRewardsList[i];
            if (rewardVisual.dayButton == null) continue;

            int targetDayNumber = i + 1;
            bool isAvailable = false;
            bool isAlreadyClaimed = PlayerPrefs.GetInt($"DailyReward_Claimed_Day_{targetDayNumber}", 0) == 1;

            if (targetDayNumber == currentStreak && !claimedToday && !isAlreadyClaimed)
            {
                isAvailable = true;
            }
            else if (targetDayNumber < currentStreak && !isAlreadyClaimed)
            {
                isAvailable = true; 
            }

            if (rewardVisual.checkIcon != null)
            {
                rewardVisual.checkIcon.SetActive(isAlreadyClaimed);
            }

            if (rewardVisual.glowObject != null)
            {
                rewardVisual.glowObject.SetActive(isAvailable);
            }

            rewardVisual.dayButton.interactable = isAvailable;

            int dayIndex = i;
            rewardVisual.dayButton.onClick.RemoveAllListeners();
            if (isAvailable)
            {
                rewardVisual.dayButton.onClick.AddListener(() => ClaimSpecificDailyReward(dayIndex));
            }

            Color targetColor = (isAvailable || isAlreadyClaimed) ? Color.white : new Color(0.25f, 0.25f, 0.25f, 1f);

            Image mainImg = rewardVisual.dayButton.GetComponent<Image>();
            if (mainImg != null) mainImg.color = targetColor;

            Image[] childImages = rewardVisual.dayButton.GetComponentsInChildren<Image>(true);
            foreach (Image img in childImages)
            {
                if (rewardVisual.glowObject != null && (img.gameObject == rewardVisual.glowObject || img.transform.IsChildOf(rewardVisual.glowObject.transform))) continue;
                if (rewardVisual.checkIcon != null && (img.gameObject == rewardVisual.checkIcon || img.transform.IsChildOf(rewardVisual.checkIcon.transform))) continue;
                
                img.color = targetColor;
            }

            TMP_Text[] childTexts = rewardVisual.dayButton.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in childTexts)
            {
                if (rewardVisual.glowObject != null && txt.transform.IsChildOf(rewardVisual.glowObject.transform)) continue;
                if (rewardVisual.checkIcon != null && txt.transform.IsChildOf(rewardVisual.checkIcon.transform)) continue;
                
                txt.color = targetColor;
            }
        }
    }

    public void ClaimSpecificDailyReward(int index)
    {
        if (dailyRewardsList == null || index < 0 || index >= dailyRewardsList.Length) return;

        int targetDayNumber = index + 1;
        string claimedKey = $"DailyReward_Claimed_Day_{targetDayNumber}";

        if (PlayerPrefs.GetInt(claimedKey, 0) == 1) return;

        DailyRewardVisuals currentReward = dailyRewardsList[index];

        switch (currentReward.resourceType)
        {
            case RewardResourceType.Gold:
                int currentMoney = PlayerPrefs.GetInt("Money", 0);
                PlayerPrefs.SetInt("Money", currentMoney + currentReward.amount);
                UpdateMoneyUI();
                break;

            case RewardResourceType.Dynamite:
                ShopManager shopD = FindObjectOfType<ShopManager>();
                if (shopD != null) shopD.AddItem("Dynamite", currentReward.amount);
                break;

            case RewardResourceType.IceOrb:
                ShopManager shopI = FindObjectOfType<ShopManager>();
                if (shopI != null) shopI.AddItem("IceOrb", currentReward.amount);
                break;
        }

        PlayerPrefs.SetInt(claimedKey, 1);
        
        int currentStreak = PlayerPrefs.GetInt("LoginStreak", 1);
        if (targetDayNumber == currentStreak)
        {
            PlayerPrefs.SetInt("RewardClaimedToday", 1);
        }
        
        PlayerPrefs.Save();

        PlayUI(questClaimClip);

        if (rewardAvailableBubble != null)
        {
            rewardAvailableBubble.SetActive(false); 
            rewardGlowObject.SetActive(false);
        }

        RefreshDailyRewardsUI();
    }
}