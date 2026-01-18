using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject turret;
    public Vector3 positionOffset;

    void OnMouseDown()
    {
        BuildManager.instance.SelectNode(this);
    }

    public void BuildTurret(GameObject turretPrefab)
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
    }
}
