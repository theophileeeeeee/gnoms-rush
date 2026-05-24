using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    private bool isUpgrading = false;

    public UIManager uiManager;
    public Animator panelTypeChoiceAnimator;
    public Animator panelModificationChoiceAnimator;

    [Header("UI")]
    public GameObject typeChoicePanel;
    public GameObject modificationChoicePanel;
    public TextMeshProUGUI sellPriceText;
    public TextMeshProUGUI upgradePriceText;

    [Header("Audio")]
    public AudioClip buildSound;
    public AudioClip selectNodeSound;
    public AudioClip deselectNodeSound;
    public AudioClip deleteSound;
    public float volume = 1f;

    [Header("Electric Turret Levels")]
    public GameObject electricLvl1;
    public GameObject electricLvl2;
    public GameObject electricLvl3;

    [Header("Soldiers Turret Levels")]
    public GameObject soldiersLvl1;
    public GameObject soldiersLvl2;
    public GameObject soldiersLvl3;

    [Header("Archer Turret Levels")]
    public GameObject archerLvl1;
    public GameObject archerLvl2;
    public GameObject archerLvl3;

    [Header("Bomb Turret Levels")]
    public GameObject bombLvl1;
    public GameObject bombLvl2;
    public GameObject bombLvl3;

    [Header("Costs")]
    public int electricCost = 100;
    public int soldiersCost = 75;
    public int archerCost = 50;
    public int bombCost = 125;
    public int upgradeCostLevel2 = 100;
    public int upgradeCostLevel3 = 150;

    private Node selectedNode;
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
        typeChoicePanel.SetActive(false);
        modificationChoicePanel.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void SelectNode(Node node)
    {
        selectedNode = node;

        if (selectNodeSound != null)
            audioSource.PlayOneShot(selectNodeSound, volume);

        if (selectedNode.turret != null)
        {
            modificationChoicePanel.SetActive(true);
            panelModificationChoiceAnimator.SetBool("Open", true);
        }
        else
        {
            typeChoicePanel.SetActive(true);
            panelTypeChoiceAnimator.SetBool("Open", true);
        }

        UpdatePanelPosition();
        UpdatePrices();
    }

    IEnumerator WaitForPanelTypeAnimation()
    {
        yield return new WaitForSeconds(0.16f);
        typeChoicePanel.SetActive(false);
    }
    public void PlayDeleteSound()
    {
        if (deleteSound != null)
            audioSource.PlayOneShot(deleteSound, volume);
    }

    IEnumerator WaitForPanelModificationAnimation()
    {
        yield return new WaitForSeconds(0.16f);
        modificationChoicePanel.SetActive(false);
    }

    int CalculateRefund(Node node)
    {
        if (node == null || node.turret == null) return 0;

        int baseCost = 0;

        switch (node.turretType)
        {
            case Node.TurretType.Electric: baseCost = electricCost; break;
            case Node.TurretType.Soldiers: baseCost = soldiersCost; break;
            case Node.TurretType.Archer:   baseCost = archerCost;   break;
            case Node.TurretType.Bomb:     baseCost = bombCost;     break;
        }

        int upgradeCost = 0;
        if (node.turretLevel >= 2) upgradeCost += upgradeCostLevel2;
        if (node.turretLevel >= 3) upgradeCost += upgradeCostLevel3;

        return Mathf.RoundToInt((baseCost + upgradeCost) * 0.7f);
    }

