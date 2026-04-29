using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    void OnMouseDown()
    {
        BuildManager.instance.UpgradeTurret();
    }
}
