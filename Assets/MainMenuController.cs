using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Quest
{
    public string key;
    public int goal;
    public int reward;
    public Text progressText;
    public Button claimButton;
}

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider qualitySlider;
    public Toggle soundToggle;
    public GameObject shopPanel;
    public GameObject statsPanel;
    public Text towerBuiltText;
    public Text killsText;
    public Text[] moneyTexts;

    public Quest[] quests;

    private readonly string[] qualityLabels = { "Bas", "Moyen", "Haut" };

    void Start()
    {
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 0);
        qualitySlider.value = savedQuality;
        SetQuality(savedQuality);

        bool savedSound = PlayerPrefs.GetInt("Sound", 1) == 1;
        soundToggle.isOn = savedSound;
        ToggleSound(savedSound);
        UpdateMoneyUI();
    }
string FormatMoney(int amount)
{
    if (amount >= 1000000) return $"{amount / 1000000f:0.#}M";
    if (amount >= 1000)    return $"{amount / 1000f:0.#}K";
    return amount.ToString();
}

void UpdateMoneyUI()
{
    string money = FormatMoney(PlayerPrefs.GetInt("Money", 0));
    foreach (Text t in moneyTexts)
        t.text = money;
}

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }

    public void ToggleStats()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
        towerBuiltText.text = PlayerPrefs.GetInt("TowersBuilt", 0).ToString();
        killsText.text = PlayerPrefs.GetInt("EnemiesKilled", 0).ToString();
        UpdateQuests();
    }

void UpdateQuests()
{
    for (int i = 0; i < quests.Length; i++)
    {
        Quest q = quests[i];
        int progress = PlayerPrefs.GetInt(q.key, 0);
        bool completed = progress >= q.goal;
        bool claimed = PlayerPrefs.GetInt($"Quest_{q.key}_{q.goal}_Claimed", 0) == 1;
        q.progressText.color = completed ? Color.green : Color.white;
        q.claimButton.interactable = completed && !claimed;
        q.claimButton.gameObject.SetActive(!claimed);

        int index = i;
        q.claimButton.onClick.RemoveAllListeners();
        q.claimButton.onClick.AddListener(() => ClaimReward(index));
    }
}

    public void ClaimReward(int questIndex)
    {
        Quest q = quests[questIndex];
        string claimedKey = $"Quest_{q.key}_{q.goal}_Claimed";

        if (PlayerPrefs.GetInt(claimedKey, 0) == 1) return;

        int currentMoney = PlayerPrefs.GetInt("Money", 0);
        PlayerPrefs.SetInt("Money", currentMoney + q.reward);
        PlayerPrefs.SetInt(claimedKey, 1);
        PlayerPrefs.Save();
        UpdateMoneyUI();
        q.claimButton.gameObject.SetActive(false);
    }

    public void ResetStats()
    {
        PlayerPrefs.DeleteKey("TowersBuilt");
        PlayerPrefs.DeleteKey("EnemiesKilled");
        towerBuiltText.text = "0";
        killsText.text = "0";

        foreach (Quest q in quests)
        {
            PlayerPrefs.DeleteKey($"Quest_{q.key}_{q.goal}_Claimed");
        }

        PlayerPrefs.Save();
        UpdateQuests();
    }

    public void SetQuality(float qualityIndex)
    {
        int[] qualityMap = { 0, 2, 5 };
        int level = Mathf.Clamp(Mathf.RoundToInt(qualityIndex), 0, 2);
        QualitySettings.SetQualityLevel(qualityMap[level]);
        PlayerPrefs.SetInt("QualityLevel", level);
        PlayerPrefs.Save();
    }

    public void ToggleSound(bool isOn)
    {
        AudioListener.volume = isOn ? 1 : 0;
        PlayerPrefs.SetInt("Sound", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}