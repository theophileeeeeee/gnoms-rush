using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class ItemManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemUI
    {
        public string itemName;
        public TextMeshProUGUI text;
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

    public List<ItemUI> itemUIs = new List<ItemUI>();

    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    private string path;

    void Awake()
    {
#if UNITY_EDITOR
        string directory = System.IO.Path.Combine(Application.dataPath, "JSON");
#else
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "JSON");
#endif

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        path = System.IO.Path.Combine(directory, "items.json");

        Debug.Log("Items path: " + path);

        LoadItems();
        RefreshUI();
    }

    public void AddItem(string itemName, int amount)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        if (!inventory.ContainsKey(itemName))
            inventory[itemName] = 0;

        inventory[itemName] += amount;

        SaveItems();
        RefreshUI();
    }

    public bool UseItem(string itemName, int amount)
    {
        if (string.IsNullOrEmpty(itemName)) return false;

        if (!inventory.ContainsKey(itemName) || inventory[itemName] < amount)
            return false;

        inventory[itemName] -= amount;

        SaveItems();
        RefreshUI();
        return true;
    }

    public int GetItemAmount(string itemName)
    {
        return inventory.TryGetValue(itemName, out int value) ? value : 0;
    }

    void SaveItems()
    {
        ItemDatabase db = new ItemDatabase();

        foreach (var item in inventory)
        {
            db.items.Add(new ItemData
            {
                itemName = item.Key,
                amount = item.Value
            });
        }

        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(path, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    void LoadItems()
    {
        inventory.Clear();

        if (!File.Exists(path))
        {
            Debug.Log("Aucun fichier → création");
            SaveItems();
            return;
        }

        string json = File.ReadAllText(path);

        if (string.IsNullOrEmpty(json)) return;

        ItemDatabase db = JsonUtility.FromJson<ItemDatabase>(json);

        if (db == null || db.items == null) return;

        foreach (var item in db.items)
        {
            inventory[item.itemName] = item.amount;
        }
    }

    void RefreshUI()
    {
        foreach (var ui in itemUIs)
        {
            if (ui.text != null)
                ui.text.text = GetItemAmount(ui.itemName).ToString();
        }
    }
}