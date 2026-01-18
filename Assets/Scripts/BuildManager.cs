using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    [Header("UI")]
    public GameObject typeChoicePanel;

    [Header("Turrets")]
    public GameObject electricTurret;
    public GameObject soldiersTurret;

    private Node selectedNode;

    void Awake()
    {
        instance = this;
        typeChoicePanel.SetActive(false);
    }

    public void SelectNode(Node node)
    {
        selectedNode = node;

        typeChoicePanel.SetActive(true);

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
        Build(electricTurret);
    }
    public void BuildSoldiers()
    {
        Build(soldiersTurret);
    }

    private void Build(GameObject turretPrefab)
    {
        if (selectedNode == null) return;

        selectedNode.BuildTurret(turretPrefab);
        typeChoicePanel.SetActive(false);
        selectedNode = null;
    }
}