public void DeselectNode()
    {
        if (selectedNode == null) return;

        if (deselectNodeSound != null)
            audioSource.PlayOneShot(deselectNodeSound, volume);

        selectedNode = null;
        panelTypeChoiceAnimator.SetBool("Open", false);
        StartCoroutine(WaitForPanelTypeAnimation());
        panelModificationChoiceAnimator.SetBool("Open", false);
        StartCoroutine(WaitForPanelModificationAnimation());
    }

    public void UpdatePrices()
    {
        if (selectedNode == null || selectedNode.turret == null)
        {
            upgradePriceText.text = "";
            sellPriceText.text = "";
            return;
        }

        if (selectedNode.turretLevel >= 3)
        {
            upgradePriceText.text = "MAX";
        }
        else
        {
            int cost = (selectedNode.turretLevel == 1) ? upgradeCostLevel2 : upgradeCostLevel3;
            upgradePriceText.text = cost.ToString();
        }

        sellPriceText.text = CalculateRefund(selectedNode).ToString();
    }

    public Node GetSelectedNode()
    {
        return selectedNode;
    }

    void Update()
    {
        if (selectedNode != null)
            UpdatePanelPosition();
    }

    private void UpdatePanelPosition()
    {
        Vector3 offset = new Vector3(0, 1f, 0);
        Vector3 worldPos = selectedNode.transform.position + offset;

        typeChoicePanel.transform.rotation = Camera.main.transform.rotation;
        modificationChoicePanel.transform.rotation = Camera.main.transform.rotation;

        RectTransform activeRect = selectedNode.turret != null
            ? modificationChoicePanel.GetComponent<RectTransform>()
            : typeChoicePanel.GetComponent<RectTransform>();

        float halfW = (activeRect.rect.width  * activeRect.lossyScale.x) / 2f;
        float halfH = (activeRect.rect.height * activeRect.lossyScale.y) / 2f;

        float camH = Camera.main.orthographicSize;
        float camW = camH * Camera.main.aspect;
        Vector3 camPos = Camera.main.transform.position;

        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(worldPos.x, camPos.x - camW + halfW, camPos.x + camW - halfW),
            Mathf.Clamp(worldPos.y, camPos.y - camH + halfH, camPos.y + camH - halfH),
            worldPos.z
        );

        typeChoicePanel.transform.position = clampedPos;
        modificationChoicePanel.transform.position = clampedPos;
    }

    public void BuildElectric()
    {
        if (uiManager.CurrentMoney < electricCost) return;
        Build(electricLvl1, Node.TurretType.Electric);
        uiManager.UseMoney(electricCost);
        PlayerPrefs.SetInt("TowersBuilt", PlayerPrefs.GetInt("TowersBuilt", 0) + 1);
        PlayerPrefs.Save();
    }

    public void BuildSoldiers()
    {
        if (uiManager.CurrentMoney < soldiersCost) return;
        Build(soldiersLvl1, Node.TurretType.Soldiers);
        uiManager.UseMoney(soldiersCost);
        PlayerPrefs.SetInt("TowersBuilt", PlayerPrefs.GetInt("TowersBuilt", 0) + 1);
        PlayerPrefs.Save();
    }

    public void BuildBomb()
    {
        if (uiManager.CurrentMoney < bombCost) return;
        Build(bombLvl1, Node.TurretType.Bomb);
        uiManager.UseMoney(bombCost);
        PlayerPrefs.SetInt("TowersBuilt", PlayerPrefs.GetInt("TowersBuilt", 0) + 1);
        PlayerPrefs.Save();
    }

    public void BuildArcher()
    {
        if (uiManager.CurrentMoney < archerCost) return;
        Build(archerLvl1, Node.TurretType.Archer);
        uiManager.UseMoney(archerCost);
        PlayerPrefs.SetInt("TowersBuilt", PlayerPrefs.GetInt("TowersBuilt", 0) + 1);
        PlayerPrefs.Save();
    }

    private void Build(GameObject prefab, Node.TurretType type)
    {
        if (selectedNode == null) return;

        selectedNode.BuildTurret(prefab, type);
        InitSpecialTurret(selectedNode.turret);

        if (buildSound != null)
            audioSource.PlayOneShot(buildSound, volume);

        typeChoicePanel.SetActive(false);
        selectedNode = null;
        UpdatePrices();
    }

    public void UpgradeTurret()
    {
        if (isUpgrading) return;
        isUpgrading = true;

        if (selectedNode == null || selectedNode.turret == null)
        {
            isUpgrading = false;
            return;
        }

        int currentLevel = selectedNode.turretLevel;

        if (currentLevel >= 3)
        {
            isUpgrading = false;
            return;
        }

        int cost = (currentLevel == 1) ? upgradeCostLevel2 : upgradeCostLevel3;

        if (uiManager.CurrentMoney < cost)
        {
            isUpgrading = false;
            return;
        }

        GameObject nextPrefab = GetNextPrefab(selectedNode.turretType, currentLevel + 1);

        if (nextPrefab == null)
        {
            isUpgrading = false;
            return;
        }

        uiManager.UseMoney(cost);
        ReplaceTurret(nextPrefab);

        if (buildSound != null)
            audioSource.PlayOneShot(buildSound, volume);

        selectedNode.turretLevel = currentLevel + 1;
        UpdatePrices();
        Invoke(nameof(ResetUpgradeLock), 0.1f);
    }

    void ResetUpgradeLock()
    {
        isUpgrading = false;
    }

    GameObject GetNextPrefab(Node.TurretType type, int level)
    {
        switch (type)
        {
            case Node.TurretType.Electric: return level == 2 ? electricLvl2 : electricLvl3;
            case Node.TurretType.Soldiers: return level == 2 ? soldiersLvl2 : soldiersLvl3;
            case Node.TurretType.Archer:   return level == 2 ? archerLvl2   : archerLvl3;
            case Node.TurretType.Bomb:     return level == 2 ? bombLvl2     : bombLvl3;
        }
        return null;
    }

    void ReplaceTurret(GameObject newPrefab)
    {
        Vector3 pos = selectedNode.turret.transform.position;
        Quaternion rot = selectedNode.turret.transform.rotation;

        Destroy(selectedNode.turret);
        selectedNode.turret = Instantiate(newPrefab, pos, rot);
        InitSpecialTurret(selectedNode.turret);
    }

    void InitSpecialTurret(GameObject turret)
    {
        SoldiersTurret soldiers = turret.GetComponent<SoldiersTurret>();
        if (soldiers != null)
            soldiers.Init(selectedNode.roadPosition);
    }
}