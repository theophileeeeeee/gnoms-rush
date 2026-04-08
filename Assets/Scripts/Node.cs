using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject turret;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public Vector3 positionOffset;

    public RoadPosition roadPosition;
    void OnMouseDown()
    {
        if (!pausePanel.activeSelf && !gameOverPanel.activeSelf && !victoryPanel.activeSelf)
        {
            BuildManager.instance.SelectNode(this);
        }
    }

    public void BuildTurret(GameObject turretPrefab)
    {
        if (turret != null)
        {
            Debug.Log("Node déjà occupé");
            Debug.Log("turret : " + turret);
            return;
        }

        turret = Instantiate(
            turretPrefab,
            transform.position + positionOffset,
            transform.rotation
        );
    }
}
public enum RoadPosition
{
    Haut,
    Bas,
    Droite,
    Gauche
}