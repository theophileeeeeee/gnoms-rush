using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    private bool isUpgrading = false;

    public UIManager uiManager;

    [Header("UI")]
    public GameObject typeChoicePanel;
    public GameObject modificationChoicePanel;
    public TextMeshProUGUI sellPriceText;
    public TextMeshProUGUI upgradePriceText;

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

    [Header("Costs")]
    public int electricCost = 100;
    public int soldiersCost = 75;
    public int archerCost = 50;

    public int upgradeCostLevel2 = 100;
    public int upgradeCostLevel3 = 150;

    private Node selectedNode;

    void Awake()
    {
        instance = this;
        typeChoicePanel.SetActive(false);
        modificationChoicePanel.SetActive(false);
    }

    // =========================
    // SELECTION
    // =========================

    public void SelectNode(Node node)
    {
        selectedNode = node;

        if (selectedNode.turret != null)
        {
            modificationChoicePanel.SetActive(true);
            typeChoicePanel.SetActive(false);
        }
        else
        {
            typeChoicePanel.SetActive(true);
            modificationChoicePanel.SetActive(false);
        }

        UpdatePanelPosition();
        UpdatePrices();
    }
    int CalculateRefund(Node node)
{
    if (node == null || node.turret == null) return 0;

    int baseCost = 0;

    switch (node.turretType)
    {
        case Node.TurretType.Electric: baseCost = electricCost; break;
        case Node.TurretType.Soldiers: baseCost = soldiersCost; break;
        case Node.TurretType.Archer: baseCost = archerCost; break;
    }

    int upgradeCost = 0;

    if (node.turretLevel >= 2) upgradeCost += upgradeCostLevel2;
    if (node.turretLevel >= 3) upgradeCost += upgradeCostLevel3;

    int totalSpent = baseCost + upgradeCost;

    // 70% de remboursement
    return Mathf.RoundToInt(totalSpent * 0.7f);
}
    public void DeselectNode()
    {
        selectedNode = null;
        typeChoicePanel.SetActive(false);
        modificationChoicePanel.SetActive(false);
    }
public void UpdatePrices()
{
    if (selectedNode == null || selectedNode.turret == null)
    {
        upgradePriceText.text = "";
        sellPriceText.text = "";
        return;
    }

    // UPGRADE
    if (selectedNode.turretLevel >= 3)
    {
        upgradePriceText.text = "MAX";
    }
    else
    {
        int cost = (selectedNode.turretLevel == 1) ? upgradeCostLevel2 : upgradeCostLevel3;
        upgradePriceText.text = cost.ToString();
    }

    // SELL
    int refund = CalculateRefund(selectedNode);
    sellPriceText.text = refund.ToString();
}
    public Node GetSelectedNode()
    {
    return selectedNode;
    }

    void Update()
    {
        if (selectedNode != null)
        {
            UpdatePanelPosition();
        }
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

    // Taille réelle en world units
    float halfW = (activeRect.rect.width  * activeRect.lossyScale.x) / 2f;
    float halfH = (activeRect.rect.height * activeRect.lossyScale.y) / 2f;

    float camH = Camera.main.orthographicSize;
    float camW = camH * Camera.main.aspect;
    Vector3 camPos = Camera.main.transform.position;

    float minX = camPos.x - camW + halfW;
    float maxX = camPos.x + camW - halfW;
    float minY = camPos.y - camH + halfH;
    float maxY = camPos.y + camH - halfH;

    Vector3 clampedPos = new Vector3(
        Mathf.Clamp(worldPos.x, minX, maxX),
        Mathf.Clamp(worldPos.y, minY, maxY),
        worldPos.z
    );

    typeChoicePanel.transform.position = clampedPos;
    modificationChoicePanel.transform.position = clampedPos;
}
    // =========================
    // BUILD
    // =========================

    public void BuildElectric()
    {
        if (uiManager.CurrentMoney < electricCost) return;

        Build(electricLvl1, Node.TurretType.Electric);
        uiManager.UseMoney(electricCost);
    }

    public void BuildSoldiers()
    {
        if (uiManager.CurrentMoney < soldiersCost) return;

        Build(soldiersLvl1, Node.TurretType.Soldiers);
        uiManager.UseMoney(soldiersCost);
    }

    public void BuildArcher()
    {
        if (uiManager.CurrentMoney < archerCost) return;

        Build(archerLvl1, Node.TurretType.Archer);
        uiManager.UseMoney(archerCost);
    }

    private void Build(GameObject prefab, Node.TurretType type)
    {
        if (selectedNode == null) return;

        selectedNode.BuildTurret(prefab, type);

        InitSpecialTurret(selectedNode.turret);

        typeChoicePanel.SetActive(false);
        selectedNode = null;
        UpdatePrices();
    }

    // =========================
    // UPGRADE
    // =========================

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
        Debug.Log("Niveau max atteint");
        isUpgrading = false;
        return;
    }

    int nextLevel = currentLevel + 1;

    int cost = (currentLevel == 1) ? upgradeCostLevel2 : upgradeCostLevel3;

    if (uiManager.CurrentMoney < cost)
    {
        Debug.Log("Pas assez d'argent");
        isUpgrading = false;
        return;
    }

    GameObject nextPrefab = GetNextPrefab(selectedNode.turretType, nextLevel);

    if (nextPrefab == null)
    {
        isUpgrading = false;
        return;
    }

    uiManager.UseMoney(cost);

    ReplaceTurret(nextPrefab);

    selectedNode.turretLevel = nextLevel;

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
            case Node.TurretType.Electric:
                return level == 2 ? electricLvl2 : electricLvl3;

            case Node.TurretType.Soldiers:
                return level == 2 ? soldiersLvl2 : soldiersLvl3;

            case Node.TurretType.Archer:
                return level == 2 ? archerLvl2 : archerLvl3;
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

    // =========================
    // INIT SPECIFIQUE (soldiers)
    // =========================

    void InitSpecialTurret(GameObject turret)
    {
        SoldiersTurret soldiers = turret.GetComponent<SoldiersTurret>();

        if (soldiers != null)
        {
            soldiers.Init(selectedNode.roadPosition);
        }
    }
}