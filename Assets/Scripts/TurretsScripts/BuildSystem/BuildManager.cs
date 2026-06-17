using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    public static event System.Action<Node> OnTurretBuilt;
    private bool isUpgrading = false;

    public UIManager uiManager;

    [Header("UI Canvas Parent")]
    public Transform canvasTransform;

    [Header("UI Prefabs")]
    public GameObject typeChoicePrefab;
    public GameObject modificationChoicePrefab;

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
    public AudioSource audioSource;
    private GameObject currentPanelInstance;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void SelectNode(Node node)
    {
        if (selectedNode == node) return;
        if (currentPanelInstance != null)
            Destroy(currentPanelInstance);

        selectedNode = node;
        Debug.Log("[BuildManager] Node sélectionnée : " + node.name);

        if (selectNodeSound != null)
            audioSource.PlayOneShot(selectNodeSound, volume);

        GameObject prefabToSpawn = (selectedNode.turret != null) ? modificationChoicePrefab : typeChoicePrefab;
        if (prefabToSpawn != null && canvasTransform != null)
            currentPanelInstance = Instantiate(prefabToSpawn, canvasTransform);

        UpdatePanelPosition();
        UpdatePrices();
    }

    public void DeselectNode()
    {
        if (selectedNode == null) return;

        Debug.Log("[BuildManager] Désélection de la Node : " + selectedNode.name);

        if (deselectNodeSound != null)
            audioSource.PlayOneShot(deselectNodeSound, volume);

        selectedNode = null;

        if (currentPanelInstance != null)
        {
            Animator anim = currentPanelInstance.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("TriggerClose");

            StartCoroutine(WaitAndDestroyPanel(currentPanelInstance, 0.15f));
            currentPanelInstance = null;
        }
    }

    private IEnumerator WaitAndDestroyPanel(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null)
            Destroy(panel);
    }

    public void UpdatePrices()
    {
        if (currentPanelInstance == null) return;

        PanelReferences panelRefs = currentPanelInstance.GetComponent<PanelReferences>();
        if (panelRefs == null) return;

        TextMeshProUGUI activeUpgradeText = panelRefs.upgradePriceText;
        TextMeshProUGUI activeSellText = panelRefs.sellPriceText;

        if (selectedNode == null || selectedNode.turret == null)
        {
            if (activeUpgradeText != null) activeUpgradeText.text = "";
            if (activeSellText != null) activeSellText.text = "";
            return;
        }

        if (activeUpgradeText != null)
        {
            if (selectedNode.turretLevel >= 3)
                activeUpgradeText.text = "MAX";
            else
                activeUpgradeText.text = ((selectedNode.turretLevel == 1) ? upgradeCostLevel2 : upgradeCostLevel3).ToString();
        }

        if (activeSellText != null)
            activeSellText.text = CalculateRefund(selectedNode).ToString();
    }

    private void UpdatePanelPosition()
    {
        if (currentPanelInstance == null || selectedNode == null) return;

        RectTransform activeRect = currentPanelInstance.GetComponent<RectTransform>();
        if (activeRect == null) return;

        float halfW = (activeRect.rect.width * activeRect.lossyScale.x) / 2f;
        float halfH = (activeRect.rect.height * activeRect.lossyScale.y) / 2f;

        float verticalOffset = selectedNode.spawnAbove ? 1f : -halfH;
        Vector3 worldPos = selectedNode.transform.position + new Vector3(0, verticalOffset, 0);

        currentPanelInstance.transform.rotation = Camera.main.transform.rotation;

        float camH = Camera.main.orthographicSize;
        float camW = camH * Camera.main.aspect;
        Vector3 camPos = Camera.main.transform.position;

        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(worldPos.x, camPos.x - camW + halfW, camPos.x + camW - halfW),
            Mathf.Clamp(worldPos.y, camPos.y - camH + halfH, camPos.y + camH - halfH),
            worldPos.z
        );

        currentPanelInstance.transform.position = clampedPos;
    }

    public void BuildElectric()
    {
        Debug.Log("[BuildManager] Clic reçu : BuildElectric");
        Build(electricLvl1, Node.TurretType.Electric, electricCost);
    }

    public void BuildSoldiers()
    {
        Debug.Log("[BuildManager] Clic reçu : BuildSoldiers");
        Build(soldiersLvl1, Node.TurretType.Soldiers, soldiersCost);
    }

    public void BuildBomb()
    {
        Debug.Log("[BuildManager] Clic reçu : BuildBomb");
        Build(bombLvl1, Node.TurretType.Bomb, bombCost);
    }

    public void BuildArcher()
    {
        Debug.Log("[BuildManager] Clic reçu : BuildArcher");
        Build(archerLvl1, Node.TurretType.Archer, archerCost);
    }

    private void UpdateTowerCount()
    {
        PlayerPrefs.SetInt("TowersBuilt", PlayerPrefs.GetInt("TowersBuilt", 0) + 1);
        PlayerPrefs.Save();
    }

    private void Build(GameObject prefab, Node.TurretType type, int cost)
    {
        if (selectedNode == null)
        {
            Debug.LogError("[BuildManager] ERREUR : La fonction Build a été coupée car 'selectedNode' est NULL !");
            return;
        }

        if (uiManager.CurrentMoney < cost)
        {
            Debug.LogWarning("[BuildManager] Construction annulée : Argent insuffisant ! Requis: " + cost + ", Actuel: " + uiManager.CurrentMoney);
            return;
        }

        Debug.Log("[BuildManager] Succès : Lancement du build sur la node " + selectedNode.name + ". Coût : " + cost);

        uiManager.UseMoney(cost);
        UpdateTowerCount();

        Node builtNode = selectedNode;
        selectedNode.BuildTurret(prefab, type);
        InitSpecialTurret(selectedNode.turret);

        OnTurretBuilt?.Invoke(builtNode);

        if (buildSound != null)
            audioSource.PlayOneShot(buildSound, volume);

        if (currentPanelInstance != null)
            Destroy(currentPanelInstance);

        selectedNode = null;
        UpdatePrices();
    }

    public void UpgradeTurret()
    {
        if (isUpgrading) return;
        isUpgrading = true;

        if (selectedNode == null || selectedNode.turret == null) { isUpgrading = false; return; }

        int currentLevel = selectedNode.turretLevel;
        if (currentLevel >= 3) { isUpgrading = false; return; }

        int cost = (currentLevel == 1) ? upgradeCostLevel2 : upgradeCostLevel3;
        if (uiManager.CurrentMoney < cost) { isUpgrading = false; return; }

        GameObject nextPrefab = GetNextPrefab(selectedNode.turretType, currentLevel + 1);
        if (nextPrefab == null) { isUpgrading = false; return; }

        uiManager.UseMoney(cost);
        ReplaceTurret(nextPrefab);

        if (buildSound != null)
            audioSource.PlayOneShot(buildSound, volume);

        selectedNode.turretLevel = currentLevel + 1;

        if (currentPanelInstance != null)
        {
            Animator anim = currentPanelInstance.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("TriggerClose");

            StartCoroutine(WaitAndDestroyPanel(currentPanelInstance, 0.15f));
            currentPanelInstance = null;
        }

        selectedNode = null;
        UpdatePrices();
        Invoke(nameof(ResetUpgradeLock), 0.1f);
    }

    void ResetUpgradeLock() => isUpgrading = false;

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

    public void PlayDeleteSound() 
    {
        if (deleteSound != null)
            audioSource.PlayOneShot(deleteSound, volume);
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

    public Node GetSelectedNode() => selectedNode;

    void Update()
    {
        if (selectedNode != null)
            UpdatePanelPosition();
    }
}