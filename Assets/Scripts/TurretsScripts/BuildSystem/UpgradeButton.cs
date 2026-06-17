using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public void ExecuteUpgrade()
    {
        if (BuildManager.instance != null)
        {
            BuildManager.instance.UpgradeTurret();
        }
    }
}