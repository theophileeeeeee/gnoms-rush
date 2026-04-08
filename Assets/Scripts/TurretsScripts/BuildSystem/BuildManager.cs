using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    public UIManager uiManager;
    [Header("UI")]
    public GameObject typeChoicePanel;
    public GameObject modificationChoicePanel;
    

    [Header("Turrets")]
    public GameObject electricTurret;
    public GameObject soldiersTurret;

    public GameObject archerTurret;

    private Node selectedNode;

    void Awake()
    {
        instance = this;
        typeChoicePanel.SetActive(false);
    }

    public void SelectNode(Node node)
    {
        selectedNode = node;
        if (selectedNode.turret != null)
        {
           modificationChoicePanel.SetActive(true);
        }
        else{
        typeChoicePanel.SetActive(true);
        }

        UpdatePanelPosition();
    }

    void Update()
    {
        if (selectedNode != null)
        {
            UpdatePanelPosition();
        }
    }
    public void DeselectNode()
{
    selectedNode = null;
    typeChoicePanel.SetActive(false);
}

    public void UpdateSoldiersPosition()
    {
        switch(selectedNode.roadPosition)
        {
            case RoadPosition.Haut : break;
            case RoadPosition.Bas : break;
            case RoadPosition.Droite : break;
            case RoadPosition.Gauche : break;
        }
    }


    private void UpdatePanelPosition()
    {
        // Position légèrement au-dessus du node
        Vector3 offset = new Vector3(0, 1f, 0); // ajuste Y selon ton jeu
        typeChoicePanel.transform.position = selectedNode.transform.position + offset;

        // Optionnel : faire face à la caméra
        typeChoicePanel.transform.rotation = Quaternion.identity; // ou Camera.main.transform.rotation
    }

    public void BuildElectric()
    {
        if(uiManager.CurrentMoney < 100)
        {
            Debug.Log("Pas assez d'argent pour construire ce bâtiment !");
            return;
        }
        Build(electricTurret);
        uiManager.UseMoney(100);
    }
    public void BuildSoldiers()
    {
        if(uiManager.CurrentMoney < 75)
        {
            Debug.Log("Pas assez d'argent pour construire ce bâtiment !");
            return;
        }
        Build(soldiersTurret);
        uiManager.UseMoney(75);
    }
    public void BuildArcher()
    {
        if(uiManager.CurrentMoney < 75)
        {
            Debug.Log("Pas assez d'argent pour construire ce bâtiment !");
            return;
        }
        Build(archerTurret);
        uiManager.UseMoney(50);
    }




private void Build(GameObject turretPrefab)
{
    if (selectedNode == null) return;

    selectedNode.BuildTurret(turretPrefab);
    
    SoldiersTurret soldiersTurretComp = selectedNode.turret?.GetComponent<SoldiersTurret>();
    if (soldiersTurretComp != null)
    {
        soldiersTurretComp.Init(selectedNode.roadPosition);
    }

    typeChoicePanel.SetActive(false);
    selectedNode = null;
}
}
