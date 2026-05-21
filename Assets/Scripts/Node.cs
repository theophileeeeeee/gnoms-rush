using UnityEngine;

public class Node : MonoBehaviour
{
    public BuildManager buildManager;
    public UIManager uiManager;
    [Header("Turret")]
    public GameObject turret;

    public enum TurretType { None, Electric, Soldiers, Archer, Bomb }
    public TurretType turretType = TurretType.None;

    public int turretLevel = 0;

    [Header("UI Blocks")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Placement")]
    public Vector3 positionOffset;
    public RoadPosition roadPosition;

    void OnMouseDown()
    {
        if (!pausePanel.activeSelf && !gameOverPanel.activeSelf && !victoryPanel.activeSelf)
        {
            BuildManager.instance.SelectNode(this);
        }
    }

    // =========================
    // BUILD
    // =========================

    public void BuildTurret(GameObject turretPrefab, TurretType type)
    {
        if (turret != null)
        {
            Debug.Log("Node déjà occupé");
            return;
        }

        turret = Instantiate(
            turretPrefab,
            transform.position + positionOffset,
            transform.rotation
        );

        turretType = type;
        turretLevel = 1;
    }
public int ClearNode()
{
    if (turret == null) return 0;

    Destroy(turret);

    int level = turretLevel;
    TurretType type = turretType;

    turret = null;
    turretType = TurretType.None;
    turretLevel = 0;

    return CalculateRefund(type, level);
}
int CalculateRefund(TurretType type, int level)
{
    int baseCost = 0;

    switch (type)
    {
        case TurretType.Electric: baseCost = BuildManager.instance.electricCost; break;
        case TurretType.Soldiers: baseCost = BuildManager.instance.soldiersCost; break;
        case TurretType.Archer: baseCost = BuildManager.instance.archerCost; break;
        case TurretType.Bomb: baseCost = BuildManager.instance.bombCost; break;
    }

    int upgradeCost = 0;

    if (level >= 2) upgradeCost += BuildManager.instance.upgradeCostLevel2;
    if (level >= 3) upgradeCost += BuildManager.instance.upgradeCostLevel3;

    int totalSpent = baseCost + upgradeCost;

    // Exemple : remboursement 70%
    return Mathf.RoundToInt(totalSpent * 0.7f);
}
}

public enum RoadPosition
{
    Haut,
    Bas,
    Droite,
    Gauche
}