using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItemData
    {
        public string itemName;
        public int price;
        public Text quantityText;
        public Button buyButton;
    }

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public int amount;
    }

    [System.Serializable]
    public class ItemDatabase
    {
        public List<ItemData> items = new List<ItemData>();
    }

    public ShopItemData[] shopItems;
    public Text[] moneyTexts;
    public Text notEnoughAgentsText;
    public float notEnoughFadeDuration = 0.5f;
    public float notEnoughDisplayTime = 1.5f;
    public Vector2 notEnoughOffset = new Vector2(0f, -10f);

    [Header("Bruitages Shop")]
    public AudioClip buySuccessClip;
    public AudioClip notEnoughClip;
    [Range(0f, 1f)]
    public float shopVolume = 1f;

    public AudioSource audioSource;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    private string path;
    private Coroutine notEnoughCoroutine;

    void Awake()
    {
#if UNITY_EDITOR
        string directory = System.IO.Path.Combine(Application.dataPath, "JSON");
#else
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "JSON");
#endif
        // Création du dossier JSON s'il n'existe pas pour éviter les crashs IO
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        
        path = System.IO.Path.Combine(directory, "items.json");
        LoadInventory();
    }

    void Start()
    {
        audioSource.playOnAwake = false;

        RefreshUI();

        for (int i = 0; i < shopItems.Length; i++)
        {
            int index = i;
            shopItems[i].buyButton.onClick.AddListener(() => TryBuy(index));
        }
    }

    void PlayShop(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, shopVolume);
    }

    void LoadInventory()
    {
        inventory.Clear();

        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        if (string.IsNullOrEmpty(json)) return;

        ItemDatabase db = JsonUtility.FromJson<ItemDatabase>(json);
        if (db == null || db.items == null) return;

        foreach (var item in db.items)
            inventory[item.itemName] = item.amount;
    }

    void SaveInventory()
    {
        ItemDatabase db = new ItemDatabase();
        foreach (var item in inventory)
            db.items.Add(new ItemData { itemName = item.Key, amount = item.Value });

        File.WriteAllText(path, JsonUtility.ToJson(db, true));

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    void TryBuy(int index)
    {
        ShopItemData item = shopItems[index];
        int currentMoney = PlayerPrefs.GetInt("Money", 0);

        if (currentMoney < item.price)
        {
            PlayShop(notEnoughClip);
            if (notEnoughCoroutine != null) StopCoroutine(notEnoughCoroutine);
            notEnoughCoroutine = StartCoroutine(NotEnoughRoutine(shopItems[index].buyButton.GetComponent<RectTransform>()));
            return;
        }

        // Déduction de l'argent actuel
        PlayerPrefs.SetInt("Money", currentMoney - item.price);
        
        // --- LE TRUC DÉPENSIER ---
        // On récupère le total dépensé jusqu'ici, et on ajoute le prix de l'objet
        int totalSpent = PlayerPrefs.GetInt("GoldSpentShop", 0);
        PlayerPrefs.SetInt("GoldSpentShop", totalSpent + item.price);
        
        // Sauvegarde des PlayerPrefs
        PlayerPrefs.Save();

        // Ajout à l'inventaire JSON
        if (!inventory.ContainsKey(item.itemName))
            inventory[item.itemName] = 0;
        inventory[item.itemName]++;

        SaveInventory();
        PlayShop(buySuccessClip);
        RefreshUI();
    }

    IEnumerator NotEnoughRoutine(RectTransform buttonRT)
    {
        RectTransform rt = notEnoughAgentsText.GetComponent<RectTransform>();

        float buttonHalfHeight = buttonRT.rect.height * 0.5f;
        float textHalfHeight = rt.rect.height * 0.5f;
        rt.anchoredPosition = buttonRT.anchoredPosition + new Vector2(notEnoughOffset.x, -buttonHalfHeight - textHalfHeight + notEnoughOffset.y);

        Color c = notEnoughAgentsText.color;
        c.a = 0f;
        notEnoughAgentsText.color = c;
        notEnoughAgentsText.gameObject.SetActive(true);

        float t = 0f;
        while (t < notEnoughFadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / notEnoughFadeDuration);
            notEnoughAgentsText.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(notEnoughDisplayTime);

        t = 0f;
        while (t < notEnoughFadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - t / notEnoughFadeDuration);
            notEnoughAgentsText.color = c;
            yield return null;
        }

        notEnoughAgentsText.gameObject.SetActive(false);
        notEnoughCoroutine = null;
    }

    void RefreshUI()
    {
        int money = PlayerPrefs.GetInt("Money", 0);

        foreach (var t in moneyTexts)
            t.text = FormatMoney(money);

        foreach (var item in shopItems)
            item.quantityText.text = inventory.TryGetValue(item.itemName, out int qty) ? qty.ToString() : "0";
    }

    string FormatMoney(int amount)
    {
        if (amount >= 1000000) return $"{amount / 1000000f:0.#}M";
        if (amount >= 1000)    return $"{amount / 1000f:0.#}K";
        return amount.ToString();
    }
}