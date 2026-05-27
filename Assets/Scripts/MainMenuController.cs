using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

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

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    [Range(0f, 1f)]
    public float uiVolume = 1f;
    public Slider qualitySlider;
    public GameObject shopPanel;
    public GameObject statsPanel;
    public GameObject levelsPanel;

    [Header("Progression Globale (Quêtes)")]
    public Slider completionSlider; 
    public Text completionText;     

    public Text[] moneyTexts;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Niveaux")]
    public string[] levelSceneNames;

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

    public Quest[] quests;

    private const float normalVolume = 0f;
    private const float mutedVolume = -80f;
    public AudioSource uiAudioSource;

    void Start()
    {
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 0);
        qualitySlider.value = savedQuality;
        SetQuality(savedQuality);

        ApplyMusic(PlayerPrefs.GetInt("MusicMuted", 0) == 0);
        ApplySFX(PlayerPrefs.GetInt("SFXMuted", 0) == 0);
        UpdateMoneyUI();

        SetupQuestButtons();
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

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void UpdateMoneyUI()
    {
        string money = FormatMoney(PlayerPrefs.GetInt("Money", 0));
        foreach (Text t in moneyTexts)
            t.text = money;
    }

    public void ToggleLevelsPanel()
    {
        bool opening = !levelsPanel.activeSelf;
        levelsPanel.SetActive(opening);
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
        {
            completionSlider.value = finalCompletionRatio;
        }

        if (completionText != null)
        {
            completionText.text = Mathf.RoundToInt(finalCompletionRatio * 100f) + "%";
        }
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

        foreach (Quest q in quests)
            PlayerPrefs.DeleteKey($"Quest_{q.key}_{q.goal}_Claimed");

        foreach (string scene in levelSceneNames)
            PlayerPrefs.DeleteKey("Stars_" + scene);

        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}